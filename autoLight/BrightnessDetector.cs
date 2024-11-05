using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Threading;
using System.Threading.Tasks;

public class BrightnessDetector : IDisposable
{
    private VideoCapture _capture;
    private Mat _frame;
    private Mat _grayFrame;
    private bool _isRunning;

    public BrightnessDetector()
    {
        _capture = new VideoCapture();
        _frame = new Mat();
        _grayFrame = new Mat();
    }

    public async Task<double> GetBrightnessAsync(CancellationToken cancellationToken)
    {
        if (!_capture.IsOpened)
        {
            Console.WriteLine("Камера не доступна.");
            return -1;
        }

        _isRunning = true;
        return await Task.Run(() =>
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                _capture.Read(_frame);

                if (_frame.IsEmpty)
                {
                    Console.WriteLine("Не удалось захватить кадр.");
                    return -1;
                }

                CvInvoke.CvtColor(_frame, _grayFrame, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
                MCvScalar mean = CvInvoke.Mean(_grayFrame);
                return mean.V0;
            }

            return -1;
        }, cancellationToken);
    }

    public void Dispose()
    {
        _isRunning = false;
        _capture.Dispose();
        _frame.Dispose();
        _grayFrame.Dispose();
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}