using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CogStayMVC.DTOs;
using CogStayMVC.Models;
using CogStayMVC.Repositories.Interfaces;
using CogStayMVC.Services.Interfaces;

namespace CogStayMVC.Services.Manager;

public class FeedbackService : IFeedbackService
{
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly IGuestRepository _guestRepository;

    public FeedbackService(
        IFeedbackRepository feedbackRepository,
        IGuestRepository guestRepository)
    {
        _feedbackRepository = feedbackRepository;
        _guestRepository = guestRepository;
    }

    public async Task<IEnumerable<FeedbackResponseDTO>> GetAllFeedbacksAsync()
    {
        var feedbacks = await _feedbackRepository.GetFeedbacksWithDetailsAsync();
        return feedbacks.Select(MapToDTO);
    }

    public async Task<FeedbackResponseDTO?> GetFeedbackByIdAsync(int id)
    {
        var feedbacks = await _feedbackRepository.GetFeedbacksWithDetailsAsync();
        var feedback = feedbacks.FirstOrDefault(f => f.FeedbackId == id);
        return feedback != null ? MapToDTO(feedback) : null;
    }

    public async Task<FeedbackResponseDTO> SubmitFeedbackAsync(CreateFeedbackDTO dto)
    {
        var guest = await _guestRepository.GetByIdAsync(dto.GuestId);
        if (guest == null)
            throw new InvalidOperationException("Guest account not found.");

        var feedback = new Feedback
        {
            GuestId = dto.GuestId,
            ReservationId = dto.ReservationId,
            Rating = dto.Rating,
            Comments = dto.Comments,
            CreatedAt = DateTime.Now
        };

        await _feedbackRepository.AddAsync(feedback);
        return MapToDTO(feedback);
    }

    public async Task DeleteFeedbackAsync(int id)
    {
        await _feedbackRepository.DeleteAsync(id);
    }

    private static FeedbackResponseDTO MapToDTO(Feedback f) => new()
    {
        FeedbackId = f.FeedbackId,
        GuestId = f.GuestId,
        GuestName = f.Guest?.FullName ?? "Guest",
        ReservationId = f.ReservationId,
        Rating = f.Rating,
        Comments = f.Comments,
        CreatedAt = f.CreatedAt
    };
}
