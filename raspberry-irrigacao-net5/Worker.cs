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
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            const int SENSOR_PIN = 17;  // GPIO 17 – Entrada digital do sensor T1592
            const int VALVE_PIN = 4;    // GPIO 4 – Saída para controlar válvula ou relé

            using GpioController controller = new GpioController();

            controller.OpenPin(SENSOR_PIN, PinMode.Input);
            controller.OpenPin(VALVE_PIN, PinMode.Output);
            controller.Write(VALVE_PIN, PinValue.Low); // Garante que inicia desligado

            DateTime? ligouAguaEm = null;

            while (!stoppingToken.IsCancellationRequested)
            {
                var sensorValue = controller.Read(SENSOR_PIN); // LOW = úmido, HIGH = seco
                _logger.LogInformation("Leitura do sensor T1592: {0}", sensorValue);

                if (sensorValue == PinValue.High) // Solo seco → ligar água
                {
                    if (ligouAguaEm == null)
                    {
                        TurnOnWater(controller, VALVE_PIN);
                        ligouAguaEm = DateTime.Now;
                    }
                }
                else // Solo úmido → desligar água se estiver ligada
                {
                    if (ligouAguaEm != null)
                    {
                        TurnOffWater(controller, VALVE_PIN);
                        ligouAguaEm = null;
                    }
                }

                // Desliga após 5 minutos, mesmo que solo ainda esteja seco
                if (ligouAguaEm != null && DateTime.Now - ligouAguaEm > TimeSpan.FromMinutes(5))
                {
                    _logger.LogWarning("Água desligada automaticamente após 5 minutos.");
                    TurnOffWater(controller, VALVE_PIN);
                    ligouAguaEm = null;
                }

                await Task.Delay(1000, stoppingToken); // Espera 1 segundo entre leituras
            }
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
