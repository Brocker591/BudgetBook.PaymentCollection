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

    [HttpGet]
    public async Task<IEnumerable<PaymentDto>> GetAllAsync()
    {
        var items = (await paymentRepository.GetAllAsync()).Select(item => item.AsDto());

        return items;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentDto>> GetByIdAsync(Guid id)
    {
        var item = await paymentRepository.GetAsync(id);

        if (item == null)
            return NotFound();

        return item.AsDto();
    }


    [HttpPost]
    public async Task<ActionResult<PaymentDto>> CreateAsync(PaymentCreateDto dto)
    {
        var user = User.FindFirst("preferred_username")?.Value;
        if (user == null)
            return BadRequest("Kein User vorhanden" + JsonConvert.SerializeObject(User));


        Payment payment = new()
        {
            Id = Guid.NewGuid(),
            UserId = user,
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
        var exitingDto = await paymentRepository.GetAsync(id);

        if (exitingDto is null)
            return NotFound();


        exitingDto.Category = dto.Category;
        exitingDto.Company = dto.Company;
        exitingDto.Amount = dto.Amount;
        exitingDto.IsIncome = dto.IsIncome;
        exitingDto.Date = dto.Date;
        exitingDto.Note = dto.Note;

        await paymentRepository.UpdateAsync(exitingDto);

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