using System.Reflection;
using Scalar.AspNetCore;
using Transportation.API;
using Transportation.Application;
using Transportation.Infrastructure;
using Transportation.Shared;
using Transportation.Shared.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton(TimeProvider.System);

builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration, builder.Environment.IsDevelopment());

builder.Services.AddJwtConfiguration(builder.Configuration);
builder.Services.AddOpenApiDoc();

const string FrontendCorsPolicy = "Frontend";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services
    .AddProblemDetails()
    .AddExceptionHandler<ExceptionToProblemDetailsHandler>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
builder.Services.AddScoped<CurrentUserMiddleware>();

builder.Services.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()); });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => { options.Title = "Transportation API"; });
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<CurrentUserMiddleware>();

app.MapControllers();

app.Run();