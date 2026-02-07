using OpenCvSharp;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;
using Sdcb.PaddleOCR.Models.Details;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace JinChanChanTool.Services
{
    /// <summary>
    /// 基于队列的PaddleOCR文字识别服务，支持CPU和GPU设备。
    /// </summary>
    public class QueuedOCRService : IDisposable
    {
        private QueuedPaddleOcrAll _ocrQueue;
        private CancellationTokenSource _cts;
        private readonly int _cpuThreadCount;
        private readonly int _consumerCount;
        private readonly bool _enableWarmup;
        private readonly 设备 _device;

        public enum 设备
        {
            CPU, GPU
        }

        /// <summary>
        /// 兼容原调用：保持无参构造器行为
        /// </summary>
        public QueuedOCRService(设备 device) : this(device, 0, 1, true) { }

        /// <summary>
        /// 自动计算推荐值的线程数
        /// 可选传入 cpuThreadCount（<=0 表示自动计算推荐值）
        /// </summary>
        public QueuedOCRService(设备 device, int cpuThreadCount) : this(device, cpuThreadCount, 1, true) { }

        /// <summary>
        /// 传入线程数、消费者数量和预热策略，兼容CPU/GPU两种设备。
        /// </summary>
        public QueuedOCRService(设备 device, int cpuThreadCount, int consumerCount, bool enableWarmup = true)
        {
            _cpuThreadCount = cpuThreadCount;
            _consumerCount = Math.Max(1, consumerCount);
            _enableWarmup = enableWarmup;
            _device = device;

            InitializeQueueByDevice();
            _cts = new CancellationTokenSource();
            QueueWarmup();
        }

        public int ConsumerCount => _consumerCount;

        /// <summary>
        /// 通过环境变量设置 Paddle/MKL/OpenBLAS/OpenMP 的线程数。threadCount <= 0 时采用自动推荐值（逻辑处理器数的一半，至少 1）。必须在创建 Paddle 设备之前调用（即在 InitializeOcrQueueCPU 的开头调用）。
        /// </summary>
        /// <param name="threadCount"></param>
        private void SetCpuThreadCount(int threadCount = 0)
        {
            int count = threadCount > 0 ? threadCount : Math.Max(1, Environment.ProcessorCount / 2);

            // 常用控制线程数的环境变量
            Environment.SetEnvironmentVariable("OMP_NUM_THREADS", count.ToString());
            Environment.SetEnvironmentVariable("MKL_NUM_THREADS", count.ToString());
            Environment.SetEnvironmentVariable("OPENBLAS_NUM_THREADS", count.ToString());

            // 额外的兼容变量（有些构建可能会检查）
            Environment.SetEnvironmentVariable("PADDLE_CPU_THREADS", count.ToString());          
        }

        /// <summary>
        /// 初始化基于CPU的OCR队列服务
        /// </summary>
        private void InitializeOcrQueueCPU()
        {
            // 先设置 CPU 线程数（如果 _cpuThreadCount <= 0，会自动计算推荐值）
            SetCpuThreadCount(_cpuThreadCount);
            _ocrQueue = CreateQueue(CreateFactory(设备.CPU));
        }

        /// <summary>
        /// 初始化基于GPU的OCR队列服务
        /// </summary>
        private void InitializeOcrQueueGPU()
        {
            _ocrQueue = CreateQueue(CreateFactory(设备.GPU));
        }

        private void InitializeQueueByDevice()
        {
            switch (_device)
            {
                case 设备.CPU:
                    InitializeOcrQueueCPU();
                    break;
                case 设备.GPU:
                    InitializeOcrQueueGPU();
                    break;
                default:
                    InitializeOcrQueueCPU();
                    break;
            }
        }

        private Func<PaddleOcrAll> CreateFactory(设备 device)
        {
            return () =>
            {
                DetectionModel de = CreateDetectionModel();
                RecognizationModel re = CreateRecognizationModel();
                PaddleDevice paddleDevice = device == 设备.GPU ? PaddleDevice.Gpu() : PaddleDevice.Mkldnn();
                return new PaddleOcrAll(new FullOcrModel(de, re), paddleDevice);
            };
        }

        private DetectionModel CreateDetectionModel()
        {
            return new FileDetectionModel(
                Path.Combine(Application.StartupPath, "Resources\\Models\\PP-OCRv5_mobile_det_infer"),
                ModelVersion.V5);
        }

        private RecognizationModel CreateRecognizationModel()
        {
            return new FileRecognizationModel(
                Path.Combine(Application.StartupPath, "Resources\\Models\\PP-OCRv5_mobile_rec_infer"),
                Path.Combine(Application.StartupPath, "Resources\\Models", "ppocr_keys_v5.txt"),
                ModelVersion.V5);
        }

        private QueuedPaddleOcrAll CreateQueue(Func<PaddleOcrAll> factory)
        {
            return new QueuedPaddleOcrAll(
                factory: factory,
                consumerCount: _consumerCount,
                boundedCapacity: 64);
        }

        private void QueueWarmup()
        {
            if (!_enableWarmup)
            {
                return;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    using Bitmap bitmap = new Bitmap(16, 16, PixelFormat.Format24bppRgb);
                    using Graphics graphics = Graphics.FromImage(bitmap);
                    graphics.Clear(Color.White);
                    await RecognizeTextAsync(bitmap);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"OCR预热失败: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// 识别图像中的文字（异步）
        /// </summary>
        /// <param name="bitmap">要识别的图像</param>
        /// <param name="recognizeBatchSize">批量识别大小</param>
        /// <param name="configure">OCR配置操作</param>
        /// <returns>识别结果</returns>
        public async Task<string> RecognizeTextAsync(Bitmap bitmap, int recognizeBatchSize = 0, Action<PaddleOcrAll> configure = null)
        {                                   
                // 转换图像格式
                using Mat src = BitmapToMat(bitmap);
                // 提交OCR请求
                PaddleOcrResult result = await _ocrQueue.Run(
                    src: src,
                    recognizeBatchSize: recognizeBatchSize,
                    configure: configure,
                    cancellationToken: _cts.Token);               
                return result.Text;                      
        }

        /// <summary>
        /// 将Bitmap转换为OpenCV Mat对象
        /// </summary>
        private Mat BitmapToMat(Bitmap bitmap)
        {
            // 确保Bitmap格式为24bppRgb（3通道）
            if (bitmap.PixelFormat != PixelFormat.Format24bppRgb)
            {
                using Bitmap converted = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb);
                using (Graphics g = Graphics.FromImage(converted))
                {
                    g.DrawImage(bitmap, 0, 0);
                }
                return BitmapToMat(converted);
            }

            // 锁定Bitmap数据
            BitmapData bmpData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                bitmap.PixelFormat);

            try
            {
                // 创建3通道Mat对象
                Mat mat = Mat.FromPixelData(
                    rows: bitmap.Height,
                    cols: bitmap.Width,
                    type: MatType.CV_8UC3, // 8位无符号4通道
                    data: bmpData.Scan0,
                    step: bmpData.Stride
                );

                return mat.Clone();
            }
            finally
            {
                bitmap.UnlockBits(bmpData);
            }
        }

        /// <summary>
        /// 取消所有待处理的OCR请求
        /// </summary>
        public void CancelAll()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _ocrQueue?.Dispose();
            _cts?.Dispose();
        }
    }
}
