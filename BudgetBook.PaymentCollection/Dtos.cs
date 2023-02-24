using System.ComponentModel.DataAnnotations;


namespace BudgetBook.PaymentCollection;

public record PaymentDto(

    [Required] Guid Id,
    Guid UserId,
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

public record PaymentUpdateDto(

    [Required] string Category,
    [Required] string Company,
    [Required] decimal Amount,
    [Required] bool IsIncome,
    [Required] DateTime? Date,
    string Note
);

public record SaldoDto(
    decimal Saldo
);