using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.Models
{
    public class CaptureResultViewModel
    {
        public int EnrolmentID { get; set; }
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string ParticipantName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;

        public int? ResultID { get; set; }

        [Required(ErrorMessage = "Finish time is required.")]
        [RegularExpression(@"^([0-9]{1,2}):([0-5][0-9]):([0-5][0-9])$", ErrorMessage = "Finish time must be in HH:mm:ss format (e.g. 01:45:30).")]
        public string FinishTime { get; set; } = "01:30:00";

        [Required(ErrorMessage = "Finishing position is required.")]
        [Range(1, 100000, ErrorMessage = "Position must be 1 or higher.")]
        public int Position { get; set; } = 1;
    }

    public class ParticipantResultHistoryViewModel
    {
        public int ResultID { get; set; }
        public int EnrolmentID { get; set; }
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string? EventLocation { get; set; }
        public string? EventType { get; set; }
        public decimal? Distance { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string FinishTime { get; set; } = string.Empty;
        public int Position { get; set; }
        public int TotalFinishers { get; set; }

        public string FormattedPosition => $"{GetOrdinal(Position)} out of {TotalFinishers} finishers";

        private static string GetOrdinal(int num)
        {
            if (num <= 0) return num.ToString();
            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return $"{num}th";
            }
            switch (num % 10)
            {
                case 1: return $"{num}st";
                case 2: return $"{num}nd";
                case 3: return $"{num}rd";
                default: return $"{num}th";
            }
        }
    }

    public class ManageEventResultsViewModel
    {
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public List<OrganiserEnrolmentViewModel> Enrolments { get; set; } = new();
    }
}
