using System.ComponentModel.DataAnnotations;

namespace Trackademy.Application.AssignmentServices.Models;

public class MyAssignmentsRequest
{
    [Required]
    public Guid OrganizationId { get; set; }
}
