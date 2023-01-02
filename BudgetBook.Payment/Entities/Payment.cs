namespace BudgetBook.Payment.Entities;

public class Payment : IEntity
{
    public Guid Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsIncome { get; set; }
    public DateTime? Date { get; set; }
    public string Note { get; set; } = string.Empty;

}