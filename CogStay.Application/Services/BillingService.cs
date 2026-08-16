using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CogStay.Application.Contracts.Persistence;
using CogStay.Application.Contracts.Services;
using CogStay.Application.DTOs;
using CogStay.Domain.Entities;
using CogStay.Domain.Enums;

namespace CogStay.Application.Services;

public class BillingService : IBillingService
{
    private readonly IBillingRepository _billingRepository;
    private readonly IStayRecordRepository _stayRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IRoomRepository _roomRepository;

    public BillingService(
        IBillingRepository billingRepository,
        IStayRecordRepository stayRepository,
        IReservationRepository reservationRepository,
        IRoomRepository roomRepository)
    {
        _billingRepository = billingRepository;
        _stayRepository = stayRepository;
        _reservationRepository = reservationRepository;
        _roomRepository = roomRepository;
    }

    public async Task<IEnumerable<BillingResponseDTO>> GetAllBillsAsync()
    {
        var bills = await _billingRepository.GetAllAsync();
        return await MapToDTOListAsync(bills);
    }

    public async Task<BillingResponseDTO?> GetBillByIdAsync(int id)
    {
        var bill = await _billingRepository.GetByIdAsync(id);
        if (bill == null) return null;
        return await MapToDTOAsync(bill);
    }

    public async Task<BillingResponseDTO?> GetBillByStayIdAsync(int stayId)
    {
        var bill = await _billingRepository.GetByStayIdAsync(stayId);
        if (bill == null) return null;
        return await MapToDTOAsync(bill);
    }

    public async Task<BillingResponseDTO> GenerateBillForStayAsync(int stayId, string? remarks = null)
    {
        var existingBill = await _billingRepository.GetByStayIdAsync(stayId);
        if (existingBill != null) return await MapToDTOAsync(existingBill);

        var stay = await _stayRepository.GetByIdAsync(stayId);
        if (stay == null)
        {
            throw new KeyNotFoundException($"Stay record with ID {stayId} not found.");
        }

        var reservation = await _reservationRepository.GetByIdAsync(stay.ReservationId);
        var room = reservation != null ? await _roomRepository.GetByIdAsync(reservation.RoomId) : null;

        int nights = 1;
        if (reservation != null)
        {
            nights = (reservation.CheckOutDate - reservation.CheckInDate).Days;
            if (nights <= 0) nights = 1;
        }

        decimal price = room?.PricePerNight ?? 100m;
        decimal totalAmount = nights * price;

        var nextId = await _billingRepository.GetNextBillIdAsync();
        var bill = new Billing
        {
            BillId = nextId,
            StayId = stay.StayId,
            GuestId = stay.GuestId,
            GuestName = stay.GuestName,
            TotalAmount = totalAmount,
            PaymentStatus = PaymentStatus.Pending,
            Remarks = remarks ?? $"Auto-generated bill for {nights} nights @ {price:C}/night"
        };

        await _billingRepository.CreateAsync(bill);

        stay.BillingReference = $"BILL-{bill.BillId}";
        await _stayRepository.UpdateAsync(stay);

        return await MapToDTOAsync(bill);
    }

    public async Task<BillingResponseDTO> CreateBillAsync(CreateBillDTO dto)
    {
        var stay = await _stayRepository.GetByIdAsync(dto.StayId);
        if (stay == null)
        {
            throw new KeyNotFoundException($"Stay record with ID {dto.StayId} not found.");
        }

        var nextId = await _billingRepository.GetNextBillIdAsync();
        var bill = new Billing
        {
            BillId = nextId,
            StayId = stay.StayId,
            GuestId = stay.GuestId,
            GuestName = stay.GuestName,
            TotalAmount = dto.TotalAmount,
            PaymentStatus = PaymentStatus.Pending,
            Remarks = dto.Remarks
        };

        await _billingRepository.CreateAsync(bill);

        stay.BillingReference = $"BILL-{bill.BillId}";
        await _stayRepository.UpdateAsync(stay);

        return await MapToDTOAsync(bill);
    }

    public async Task ProcessPaymentAsync(ProcessPaymentDTO dto)
    {
        var bill = await _billingRepository.GetByIdAsync(dto.BillId);
        if (bill == null)
        {
            throw new KeyNotFoundException($"Bill with ID {dto.BillId} not found.");
        }

        bill.PaymentStatus = PaymentStatus.Paid;
        bill.Remarks = (bill.Remarks ?? "") + $" | Paid: {dto.Remarks}";

        await _billingRepository.UpdateAsync(bill);
    }

    public async Task DeleteBillAsync(int id)
    {
        await _billingRepository.DeleteAsync(id);
    }

    private async Task<IEnumerable<BillingResponseDTO>> MapToDTOListAsync(IEnumerable<Billing> list)
    {
        var dtos = new List<BillingResponseDTO>();
        foreach (var b in list)
        {
            dtos.Add(await MapToDTOAsync(b));
        }
        return dtos;
    }

    private async Task<BillingResponseDTO> MapToDTOAsync(Billing b)
    {
        var stay = await _stayRepository.GetByIdAsync(b.StayId);
        var reservation = stay != null ? await _reservationRepository.GetByIdAsync(stay.ReservationId) : null;
        var room = reservation != null ? await _roomRepository.GetByIdAsync(reservation.RoomId) : null;

        return new BillingResponseDTO
        {
            BillId = b.BillId,
            StayId = b.StayId,
            GuestName = b.GuestName,
            RoomNumber = room?.RoomNumber ?? "N/A",
            TotalAmount = b.TotalAmount,
            PaymentStatus = b.PaymentStatus,
            Remarks = b.Remarks
        };
    }
}
