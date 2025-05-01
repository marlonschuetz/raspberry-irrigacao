using System.Device.Gpio;

namespace raspberry_irrigacao
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
            while (!stoppingToken.IsCancellationRequested)
            {
                const int sensorPin = 17; // GPIO 17 (pino físico 11)
                using GpioController controller = new GpioController();

                var pin = controller.OpenPin(sensorPin, PinMode.Input);

                Console.WriteLine("Lendo sensor T1592... {sensorPin}", pin.Read());


                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
