namespace GOL.Domain.Exceptions
{
    /// <summary>
    /// Custom Error Exception.
    /// </summary>
    public class CustomErrorException : Exception
    {
        public readonly int HttpStatusCode;

        /// <summary>
        /// C-tor.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="httpStatusCode">Http status code that it will return in the API.</param>
        public CustomErrorException(string message, int httpStatusCode)
            : base(message)
        {
            HttpStatusCode = httpStatusCode;
        }
    }
}