using HtmlToPdfService.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HtmlToPdfService.Data
{
    public class JobRepository : IJobRepository
    {
        private readonly JobContext _db;
        private readonly ILogger<JobRepository> _logger;

        public JobRepository(JobContext db, ILogger<JobRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Job?> GetJobAsync(string jobId, CancellationToken cancellationToken = default)
        {
            return await _db.Jobs
                .AsNoTracking()
                .Where(x => x.Id == jobId)
                .FirstOrDefaultAsync();
        }

        public async Task AddJobAsync(Job job, CancellationToken cancellationToken = default)
        {
            _db.Jobs.Add(job);
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Add job '{0}'", job.Id);
        }

        public async Task DeleteJobAsync(string jobId, CancellationToken cancellationToken = default)
        {
            await _db.Jobs
                .Where(x => x.Id == jobId)
                .ExecuteDeleteAsync(cancellationToken);

            _logger.LogInformation("Delete job '{0}'", jobId);
        }

        public async Task<Job?> StartJobAsync(string workerId, CancellationToken cancellationToken = default)
        {
            var affectedRows = await _db.Jobs
                    .Where(x => x.State == JobState.New)
                    .Where(x => x.WorkerId == null)
                    .OrderBy(x => x.CreatedAt)
                    .Take(1)
                    .ExecuteUpdateAsync(setter =>
                        setter
                            .SetProperty(x => x.State, JobState.Started)
                            .SetProperty(x => x.WorkerId, workerId)
                            .SetProperty(x => x.ModifiedAt, DateTime.UtcNow), cancellationToken);

            if (affectedRows == 0)
            {
                return null;
            }

            var job = await _db.Jobs
                .AsNoTracking()
                .Where(x => x.State == JobState.Started)
                .Where(x => x.WorkerId == workerId)
                .FirstAsync(cancellationToken);

            _logger.LogInformation("Worker '{0}' lock job '{1}'", workerId, job.Id);

            return job;
        }

        public async Task CompleteJobAsync(Job job, CancellationToken cancellationToken = default)
        {
            await _db.Jobs
                .Where(x => x.Id == job.Id)
                .ExecuteUpdateAsync(setter =>
                    setter
                        .SetProperty(x => x.State, JobState.Completed)
                        .SetProperty(x => x.Result, job.Result)
                        .SetProperty(x => x.ModifiedAt, DateTime.UtcNow), cancellationToken);

            _logger.LogInformation("Complete job '{0}'", job.Id);
        }

        public async Task FailJobAsync(Job job, Exception ex, CancellationToken cancellationToken = default)
        {
            await _db.Jobs
                .Where(x => x.Id == job.Id)
                .ExecuteUpdateAsync(setter =>
                    setter
                        .SetProperty(x => x.State, JobState.Fail)
                        .SetProperty(x => x.Details, ex.Message)
                        .SetProperty(x => x.ModifiedAt, DateTime.UtcNow), cancellationToken);

            _logger.LogError(ex, "Fail job '{0}'", job.Id);
        }
    }
}
