using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using CogStay.Application.Contracts.Persistence;
using CogStay.Domain.Entities;
using CogStay.Infrastructure.Data;

namespace CogStay.Infrastructure.Repositories;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly MongoDbContext _context;

    public FeedbackRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Feedback>> GetAllAsync()
    {
        return await _context.Feedbacks.Find(_ => true).ToListAsync();
    }

    public async Task<Feedback?> GetByIdAsync(int id)
    {
        return await _context.Feedbacks.Find(f => f.FeedbackId == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Feedback>> GetByGuestIdAsync(int guestId)
    {
        return await _context.Feedbacks.Find(f => f.GuestId == guestId).ToListAsync();
    }

    public async Task<Feedback> CreateAsync(Feedback feedback)
    {
        await _context.Feedbacks.InsertOneAsync(feedback);
        return feedback;
    }

    public async Task DeleteAsync(int id)
    {
        await _context.Feedbacks.DeleteOneAsync(f => f.FeedbackId == id);
    }

    public async Task<int> GetNextFeedbackIdAsync()
    {
        var maxFb = await _context.Feedbacks
            .Find(_ => true)
            .SortByDescending(f => f.FeedbackId)
            .FirstOrDefaultAsync();

        return maxFb == null ? 1 : maxFb.FeedbackId + 1;
    }
}
