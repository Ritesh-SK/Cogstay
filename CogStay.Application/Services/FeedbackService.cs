using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CogStay.Application.Contracts.Persistence;
using CogStay.Application.Contracts.Services;
using CogStay.Application.DTOs;
using CogStay.Domain.Entities;

namespace CogStay.Application.Services;

public class FeedbackService : IFeedbackService
{
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly IGuestRepository _guestRepository;

    public FeedbackService(IFeedbackRepository feedbackRepository, IGuestRepository guestRepository)
    {
        _feedbackRepository = feedbackRepository;
        _guestRepository = guestRepository;
    }

    public async Task<IEnumerable<FeedbackResponseDTO>> GetAllFeedbacksAsync()
    {
        var list = await _feedbackRepository.GetAllAsync();
        return await MapToDTOListAsync(list);
    }

    public async Task<FeedbackResponseDTO?> GetFeedbackByIdAsync(int id)
    {
        var feedback = await _feedbackRepository.GetByIdAsync(id);
        if (feedback == null) return null;
        var guest = await _guestRepository.GetByIdAsync(feedback.GuestId);
        return MapToDTO(feedback, guest);
    }

    public async Task<FeedbackResponseDTO> SubmitFeedbackAsync(CreateFeedbackDTO dto)
    {
        var guest = await _guestRepository.GetByIdAsync(dto.GuestId);
        if (guest == null)
        {
            throw new KeyNotFoundException($"Guest with ID {dto.GuestId} not found.");
        }

        var nextId = await _feedbackRepository.GetNextFeedbackIdAsync();
        var feedback = new Feedback
        {
            FeedbackId = nextId,
            GuestId = dto.GuestId,
            ReservationId = dto.ReservationId,
            Rating = dto.Rating,
            Comments = dto.Comments,
            CreatedAt = DateTime.UtcNow
        };

        await _feedbackRepository.CreateAsync(feedback);
        return MapToDTO(feedback, guest);
    }

    public async Task DeleteFeedbackAsync(int id)
    {
        await _feedbackRepository.DeleteAsync(id);
    }

    private async Task<IEnumerable<FeedbackResponseDTO>> MapToDTOListAsync(IEnumerable<Feedback> list)
    {
        var dtos = new List<FeedbackResponseDTO>();
        foreach (var f in list)
        {
            var guest = await _guestRepository.GetByIdAsync(f.GuestId);
            dtos.Add(MapToDTO(f, guest));
        }
        return dtos;
    }

    private static FeedbackResponseDTO MapToDTO(Feedback f, Guest? g) => new()
    {
        FeedbackId = f.FeedbackId,
        GuestId = f.GuestId,
        GuestName = g?.FullName ?? "Guest",
        ReservationId = f.ReservationId,
        Rating = f.Rating,
        Comments = f.Comments,
        CreatedAt = f.CreatedAt
    };
}
