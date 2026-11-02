namespace BusinessManagement.Api.Infrastructure;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseBusinessManagementPipeline(this WebApplication app)
    {
        app.UseExceptionHandler();
        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.UseHttpsRedirection();
        }

        return app;
    }
}
