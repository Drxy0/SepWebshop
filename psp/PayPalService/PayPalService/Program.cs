using Microsoft.EntityFrameworkCore;
using PayPalService.Clients;
using PayPalService.Clients.Interfaces;
using PayPalService.Config;
using PayPalService.Persistance;
using PayPalService.Services;
using PayPalService.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PayPalDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.Configure<PayPalSettings>(builder.Configuration.GetSection("PayPal"));

builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IWebshopClient, WebshopClient>();

builder.Services.AddHttpClient<PayPalClient>();
builder.Services.AddHttpClient("DataServiceClient", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ApiSettings:DataServiceBaseUrl"]
            ?? throw new Exception("DataService URL is missing")
    );
});

builder.Services.AddScoped<PayPalGatewayService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors();

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
