using Helpdesk.Infrastructure;
using Helpdesk.Infrastructure.Services;
using Helpdesk.Infrastructure.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddScoped<ITicketService, TicketService>();


builder.Services.AddDbContext<HelpdeskDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// The React dev server runs on a different port, so the browser treats calls to
// this API as cross-origin and blocks them unless the API opts in. Origin comes
// from configuration rather than being hardcoded.
const string DevCorsPolicy = "dev";
builder.Services.AddCors(options =>
    options.AddPolicy(DevCorsPolicy, policy => policy
        .WithOrigins(builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()));

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Order matters: CORS must run before the request reaches an endpoint.
app.UseCors(DevCorsPolicy);

app.MapControllers();

app.Run();

