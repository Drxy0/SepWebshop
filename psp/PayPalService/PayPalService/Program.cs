using PayPalService.Clients;
using PayPalService.Config;
using PayPalService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<PayPalSettings>(
    builder.Configuration.GetSection("PayPal"));

builder.Services.Configure<PSPDataServiceSettings>(
    builder.Configuration.GetSection("PSPDataService"));


builder.Services.AddHttpClient<PayPalClient>();
builder.Services.AddHttpClient<PSPDataServiceClient>();

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
