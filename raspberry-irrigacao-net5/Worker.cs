using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace raspberry_irrigacao_net5
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        // T1592
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            const int GPIO_17 = 17; 
            const int GPIO_4 = 4;
            using GpioController controller = new GpioController();

            var input  = controller.OpenPin(GPIO_17, PinMode.Input);
            var output = controller.OpenPin(GPIO_4, PinMode.Output);

            do
            {
                if (input.Read() == PinValue.Low)
                {
                    TurnOnWater(output);
                }

                _logger.LogInformation("Lendo sensor T1592... {0}", input.Read());


                await Task.Delay(1000, stoppingToken);
            }
            while (true || !stoppingToken.IsCancellationRequested/* se temporizador passar 5 min deve desligar*/);
        }

        private void TurnOnWater(GpioPin output) 
        {
            // Criar lógica para iniciar um temporizador quando passar aqui pela primeira vez.

            _logger.LogInformation("Água ligada.");
            output.Write(PinValue.High);
        }

        private void TurnOffWater(GpioPin output)
        {
            _logger.LogInformation("Água desligada.");
            output.Write(PinValue.Low);
        }
    }
}
