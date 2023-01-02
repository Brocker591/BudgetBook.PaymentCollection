using BudgetBook.Payment.Entities;

namespace BudgetBook.Payment;

public static class Extensions
{
    public static PaymentDto AsDto(this Payment payment)
    {
        return new PaymentDto
        (
            payment.Id,
            payment.Category,
            payment.Company,
            payment.Amount,
            payment.PaymentType,
            payment.Date,
            payment.Note
        );
    }
}