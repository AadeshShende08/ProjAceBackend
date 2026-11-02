using BusinessManagement.Api.Infrastructure;
using BusinessManagement.Api.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});
builder.Services.AddHealthChecks();
builder.Services.AddBusinessManagementModules(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Business Management API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT access token. Do not include the Bearer prefix."
    });
    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            []
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseBusinessManagementPipeline();
app.UseAuthentication();
app.UseMiddleware<BusinessManagement.Api.Security.TenantResolutionMiddleware>();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapPost("/api/v1/auth/login", async (
    PasswordLoginRequest request,
    IServiceProvider services,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new { message = "Email and password are required." });
    }

    SupabaseTokenResponse? tokenResponse;
    try
    {
        SupabaseAuthenticationService authenticationService = services.GetRequiredService<SupabaseAuthenticationService>();
        tokenResponse = await authenticationService.SignInWithPasswordAsync(
            request.Email,
            request.Password,
            cancellationToken);
    }
    catch (InvalidOperationException exception)
    {
        return Results.Problem(
            title: "Authentication provider is not configured.",
            detail: exception.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
    catch (HttpRequestException)
    {
        return Results.Problem(
            title: "Authentication provider is unavailable.",
            statusCode: StatusCodes.Status502BadGateway);
    }

    return tokenResponse is null
        ? Results.Unauthorized()
        : Results.Ok(tokenResponse);
})
    .AllowAnonymous()
    .WithTags("Authentication")
    .WithSummary("Sign in with Supabase email/password authentication")
    .Produces<SupabaseTokenResponse>()
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status401Unauthorized)
    .Produces(StatusCodes.Status502BadGateway)
    .Produces(StatusCodes.Status503ServiceUnavailable);
app.MapGet("/api/v1/security/context", (BusinessManagement.Application.Security.ICurrentUser currentUser, BusinessManagement.Application.Security.ICurrentBusiness currentBusiness) =>
    Results.Ok(new { currentUser.UserId, currentBusiness.BusinessId, currentBusiness.RoleId }))
    .RequireAuthorization("BusinessMember");

app.Run();
