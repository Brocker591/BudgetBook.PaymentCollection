using BudgetBook.PaymentCollection.Entities;

namespace BudgetBook.PaymentCollection;

public static class Extensions
{
    public static PaymentDto AsDto(this Payment payment)
    {
        return new PaymentDto
        (
            payment.Id,
            payment.UserId,
            payment.Category,
            payment.Company,
            payment.Amount,
            payment.IsIncome,
            payment.Date,
            payment.Note
        );
    }
}