namespace AISEP.Common
{
    public static class ApiResponse
    {
        public static object Success(object? data = null) =>
            new { success = true, data };

        public static object Fail(string message) =>
            new { success = false, message };
    }
}
