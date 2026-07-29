using System.Reflection;
using Scalar.AspNetCore;
using Transportation.API;
using Transportation.Application;
using Transportation.Infrastructure;
using Transportation.Shared;
using Transportation.Shared.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => { options.Title = "Transportation API"; });
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<CurrentUserMiddleware>();

app.MapControllers();

app.Run();