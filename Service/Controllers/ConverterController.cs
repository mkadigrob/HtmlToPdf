using HtmlToPdfService.Data;
using HtmlToPdfService.Data.Dto;
using HtmlToPdfService.Data.Models;
using HtmlToPdfService.Services;
using Microsoft.AspNetCore.Mvc;

namespace HtmlToPdfService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConverterController : ControllerBase
    {
        private readonly IJobRepository _repository;
        private readonly IStorageService _storage;

        public ConverterController(IJobRepository repository, IStorageService storage)
        {
            _repository = repository;
            _storage = storage;
        }

        [HttpPost()]
        [Route("jobs")]
        public async Task<IActionResult> CreateJob(IFormFile file)
        {
            if (Path.GetExtension(file.FileName) != ".html")
            {
                return BadRequest(new ErrorDto()
                {
                    Message = "Unsupported file format"
                });
            }

            var job = new Job();
            using (var stream = file.OpenReadStream())
            {
                var fileName = $"{job.Id}{Path.GetExtension(file.FileName)}";
                job.Data = await _storage.PutAsync(fileName, stream);
            }
            try
            {
                await _repository.AddJobAsync(job);
            }
            catch
            {
                await _storage.DeleteAsync(job.Data);
                throw;
            }

            return Ok(new JobDto()
            {
                JobId = job.Id
            });
        }

        [HttpDelete]
        [Route("jobs/{jobId}")]
        public async Task DeleteJob(string jobId)
        {
            var job = await _repository.GetJobAsync(jobId);

            if (job == null) { return; }

            if (job.Data != null)
            {
                await _storage.DeleteAsync(job.Data);
            }

            if (job.Result != null)
            {
                await _storage.DeleteAsync(job.Result);
            }

            await _repository.DeleteJobAsync(jobId);
        }

        [HttpGet]
        [Route("details/{jobId}")]
        public async Task<IActionResult> GetJobDetails(string jobId)
        {
            var job = await _repository.GetJobAsync(jobId);

            if (job == null)
            {
                return BadRequest();
            }

            return Ok(new JobDetailsDto()
            {
                State = job.State.ToString(),
                Details = job.Details
            });
        }

        [HttpGet]
        [Route("results/{jobId}")]
        public async Task<IActionResult> GetJobResult(string jobId)
        {
            var job = await _repository.GetJobAsync(jobId);

            if (job == null || job.State != JobState.Completed)
            {
                return BadRequest();
            }

            _ = job.Result ?? throw new Exception("Unknown job result");

            var stream = await _storage.GetAsync(job.Result);
            return File(stream, "application/octet-stream", Path.GetFileName(job.Result));
        }
    }
}
