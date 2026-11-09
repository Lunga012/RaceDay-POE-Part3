using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Mvc.Models
{
    public class EventCardViewModel
    {
        public int EventID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public string? Location { get; set; }
        public decimal? Distance { get; set; }
        public string? Type { get; set; }
        public string? ImageUrl { get; set; }
        public int OrganiserID { get; set; }
        public string OrganiserName { get; set; } = string.Empty;
        public int TotalEnrolments { get; set; }
        public List<CategoryItemViewModel> Categories { get; set; } = new();
    }

    public class EventDetailViewModel
    {
        public int EventID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public string? Location { get; set; }
        public decimal? Distance { get; set; }
        public string? Type { get; set; }
        public string? ImageUrl { get; set; }
        public int OrganiserID { get; set; }
        public string OrganiserName { get; set; } = string.Empty;
        public int TotalEnrolments { get; set; }
        public List<CategoryItemViewModel> Categories { get; set; } = new();

        // Enrolment state for current user
        public bool IsUserLoggedIn { get; set; }
        public bool IsUserParticipant { get; set; }
        public bool IsUserEnrolled { get; set; }
        public string? UserEnrolmentStatus { get; set; }
        public string? UserCategoryName { get; set; }
        public int? SelectedCategoryID { get; set; }
    }

    public class CreateEventViewModel
    {
        [Required(ErrorMessage = "Event name is required.")]
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "Description cannot exceed 150 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Event date is required.")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today.AddDays(14);

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters.")]
        public string? Location { get; set; }

        [Required(ErrorMessage = "Distance is required.")]
        [Range(0.1, 999.99, ErrorMessage = "Distance must be between 0.1 and 999.99 km.")]
        public decimal? Distance { get; set; }

        [Required(ErrorMessage = "Event type is required.")]
        public string? Type { get; set; } = "Run"; // Run, Walk, Cycle

        [Display(Name = "Event Banner Image")]
        public IFormFile? BannerImage { get; set; }

        public string? ExistingImageUrl { get; set; }
    }

    public class EditEventViewModel
    {
        public int EventID { get; set; }

        [Required(ErrorMessage = "Event name is required.")]
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "Description cannot exceed 150 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Event date is required.")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters.")]
        public string? Location { get; set; }

        [Required(ErrorMessage = "Distance is required.")]
        [Range(0.1, 999.99, ErrorMessage = "Distance must be between 0.1 and 999.99 km.")]
        public decimal? Distance { get; set; }

        [Required(ErrorMessage = "Event type is required.")]
        public string? Type { get; set; }

        [Display(Name = "Change Event Banner Image")]
        public IFormFile? BannerImage { get; set; }

        public string? ExistingImageUrl { get; set; }
    }

    public class CategoryItemViewModel
    {
        public int CategoryID { get; set; }
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class ManageCategoriesViewModel
    {
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public List<CategoryItemViewModel> Categories { get; set; } = new();

        // New category creation fields
        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(50, ErrorMessage = "Category name cannot exceed 50 characters.")]
        public string NewCategoryName { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "Description cannot exceed 150 characters.")]
        public string? NewCategoryDescription { get; set; }
    }

    public class OrganiserDashboardViewModel
    {
        public string OrganiserName { get; set; } = string.Empty;
        public int TotalEvents { get; set; }
        public int TotalEnrolments { get; set; }
        public int UpcomingEventsCount { get; set; }
        public int CompletedEventsCount { get; set; }
        public List<EventCardViewModel> Events { get; set; } = new();
    }
}
