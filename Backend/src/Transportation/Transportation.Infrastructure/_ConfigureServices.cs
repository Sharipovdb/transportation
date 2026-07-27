using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Transportation.Application.Auth;
using Transportation.Application.Common.Interfaces;
using Transportation.Application.Role;
using Transportation.Application.User;
using Transportation.Domain.Entities;
using Transportation.Infrastructure.Persistence;
using UserMapper = Transportation.Application.User.UserMapper;

namespace Transportation.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDev)
    {
        services
            .AddIdentity<User, IdentityRole<long>>()
            .AddEntityFrameworkStores<TransportationDbContext>()
            .AddDefaultTokenProviders();

        services.AddPersistenceServices(configuration, isDev);
        services.AddScoped<UserMapper>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddScoped<JwtTokenService>();

        return services;
    }
}