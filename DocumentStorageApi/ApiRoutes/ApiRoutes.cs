namespace DocumentStorageApi.Routing;

/// <summary>
/// Routes distinguishing possible API versions
/// </summary>
public static class ApiRoutes
{
    /// <summary>
    /// The basic documents API URL routing services to appropriate REST API version and controller
    /// </summary>
    public const string DocumentsControllerURL = "[controller]"; // or better: "api/v1.0/[controller]";
}