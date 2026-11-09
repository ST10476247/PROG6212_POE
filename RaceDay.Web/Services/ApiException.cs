namespace RaceDay.Web.Services
{
    /// <summary>
    /// Thrown when the RaceDay API returns a non-success status code that the
    /// UI should present gracefully (never as a raw stack trace).
    /// </summary>
    public class ApiException : Exception
    {
        public int StatusCode { get; }

        public ApiException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
