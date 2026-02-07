using System.Diagnostics;
using System.Globalization;
using System.Text;
using JinChanChan.Core.Abstractions;
using JinChanChan.Core.Models;
using JinChanChan.Ocr.Models;
using JinChanChan.Ocr.Utilities;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace JinChanChan.Ocr.Engines;

public sealed class OnnxPPOcrEngine : IOcrEngine, IDisposable
{
    private readonly OnnxPPOcrOptions _options;
    private readonly Action<string>? _logger;
    private readonly object _sessionLock = new();
    private InferenceSession? _recognizerSession;
    private IReadOnlyList<string> _dictionary = Array.Empty<string>();
    private string _provider = "CPU";
    private bool _usedFallback;
    private string _note = "OCR 初始化完成。";

    public OnnxPPOcrEngine(OnnxPPOcrOptions options, Action<string>? logger = null)
    {
        _options = options;
        _logger = logger;
        Initialize();
    }

    public async Task<OcrBatchResult> RecognizeAsync(IReadOnlyList<FrameImage> frames, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            Stopwatch sw = Stopwatch.StartNew();
            int frameCount = frames?.Count ?? 0;
            string[] values = new string[frameCount];

            for (int i = 0; i < frameCount; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                values[i] = RecognizeFrameSafe(frames[i], cancellationToken);
            }

            sw.Stop();
            return new OcrBatchResult
            {
                RawTexts = values,
                Provider = _provider,
                UsedFallbackProvider = _usedFallback,
                Elapsed = sw.Elapsed,
                Note = _recognizerSession == null
                    ? "未加载识别模型，返回空识别结果。"
                    : _note
            };
        }, cancellationToken);
    }

    private void Initialize()
    {
        if (!File.Exists(_options.RecognizerModelPath))
        {
            _logger?.Invoke($"OCR模型不存在: {_options.RecognizerModelPath}");
            return;
        }

        SessionOptions sessionOptions = new();
        sessionOptions.InterOpNumThreads = 1;
        sessionOptions.IntraOpNumThreads = Math.Max(1, _options.IntraOpNumThreads);

        try
        {
            _provider = OnnxProviderSelector.ConfigureProvider(sessionOptions, _options.PreferredProvider, _options.AllowProviderFallback);
            _usedFallback = _options.PreferredProvider == OnnxExecutionProvider.DirectMl && _provider != "DirectML";
            _recognizerSession = new InferenceSession(_options.RecognizerModelPath, sessionOptions);
            _dictionary = LoadDictionary();
            _note = _dictionary.Count > 0 ? "使用字典CTC解码。" : "字典缺失，使用索引解码。";
        }
        catch (Exception ex)
        {
            _logger?.Invoke($"初始化 OCR provider 失败，回退 CPU: {ex.Message}");
            _provider = "CPU";
            _usedFallback = true;

            SessionOptions cpuOptions = new();
            cpuOptions.InterOpNumThreads = 1;
            cpuOptions.IntraOpNumThreads = Math.Max(1, _options.IntraOpNumThreads);
            _recognizerSession = new InferenceSession(_options.RecognizerModelPath, cpuOptions);
            _dictionary = LoadDictionary();
            _note = "Provider 回退 CPU，使用CTC解码。";
        }
    }

    public void Dispose()
    {
        _recognizerSession?.Dispose();
    }

    private string RecognizeFrameSafe(FrameImage frame, CancellationToken cancellationToken)
    {
        try
        {
            return RecognizeFrame(frame, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.Invoke($"OCR单帧识别失败: {ex.Message}");
            return string.Empty;
        }
    }

    private string RecognizeFrame(FrameImage frame, CancellationToken cancellationToken)
    {
        if (_recognizerSession == null || frame.Width <= 0 || frame.Height <= 0 || frame.Pixels.Length == 0)
        {
            return string.Empty;
        }

        cancellationToken.ThrowIfCancellationRequested();

        (string inputName, int inputHeight, int inputWidth) = ResolveInputShape(_recognizerSession);
        DenseTensor<float> input = BuildInputTensor(frame, inputHeight, inputWidth);
        List<NamedOnnxValue> feed = [NamedOnnxValue.CreateFromTensor(inputName, input)];

        IDisposableReadOnlyCollection<DisposableNamedOnnxValue> outputs;
        lock (_sessionLock)
        {
            outputs = _recognizerSession.Run(feed);
        }

        using (outputs)
        {
            DisposableNamedOnnxValue? output = outputs.FirstOrDefault();
            if (output == null)
            {
                return string.Empty;
            }

            Tensor<float> logits = output.AsTensor<float>();
            return DecodeCtc(logits);
        }
    }

    private static (string InputName, int InputHeight, int InputWidth) ResolveInputShape(InferenceSession session)
    {
        var input = session.InputMetadata.First();
        IReadOnlyList<int> dims = input.Value.Dimensions;

        int inputHeight = ResolveDim(dims, 2, 48);
        int inputWidth = ResolveDim(dims, 3, 320);
        return (input.Key, inputHeight, inputWidth);
    }

    private static int ResolveDim(IReadOnlyList<int> dims, int index, int fallback)
    {
        if (index >= 0 && index < dims.Count && dims[index] > 0)
        {
            return dims[index];
        }

        return fallback;
    }

    private static DenseTensor<float> BuildInputTensor(FrameImage frame, int targetHeight, int targetWidth)
    {
        DenseTensor<float> tensor = new([1, 3, targetHeight, targetWidth]);

        int sourceWidth = Math.Max(1, frame.Width);
        int sourceHeight = Math.Max(1, frame.Height);
        int channels = Math.Max(1, frame.Channels);
        int minLength = sourceWidth * sourceHeight * channels;
        if (frame.Pixels.Length < minLength)
        {
            return tensor;
        }

        for (int y = 0; y < targetHeight; y++)
        {
            int sourceY = y * sourceHeight / targetHeight;
            for (int x = 0; x < targetWidth; x++)
            {
                int sourceX = x * sourceWidth / targetWidth;
                int offset = (sourceY * sourceWidth + sourceX) * channels;
                if (offset < 0 || offset >= frame.Pixels.Length)
                {
                    continue;
                }

                float r;
                float g;
                float b;
                if (channels >= 4 && offset + 2 < frame.Pixels.Length)
                {
                    // 默认输入来自 BGRA。
                    b = frame.Pixels[offset] / 255f;
                    g = frame.Pixels[offset + 1] / 255f;
                    r = frame.Pixels[offset + 2] / 255f;
                }
                else if (channels >= 3 && offset + 2 < frame.Pixels.Length)
                {
                    r = frame.Pixels[offset] / 255f;
                    g = frame.Pixels[offset + 1] / 255f;
                    b = frame.Pixels[offset + 2] / 255f;
                }
                else
                {
                    float gray = frame.Pixels[offset] / 255f;
                    r = gray;
                    g = gray;
                    b = gray;
                }

                tensor[0, 0, y, x] = (r - 0.5f) / 0.5f;
                tensor[0, 1, y, x] = (g - 0.5f) / 0.5f;
                tensor[0, 2, y, x] = (b - 0.5f) / 0.5f;
            }
        }

        return tensor;
    }

    private string DecodeCtc(Tensor<float> logits)
    {
        int[] dims = logits.Dimensions.ToArray();
        if (dims.Length < 2)
        {
            return string.Empty;
        }

        int timeSteps;
        int classCount;
        Func<int, int, float> reader;

        if (dims.Length == 2)
        {
            timeSteps = dims[0];
            classCount = dims[1];
            reader = (t, c) => logits[t, c];
        }
        else
        {
            int first = dims[1];
            int second = dims[2];
            if (first > second)
            {
                // 一些模型输出为 [1, classes, timesteps]。
                timeSteps = second;
                classCount = first;
                reader = (t, c) => logits[0, c, t];
            }
            else
            {
                // 常规 PP-OCR 识别输出 [1, timesteps, classes]。
                timeSteps = first;
                classCount = second;
                reader = (t, c) => logits[0, t, c];
            }
        }

        if (timeSteps <= 0 || classCount <= 0)
        {
            return string.Empty;
        }

        StringBuilder builder = new();
        int previousIndex = -1;

        for (int t = 0; t < timeSteps; t++)
        {
            int bestIndex = 0;
            float bestValue = float.MinValue;
            for (int c = 0; c < classCount; c++)
            {
                float value = reader(t, c);
                if (value > bestValue)
                {
                    bestValue = value;
                    bestIndex = c;
                }
            }

            if (bestIndex == 0 || bestIndex == previousIndex)
            {
                previousIndex = bestIndex;
                continue;
            }

            previousIndex = bestIndex;
            builder.Append(MapToken(bestIndex));
        }

        return builder.ToString().Trim();
    }

    private string MapToken(int tokenIndex)
    {
        if (_dictionary.Count == 0)
        {
            return tokenIndex.ToString(CultureInfo.InvariantCulture) + " ";
        }

        int dictIndex = tokenIndex - 1;
        if (dictIndex < 0 || dictIndex >= _dictionary.Count)
        {
            return string.Empty;
        }

        return _dictionary[dictIndex];
    }

    private IReadOnlyList<string> LoadDictionary()
    {
        if (string.IsNullOrWhiteSpace(_options.KeysFilePath) || !File.Exists(_options.KeysFilePath))
        {
            return Array.Empty<string>();
        }

        try
        {
            return File.ReadLines(_options.KeysFilePath)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(ExtractToken)
                .Where(token => !string.IsNullOrWhiteSpace(token))
                .ToArray();
        }
        catch (Exception ex)
        {
            _logger?.Invoke($"读取OCR字典失败: {ex.Message}");
            return Array.Empty<string>();
        }
    }

    private static string ExtractToken(string line)
    {
        if (line.Contains('\t', StringComparison.Ordinal))
        {
            string[] parts = line.Split('\t');
            return parts[^1];
        }

        return line;
    }
}
