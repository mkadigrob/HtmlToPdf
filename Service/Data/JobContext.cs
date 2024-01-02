using HtmlToPdfService.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HtmlToPdfService.Data
{
    public class JobContext : DbContext
    {
        public DbSet<Job> Jobs { get; set; }

        public JobContext(DbContextOptions<JobContext> options) : base(options) { }
    }
}
