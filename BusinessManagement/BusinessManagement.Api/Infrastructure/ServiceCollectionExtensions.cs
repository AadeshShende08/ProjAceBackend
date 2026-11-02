using BusinessManagement.Application;
using BusinessManagement.Application.Security;
using BusinessManagement.Infrastructure;
using BusinessManagement.Infrastructure.Persistence;
using BusinessManagement.Infrastructure.Security;
using BusinessManagement.Api.Security;
using BusinessManagement.Api.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace BusinessManagement.Api.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessManagementModules(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ApplicationAssemblyMarker>();
        services.AddSingleton<InfrastructureAssemblyMarker>();
        services.AddPersistence(configuration);
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddScoped<ICurrentBusiness, HttpCurrentBusiness>();
        services.AddScoped<IBusinessMembershipValidator, BusinessMembershipValidator>();
        services.AddScoped<IAuditLogger, EfAuditLogger>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddHttpClient<SupabaseAuthenticationService>((serviceProvider, client) =>
        {
            string? supabaseUrl = configuration["Authentication:Supabase:Url"];
            if (string.IsNullOrWhiteSpace(supabaseUrl))
            {
                throw new InvalidOperationException("Authentication:Supabase:Url is not configured.");
            }

            client.BaseAddress = new Uri($"{supabaseUrl.TrimEnd('/')}/");
        });

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                string? authority = configuration["Authentication:Jwt:Authority"];
                string? audience = configuration["Authentication:Jwt:Audience"];

                if (!string.IsNullOrWhiteSpace(authority))
                {
                    options.Authority = authority;
                }

                options.Audience = audience;
                options.RequireHttpsMetadata = true;
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("BusinessMember", policy => policy.RequireAuthenticatedUser());

        return services;
    }
}
