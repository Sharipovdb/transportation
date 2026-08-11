using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Transportation.API;
using Transportation.Application;
using Transportation.Infrastructure;
using Transportation.Infrastructure.Persistence;
using Transportation.Shared;
using Transportation.Shared.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // The LAN entry lets colleagues on the office Wi-Fi reach the dev frontend.
        // It is this machine's DHCP address — re-check `ipconfig` if requests start
        // failing CORS after a reconnect.
        policy.WithOrigins("http://localhost:3000", "http://192.168.1.214:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

builder.Services.AddSingleton(TimeProvider.System);

builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration, builder.Environment.IsDevelopment());

builder.Services.AddJwtConfiguration(builder.Configuration);
builder.Services.AddOpenApiDoc();

builder.Services
    .AddProblemDetails()
    .AddExceptionHandler<ExceptionToProblemDetailsHandler>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
builder.Services.AddScoped<CurrentUserMiddleware>();

builder.Services.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()); });

var app = builder.Build();

app.UseCors("AllowFrontend"); 

// await using (var scope = app.Services.CreateAsyncScope())
// {
//     var dbContext = scope.ServiceProvider.GetRequiredService<TransportationDbContext>();
//     await dbContext.Database.MigrateAsync();
// }

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => { options.Title = "Transportation API"; });
    app.MapGet("/", () => Results.Redirect("/scalar"));
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<CurrentUserMiddleware>();

app.MapControllers();

app.Run();