using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HtmlToPdfService.Data.Models
{
    [PrimaryKey(nameof(Id))]
    public class Job
    {
        [Column(TypeName = "nvarchar(36)")]
        public string Id { get; private set; }

        public JobState State { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "nvarchar(500)")]
        public string? Data { get; set; }

        [Column(TypeName = "nvarchar(500)")]
        public string? Result { get; set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime? ModifiedAt { get; set; }

        public string? WorkerId { get; set; }

        public string? Details { get; set; }

        public Job()
        {
            Id = Guid.NewGuid().ToString();
            State = JobState.New;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
