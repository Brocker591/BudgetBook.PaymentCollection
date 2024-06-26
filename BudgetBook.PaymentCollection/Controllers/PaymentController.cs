using System.Security.Claims;
using BudgetBook.Account.Contracts;
using BudgetBook.PaymentCollection.Entities;
using BudgetBook.PaymentCollection.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BudgetBook.PaymentCollection.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class PaymentController : ControllerBase
{
    private readonly ILogger<PaymentController> logger;
    private readonly IRepository<Payment> paymentRepository;
    private readonly IPublishEndpoint publishEndpoint;

    public PaymentController(IRepository<Payment> paymentRepository, IPublishEndpoint publishEndpoint, ILogger<PaymentController> logger)
    {
        this.paymentRepository = paymentRepository;
        this.publishEndpoint = publishEndpoint;
        this.logger = logger;
    }

    [HttpGet("All")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAllAsync()
    {
        try
        {
            var items = await paymentRepository.GetAllAsync();
            var payments = items.Select(item => item.AsDto()).ToList();

            return payments;
        }
        catch (System.Exception ex)
        {
            logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }

    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAllFromUserAsync()
    {
        try
        {
            var user = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
            if (user == null)
                return BadRequest("Kein User vorhanden");

            Guid userId = new Guid(user);

            var items = (await paymentRepository.GetAllAsync()).Select(item => item.AsDto());
            var userPayments = items.Where(x => x.UserId == userId).ToList();


            return userPayments;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }

    }




    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var item = await paymentRepository.GetAsync(id);

            if (item == null)
                return NotFound();

            return item.AsDto();
        }
        catch (System.Exception ex)
        {
            logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }

    }

    [HttpGet("Saldo")]
    public async Task<ActionResult<SaldoDto>> GetSaldoFromUserAsync()
    {
        try
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
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }



    }


    [HttpPost]
    public async Task<ActionResult<PaymentDto>> CreateAsync(PaymentCreateDto dto)
    {
        try
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

            logger.LogInformation("Zahlung wurde erstellt");
            //Berechnen des Queue Werts für die Zahlung
            decimal paymentAmount;

            if (dto.IsIncome)
                paymentAmount = dto.Amount;
            else
                paymentAmount = (dto.Amount * -1);


            logger.LogInformation($"Starte mit Endpoint {publishEndpoint.ToString()}");

            //Senden in die Queue
            await publishEndpoint.Publish(new UserBankAccountChange(userId, paymentAmount));

            logger.LogInformation("Message gesendet");


            return payment.AsDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }

    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAsync(Guid id, PaymentUpdateDto dto)
    {
        try
        {
            var exitsingModel = await paymentRepository.GetAsync(id);

            if (exitsingModel is null)
                return NotFound();


            //Korrigieren des Bankkontos
            decimal paymentAmount;

            if (exitsingModel.IsIncome)
                paymentAmount = (exitsingModel.Amount * -1);
            else
                paymentAmount = exitsingModel.Amount;

            await publishEndpoint.Publish(new UserBankAccountChange(exitsingModel.UserId, paymentAmount));


            exitsingModel.Category = dto.Category;
            exitsingModel.Company = dto.Company;
            exitsingModel.Amount = dto.Amount;
            exitsingModel.IsIncome = dto.IsIncome;
            exitsingModel.Date = dto.Date;
            exitsingModel.Note = dto.Note;

            await paymentRepository.UpdateAsync(exitsingModel);

            //Senden des korrekten Wertes
            if (dto.IsIncome)
                paymentAmount = dto.Amount;
            else
                paymentAmount = (dto.Amount * -1);

            await publishEndpoint.Publish(new UserBankAccountChange(exitsingModel.UserId, paymentAmount));

            return NoContent();
        }
        catch (System.Exception ex)
        {
            logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }

    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> RemoveAsync(Guid id)
    {
        try
        {
            var exitsingModel = await paymentRepository.GetAsync(id);

            if (exitsingModel is null)
                return NotFound();

            //Korrigieren des Bankkontos
            decimal paymentAmount;

            if (exitsingModel.IsIncome)
                paymentAmount = (exitsingModel.Amount * -1);
            else
                paymentAmount = exitsingModel.Amount;

            await publishEndpoint.Publish(new UserBankAccountChange(exitsingModel.UserId, paymentAmount));

            await paymentRepository.RemoveAsync(id);

            return NoContent();
        }
        catch (System.Exception ex)
        {
            logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }

    }
}