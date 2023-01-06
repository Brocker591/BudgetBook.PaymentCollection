using BudgetBook.PaymentCollection.Entities;
using BudgetBook.PaymentCollection.Repositories;
using BudgetBook.PaymentCollection.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

ServiceSettings serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
MongoDbSettings mongoDbSettings = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();

builder.Services.AddCors(o =>
             {
                 o.AddPolicy("AllowSetOrigins", options =>
                 {

                     options.AllowAnyOrigin();
                     options.AllowAnyHeader();
                     options.AllowAnyMethod();

                 });
             });

// Add services to the container.


builder.Services.AddControllers();



BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
BsonSerializer.RegisterSerializer(new DateTimeSerializer(BsonType.String));


builder.Services.AddSingleton<IRepository<Payment>>(serviceProvider =>
{

    var mongoClient = new MongoClient(mongoDbSettings.ConnectionString);
    var database = mongoClient.GetDatabase(serviceSettings.ServiceName);
    return new MongoRepository<Payment>(database, serviceSettings.ServiceName);
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));




// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
