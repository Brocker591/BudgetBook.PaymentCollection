using System.Security.Claims;
using BudgetBook.PaymentCollection.Entities;
using BudgetBook.PaymentCollection.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BudgetBook.PaymentCollection.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IRepository<Payment> paymentRepository;

    public PaymentController(IRepository<Payment> paymentRepository)
    {
        this.paymentRepository = paymentRepository;
    }

    [HttpGet("All")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAllAsync()
    {
        var items = await paymentRepository.GetAllAsync();
        var payments = items.Select(item => item.AsDto()).ToList();

        return payments;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAllFromUserAsync()
    {
        var user = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
        if (user == null)
            return BadRequest("Kein User vorhanden");

        Guid userId = new Guid(user);

        var items = (await paymentRepository.GetAllAsync()).Select(item => item.AsDto());
        var userPayments = items.Where(x => x.UserId == userId).ToList();


        return userPayments;
    }




    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentDto>> GetByIdAsync(Guid id)
    {
        var item = await paymentRepository.GetAsync(id);

        if (item == null)
            return NotFound();

        return item.AsDto();
    }

    [HttpGet("Saldo")]
    public async Task<ActionResult<SaldoDto>> GetSaldoFromUserAsync()
    {
        var user = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
        if (user == null)
            return BadRequest("Kein User vorhanden");

        Guid userId = new Guid(user);

        decimal totalSaldo = 0;


        var items = (await paymentRepository.GetAllAsync()).Where(x => x.UserId == userId).ToList();

        foreach (var item in items)
        {
            if (item.IsIncome)
            {
                totalSaldo = totalSaldo + item.Amount;
            }
            else
            {
                totalSaldo = totalSaldo - item.Amount;
            }

        }

        return new SaldoDto(totalSaldo);


    }


    [HttpPost]
    public async Task<ActionResult<PaymentDto>> CreateAsync(PaymentCreateDto dto)
    {
        var user = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
        if (user == null)
            return BadRequest("Kein User vorhanden");

        Guid userId = new Guid(user);


        Payment payment = new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Category = dto.Category,
            Company = dto.Company,
            Amount = dto.Amount,
            IsIncome = dto.IsIncome,
            Date = dto.Date,
            Note = dto.Note
        };

        await paymentRepository.CreateAsync(payment);

        return payment.AsDto();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAsync(Guid id, PaymentUpdateDto dto)
    {
        var exitsingModel = await paymentRepository.GetAsync(id);

        if (exitsingModel is null)
            return NotFound();


        exitsingModel.Category = dto.Category;
        exitsingModel.Company = dto.Company;
        exitsingModel.Amount = dto.Amount;
        exitsingModel.IsIncome = dto.IsIncome;
        exitsingModel.Date = dto.Date;
        exitsingModel.Note = dto.Note;

        await paymentRepository.UpdateAsync(exitsingModel);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> RemoveAsync(Guid id)
    {
        var exitingDto = await paymentRepository.GetAsync(id);

        if (exitingDto is null)
            return NotFound();

        await paymentRepository.RemoveAsync(id);

        return NoContent();
    }
}