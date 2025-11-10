using Trackademy.Application.Shared.Models;
using Trackademy.Domain.Enums;

namespace Trackademy.Application.Shared.Models
{
    public class GetSubmissionsRequest : PagedRequest
    {
        /// <summary>
        /// ID организации (обязательно)
        /// </summary>
        public Guid OrganizationId { get; set; }
        
        /// <summary>
        /// Фильтр по заданию (опционально)
        /// </summary>
        public Guid? AssignmentId { get; set; }
        
        /// <summary>
        /// Фильтр по группе (опционально)
        /// </summary>
        public Guid? GroupId { get; set; }
        
        /// <summary>
        /// Фильтр по студенту (опционально)
        /// Автоматически устанавливается для студентов
        /// </summary>
        public Guid? StudentId { get; set; }
        
        /// <summary>
        /// Фильтр по статусу (опционально)
        /// </summary>
        public SubmissionStatus? Status { get; set; }
        
        /// <summary>
        /// Фильтр по дате сдачи - С (опционально)
        /// </summary>
        public DateTime? FromDate { get; set; }
        
        /// <summary>
        /// Фильтр по дате сдачи - ПО (опционально)
        /// </summary>
        public DateTime? ToDate { get; set; }
    }
}
