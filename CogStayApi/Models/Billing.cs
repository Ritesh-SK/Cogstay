using CogStayMVC.Enums;

namespace CogStayMVC.Models;

public class Billing
{
    public int BillId { get; set; }
    public int StayId { get; set; }
    public int GuestId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? Remarks { get; set; }

    // Navigation Properties
    public virtual StayRecord StayRecord { get; set; } = null!;
}
