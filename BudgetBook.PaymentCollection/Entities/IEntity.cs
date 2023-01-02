using System;

namespace BudgetBook.PaymentCollection.Entities;

public interface IEntity
{
    Guid Id { get; set; }
}