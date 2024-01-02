using HtmlToPdfService.Helpers;

namespace HtmlToPdfService.Services
{
    public class WorkerService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WorkerService> _logger;

        public WorkerService(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILogger<WorkerService> logger)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var workersNumber = _configuration.GetWorkersNumber();
            var workers = new Task[workersNumber];

            for (var i = 0; i < workersNumber; i++)
            {
                workers[i] = RunWorkerAsync(stoppingToken);
            }

            _logger.LogInformation("{0} workers are started", workersNumber);
            await Task.WhenAll(workers);
        }

        private async Task RunWorkerAsync(CancellationToken cancellationToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var worker = scope.ServiceProvider.GetRequiredService<Worker>();
                await worker.ExecuteAsync(cancellationToken);
            };
        }
    }
}
