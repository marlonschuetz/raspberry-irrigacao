using Microsoft.Extensions.Hosting;
using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace raspberry_irrigacao_net5
{
    public class Worker : BackgroundService
    {
        DateTime? _startedDate = null;

        // T1592
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            const int GPIO_17 = 17; 
            const int GPIO_4 = 4;
            using GpioController controller = new GpioController();

            var input  = controller.OpenPin(GPIO_17, PinMode.Input);
            var output = controller.OpenPin(GPIO_4, PinMode.Output, PinValue.Low);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (output.Read() == PinValue.High && DateTime.Now - _startedDate > TimeSpan.FromMinutes(1))
                {
                    Console.WriteLine("TRAVA ACIONADA!!!");
                    TurnOffWater(output);
                    break;
                }

                if (input.Read() == PinValue.Low)
                    TurnOnWater(output);
                else 
                    TurnOffWater(output);
                
                await Task.Delay(1000, stoppingToken);
            }
        }

        private void TurnOnWater(GpioPin output) 
        {
            _startedDate = DateTime.Now;
            output.Write(PinValue.High);
            Console.WriteLine("Água ligada. {0}", _startedDate);
        }

        private static void TurnOffWater(GpioPin output)
        {
            output.Write(PinValue.Low);
            Console.WriteLine("Água desligada. {0}", DateTime.Now);
        }
    }
}
