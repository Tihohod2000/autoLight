using System;
using System.Threading;
using System.Threading.Tasks;
using System.Management;

class Program
{
    static async Task Main()
    {
        using (BrightnessDetector detector = new BrightnessDetector())
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Console.WriteLine("Нажмите LCtrl + C чтобы остановить программу...");

            int lastChenge = 10;

            while (cts.Token.CanBeCanceled)
            {
                Thread.Sleep(200);

                // Получение яркости асинхронно
                double brightness1 = await detector.GetBrightnessAsync(cts.Token);

                if (brightness1 != -1)
                {
                    // Console.WriteLine("Яркость окружения: " + brightness1);
                }
                else if (brightness1 == -1)
                {
                    Console.WriteLine("Не удалось получить доступ к камере!!!");
                    Console.WriteLine("Выход из программы");
                    Thread.Sleep(1000);

                    break;
                }

                // Console.Write("Введите уровень яркости (0-100): ");

                // brightness1 = int.Parse(Console.ReadLine());


                int brightness = (int)brightness1 - 10;
                if (Math.Abs(brightness - lastChenge) > 20)
                {
                    // Console.WriteLine("Слишком сильный перепад яркости...");
                    
                    if (brightness - lastChenge < 0)
                    {
                        brightness = Math.Clamp(lastChenge - 10, 0, 100);
                        SetBrightness(brightness);
                    }
                    else
                    {
                        brightness = Math.Clamp(lastChenge + 10, 0, 100);
                        SetBrightness(brightness);
                    }
                }
                else
                {
                    brightness = Math.Clamp(brightness, 0, 100);
                    SetBrightness(brightness);
                }

                lastChenge = brightness;
            }
        }
    }

    static ManagementScope scope = new ManagementScope("root\\WMI");
    static SelectQuery query = new SelectQuery("WmiMonitorBrightnessMethods");

    static void SetBrightness(int brightness)
    {
        try
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    obj.InvokeMethod("WmiSetBrightness", new object[] { uint.MaxValue, brightness });
                    // Console.WriteLine($"Яркость установлена на {brightness}%.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при изменении яркости: " + ex.Message);
        }
    }
}