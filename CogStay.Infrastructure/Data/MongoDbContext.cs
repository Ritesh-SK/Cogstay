using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using CogStay.Domain.Entities;

namespace CogStay.Infrastructure.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration["MongoDB:ConnectionString"] 
            ?? configuration["MONGODB_CONNECTION_STRING"] 
            ?? "mongodb://localhost:27017";

        var databaseName = configuration["MongoDB:DatabaseName"] 
            ?? configuration["MONGODB_DATABASE_NAME"] 
            ?? "CogStayDb";

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);

        // Asynchronously initialize indexes without blocking startup
        Task.Run(InitializeIndexesAsync);
    }

    public IMongoDatabase Database => _database;

    public IMongoCollection<Guest> Guests => _database.GetCollection<Guest>("Guests");
    public IMongoCollection<Staff> Staff => _database.GetCollection<Staff>("Staff");
    public IMongoCollection<Room> Rooms => _database.GetCollection<Room>("Rooms");
    public IMongoCollection<Reservation> Reservations => _database.GetCollection<Reservation>("Reservations");
    public IMongoCollection<StayRecord> StayRecords => _database.GetCollection<StayRecord>("StayRecords");
    public IMongoCollection<Billing> Billings => _database.GetCollection<Billing>("Billings");
    public IMongoCollection<HousekeepingTask> HousekeepingTasks => _database.GetCollection<HousekeepingTask>("HousekeepingTasks");
    public IMongoCollection<Feedback> Feedbacks => _database.GetCollection<Feedback>("Feedbacks");
    public IMongoCollection<OtpRecord> Otps => _database.GetCollection<OtpRecord>("Otps");
    public IMongoCollection<RefreshToken> RefreshTokens => _database.GetCollection<RefreshToken>("RefreshTokens");

    public async Task InitializeIndexesAsync()
    {
        try
        {
            // Guests Indexes
            var guestEmailIndex = new CreateIndexModel<Guest>(
                Builders<Guest>.IndexKeys.Ascending(g => g.Email),
                new CreateIndexOptions { Unique = true });
            var guestPhoneIndex = new CreateIndexModel<Guest>(
                Builders<Guest>.IndexKeys.Ascending(g => g.PhoneNumber),
                new CreateIndexOptions { Unique = true });
            var guestIdIndex = new CreateIndexModel<Guest>(
                Builders<Guest>.IndexKeys.Ascending(g => g.GuestId));
            await Guests.Indexes.CreateManyAsync(new[] { guestEmailIndex, guestPhoneIndex, guestIdIndex });

            // Staff Indexes
            var staffEmailIndex = new CreateIndexModel<Staff>(
                Builders<Staff>.IndexKeys.Ascending(s => s.Email),
                new CreateIndexOptions { Unique = true });
            var staffIdIndex = new CreateIndexModel<Staff>(
                Builders<Staff>.IndexKeys.Ascending(s => s.StaffId));
            await Staff.Indexes.CreateManyAsync(new[] { staffEmailIndex, staffIdIndex });

            // Rooms Indexes
            var roomNumberIndex = new CreateIndexModel<Room>(
                Builders<Room>.IndexKeys.Ascending(r => r.RoomNumber),
                new CreateIndexOptions { Unique = true });
            var roomIdIndex = new CreateIndexModel<Room>(
                Builders<Room>.IndexKeys.Ascending(r => r.RoomId));
            await Rooms.Indexes.CreateManyAsync(new[] { roomNumberIndex, roomIdIndex });

            // Reservations Indexes
            var resGuestIdIndex = new CreateIndexModel<Reservation>(
                Builders<Reservation>.IndexKeys.Ascending(r => r.GuestId));
            var resRoomIdIndex = new CreateIndexModel<Reservation>(
                Builders<Reservation>.IndexKeys.Ascending(r => r.RoomId));
            var resIdIndex = new CreateIndexModel<Reservation>(
                Builders<Reservation>.IndexKeys.Ascending(r => r.ReservationId));
            await Reservations.Indexes.CreateManyAsync(new[] { resGuestIdIndex, resRoomIdIndex, resIdIndex });

            // Otps Indexes
            var otpUserTypeIndex = new CreateIndexModel<OtpRecord>(
                Builders<OtpRecord>.IndexKeys.Ascending(o => o.UserId).Ascending(o => o.OtpType));
            var otpTargetTypeIndex = new CreateIndexModel<OtpRecord>(
                Builders<OtpRecord>.IndexKeys.Ascending(o => o.Target).Ascending(o => o.OtpType));
            var otpTtlIndex = new CreateIndexModel<OtpRecord>(
                Builders<OtpRecord>.IndexKeys.Ascending(o => o.ExpiresAt),
                new CreateIndexOptions { ExpireAfter = TimeSpan.Zero });
            await Otps.Indexes.CreateManyAsync(new[] { otpUserTypeIndex, otpTargetTypeIndex, otpTtlIndex });

            // RefreshTokens Index
            var refreshTokenIndex = new CreateIndexModel<RefreshToken>(
                Builders<RefreshToken>.IndexKeys.Ascending(rt => rt.Token),
                new CreateIndexOptions { Unique = true });
            await RefreshTokens.Indexes.CreateOneAsync(refreshTokenIndex);
        }
        catch (Exception ex)
        {
            // Logging index creation warning
            Console.WriteLine($"[MongoDB Index Initialization Warning] {ex.Message}");
        }
    }
}
