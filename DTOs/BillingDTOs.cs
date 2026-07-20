using System.ComponentModel.DataAnnotations;
using CogStayMVC.Enums;

namespace CogStayMVC.DTOs;

public class BillingResponseDTO
{
    public int BillId { get; set; }
    public int StayId { get; set; }
    public string GuestName { get; set; } = null!;
    public string RoomNumber { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? Remarks { get; set; }
}

public class CreateBillDTO
{
    [Required(ErrorMessage = "Stay ID is required.")]
    public int StayId { get; set; }

    [Required]
    [Range(0.00, 1000000.00)]
    public decimal TotalAmount { get; set; }

    public string? Remarks { get; set; }
}

public class ProcessPaymentDTO
{
    [Required(ErrorMessage = "Bill ID is required.")]
    public int BillId { get; set; }

    [Required(ErrorMessage = "Payment method/remarks is required.")]
    public string Remarks { get; set; } = "Paid via Cash / Card at Front Desk";
}
