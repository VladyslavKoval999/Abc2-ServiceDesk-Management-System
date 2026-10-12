using Microsoft.EntityFrameworkCore;
using ServiceDesk.Api;
using ServiceDesk.Api.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<EquipmentService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<InvoiceService>();
builder.Services.AddScoped<PurchasingService>();
builder.Services.AddScoped<QuotationService>();
builder.Services.AddScoped<SupplierService>();
builder.Services.AddScoped<TechnicianService>();
builder.Services.AddScoped<WorkOrderService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();