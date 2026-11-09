using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.Models
{
    public class ParticipantEnrolmentViewModel
    {
        public int EnrolmentID { get; set; }
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string? EventLocation { get; set; }
        public string? EventType { get; set; }
        public decimal? Distance { get; set; }
        public string? EventImageUrl { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public bool HasResult { get; set; }
        public string? FinishTime { get; set; }
        public int? Position { get; set; }
    }

    public class OrganiserEnrolmentViewModel
    {
        public int EnrolmentID { get; set; }
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int ParticipantID { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string ParticipantEmail { get; set; } = string.Empty;
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public bool HasResult { get; set; }
        public int? ResultID { get; set; }
        public string? FinishTime { get; set; }
        public int? Position { get; set; }
    }

    public class ManageEventEnrolmentsViewModel
    {
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string? EventLocation { get; set; }
        public int TotalEnrolments => Enrolments.Count;
        public int ConfirmedCount { get; set; }
        public int PendingCount { get; set; }
        public int CancelledCount { get; set; }
        public List<OrganiserEnrolmentViewModel> Enrolments { get; set; } = new();
    }

    public class EnterEventPostModel
    {
        [Required]
        public int EventID { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        public int CategoryID { get; set; }
    }
}
