using HtmlToPdfService.Data;
using HtmlToPdfService.Data.Models;
using PuppeteerSharp;
using System.Text;

namespace HtmlToPdfService.Services
{
    public class Worker
    {
        private readonly string _id;
        private readonly IJobRepository _repository;
        private readonly IStorageService _storage;
        private readonly ILogger<Worker> _logger;

        public Worker(IJobRepository repository, IStorageService storage, ILogger<Worker> logger)
        {
            _id = Guid.NewGuid().ToString();
            _repository = repository;
            _storage = storage;
            _logger = logger;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var job = await _repository.StartJobAsync(_id, stoppingToken);

                if (job == null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(10));
                    continue;
                }

                try
                {
                    await ConvertAsync(job, stoppingToken);
                }
                catch (Exception ex)
                {
                    await _repository.FailJobAsync(job, ex, stoppingToken);
                    continue;
                }

                await _repository.CompleteJobAsync(job, stoppingToken);
            }
        }

        private async Task ConvertAsync(Job job, CancellationToken cancellationToken)
        {
            using (var browserFetcher = new BrowserFetcher())
            {
                await browserFetcher.DownloadAsync();
                var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true
                });

                using (var page = await browser.NewPageAsync())
                {
                    var html = await GetHtmlAsync(job, cancellationToken);
                    await page.SetContentAsync(html);
                    using (var stream = await page.PdfStreamAsync())
                    {
                        await SaveStreamAsync(stream, job, cancellationToken);
                    }
                }
            }
        }

        private async Task<string> GetHtmlAsync(Job job, CancellationToken cancellationToken)
        {
            _ = job.Data ?? throw new Exception("Job data is null");

            using (var stream = await _storage.GetAsync(job.Data))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }

        private async Task SaveStreamAsync(Stream stream, Job job, CancellationToken cancellationToken)
        {
            var fileName = $"{Path.GetFileNameWithoutExtension(job.Data)}.pdf";
            job.Result = await _storage.PutAsync(fileName, stream);
        }
    }
}
