using Microsoft.EntityFrameworkCore;
using OrdersHandler.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<Context>(options =>
    options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Orders;Trusted_Connection=True")
);
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
