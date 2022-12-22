using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Contoso.Models;
using Constants = Contoso.Repository.Constants;
using Contoso.Repository.Sql;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ContosoContext>(o => o.UseSqlServer(
        Constants.SqlAzureConnectionString,
        o => o.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds)));
builder.Services.AddScoped<ICustomerRepository, SqlCustomerRepository>();
builder.Services.AddScoped<IOrderRepository, SqlOrderRepository>();
builder.Services.AddScoped<IProductRepository, SqlProductRepository>();

builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);

var app = builder.Build();

app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapControllers();

app.Run(Constants.ApiUrl);
