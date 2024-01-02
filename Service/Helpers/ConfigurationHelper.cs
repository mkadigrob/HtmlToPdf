namespace HtmlToPdfService.Helpers
{
    public static class ConfigurationHelper
    {
        public static string GetStoragePath(this IConfiguration configuration)
        {
            return configuration.GetValue<string>("StoragePath");
        }

        public static int GetWorkersNumber(this IConfiguration configuration)
        {
            return configuration.GetValue<int>("WorkersNumber");
        }
    }
}
