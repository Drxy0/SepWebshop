using CryptoService.Clients;
<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> 69563e2 (Add wallet, start)
using CryptoService.Clients.Interfaces;
using CryptoService.Persistance;
using CryptoService.Services;
using CryptoService.Services.Interfaces;
<<<<<<< HEAD
=======
using CryptoService.Persistance;
using CryptoService.Services;
>>>>>>> 5cbd7fe (Add base implementation)
=======
>>>>>>> 69563e2 (Add wallet, start)
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddDbContext<CryptoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ICryptoPaymentService, CryptoPaymentService>();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<IBinanceClient, BinanceClient>();
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
builder.Services.AddHttpClient<ITestnetWallet, TestnetWallet>();
=======
>>>>>>> 5cbd7fe (Add base implementation)
=======
builder.Services.AddHttpClient<ITestnetWallet, TestnetWallet>();
>>>>>>> 69563e2 (Add wallet, start)
=======
>>>>>>> b85b831 (Add qr code endpoint)

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
