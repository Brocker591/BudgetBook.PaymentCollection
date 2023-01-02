using System.ComponentModel.DataAnnotations;


namespace BudgetBook.Payment;

public record PaymentDto(

    [Required] Guid Id,
    [Required] string Category,
    [Required] string Company,
    [Required] decimal Amount,
    [Required] bool IsIncome,
    [Required] DateTime? Date,
    string Note
);

public record PaymentCreateDto(

    [Required] string Category,
    [Required] string Company,
    [Required] decimal Amount,
    [Required] bool IsIncome,
    [Required] DateTime? Date,
    string Note
);