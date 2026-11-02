namespace BusinessManagement.Api.Authentication;

public sealed record PasswordLoginRequest(string Email, string Password);
