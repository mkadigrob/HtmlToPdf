using HtmlToPdfService.Data.Models;

namespace HtmlToPdfService.Data
{
    public interface IJobRepository
    {
        Task<Job?> GetJobAsync(string jobId, CancellationToken cancellationToken = default);
        Task AddJobAsync(Job job, CancellationToken cancellationToken = default);
        Task DeleteJobAsync(string jobId, CancellationToken cancellationToken = default);
        Task<Job?> StartJobAsync(string workerId, CancellationToken cancellationToken = default);
        Task CompleteJobAsync(Job job, CancellationToken cancellationToken = default);
        Task FailJobAsync(Job job, Exception ex, CancellationToken cancellationToken = default);
    }
}
