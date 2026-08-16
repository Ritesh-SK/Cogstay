using System;
using System.ComponentModel.DataAnnotations;

namespace CogStay.Application.DTOs;

public class FeedbackResponseDTO
{
    public int FeedbackId { get; set; }
    public int GuestId { get; set; }
    public string GuestName { get; set; } = null!;
    public int? ReservationId { get; set; }
    public int Rating { get; set; }
    public string Comments { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class CreateFeedbackDTO
{
    [Required(ErrorMessage = "Guest ID is required.")]
    public int GuestId { get; set; }

    public int? ReservationId { get; set; }

    [Required(ErrorMessage = "Rating is required.")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
    public int Rating { get; set; }

    [Required(ErrorMessage = "Comments are required.")]
    [StringLength(1000)]
    public string Comments { get; set; } = null!;
}
