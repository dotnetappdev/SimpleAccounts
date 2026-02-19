using Microsoft.AspNetCore.Mvc;

namespace SimpleAccounts.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    [HttpPost("stripe")]
    public async Task<IActionResult> ProcessStripePayment([FromBody] PaymentRequest request)
    {
        // Placeholder for Stripe integration
        // In production, you would use Stripe.NET SDK
        return Ok(new PaymentResponse
        {
            Success = true,
            TransactionId = $"STRIPE_{Guid.NewGuid():N}",
            Message = "Payment processed successfully via Stripe"
        });
    }

    [HttpPost("worldpay")]
    public async Task<IActionResult> ProcessWorldPayPayment([FromBody] PaymentRequest request)
    {
        // Placeholder for WorldPay integration
        return Ok(new PaymentResponse
        {
            Success = true,
            TransactionId = $"WORLDPAY_{Guid.NewGuid():N}",
            Message = "Payment processed successfully via WorldPay"
        });
    }

    [HttpPost("paypal")]
    public async Task<IActionResult> ProcessPayPalPayment([FromBody] PaymentRequest request)
    {
        // Placeholder for PayPal integration
        // In production, you would use PayPal SDK
        return Ok(new PaymentResponse
        {
            Success = true,
            TransactionId = $"PAYPAL_{Guid.NewGuid():N}",
            Message = "Payment processed successfully via PayPal"
        });
    }

    [HttpGet("methods")]
    public IActionResult GetPaymentMethods()
    {
        return Ok(new[]
        {
            new { Name = "Stripe", Enabled = true },
            new { Name = "WorldPay", Enabled = true },
            new { Name = "PayPal", Enabled = true }
        });
    }
}

public class PaymentRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "GBP";
    public string CustomerEmail { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? OrderId { get; set; }
}

public class PaymentResponse
{
    public bool Success { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
