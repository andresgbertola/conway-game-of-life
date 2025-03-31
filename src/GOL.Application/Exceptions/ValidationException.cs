namespace GOL.Application.Exceptions
{
    /// <summary>
    /// Represents a Validation Exception.
    /// </summary>
    public class ValidationException : Exception
    {
        public List<string> Errors { get; }

        /// <summary>
        /// C-tor.
        /// </summary>
        /// <param name="errors">Error list.</param>
        public ValidationException(List<string> errors)
            : base("One or more validation failures have occurred.")
        {
            Errors = errors;
        }

        /// <summary>
        /// C-tor.
        /// </summary>
        /// <param name="error">Error list.</param>
        public ValidationException(string error)
            : base("One or more validation failures have occurred.")
        {
            Errors = new List<string> { error };
        }
    }
}
