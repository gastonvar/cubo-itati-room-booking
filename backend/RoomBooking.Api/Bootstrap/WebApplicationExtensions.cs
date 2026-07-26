namespace RoomBooking.Api.Bootstrap;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
        app.MapControllers();
        return app;
    }
}
