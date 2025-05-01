using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Device.Gpio;
using System.Text;
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.OutputEncoding = Encoding.UTF8; // Garante saída UTF-8 no console

            const int SENSOR_PIN = 17;  // GPIO do sensor T1592
            const int VALVE_PIN = 4;    // GPIO da válvula (saída para ligar água)
            using GpioController controller = new GpioController();

            controller.OpenPin(SENSOR_PIN, PinMode.Input);
            controller.OpenPin(VALVE_PIN, PinMode.Output);
            controller.Write(VALVE_PIN, PinValue.Low); // Inicia com a água desligada

            DateTime? ligouAguaEm = null;

            do
            {
                var leitura = controller.Read(SENSOR_PIN); // LOW = úmido, HIGH = seco
                _logger.LogInformation("Leitura do sensor T1592: {0}", leitura);

                if (leitura == PinValue.High) // Solo seco
                {
                    if (ligouAguaEm == null)
                    {
                        TurnOnWater(controller, VALVE_PIN);
                        ligouAguaEm = DateTime.Now;
                    }
                }
                else // Solo molhado
                {
                    if (ligouAguaEm != null)
                    {
                        TurnOffWater(controller, VALVE_PIN);
                        ligouAguaEm = null;
                    }
                }

                // Se passou de 5 minutos irrigando, força o desligamento e encerra app
                if (ligouAguaEm != null && DateTime.Now - ligouAguaEm > TimeSpan.FromMinutes(2))
                {
                    _logger.LogWarning("Água desligada automaticamente após 2 minutos.");
                    TurnOffWater(controller, VALVE_PIN);
                    break; // Encerra a aplicação
                }

                await Task.Delay(1000, stoppingToken);
            }
            while (!stoppingToken.IsCancellationRequested);
        }

        private void TurnOnWater(GpioController controller, int pin)
        {
            _logger.LogInformation("Água ligada.");
            controller.Write(pin, PinValue.High);
        }

        private void TurnOffWater(GpioController controller, int pin)
        {
            _logger.LogInformation("Água desligada.");
            controller.Write(pin, PinValue.Low);
        }
    }
}
