namespace GOL.WebApi.Middlewares
{
    /// <summary>
    /// Api Error.
    /// </summary>
    public class ApiError
    {
        /// <summary>
        /// Error message.
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Error messages.
        /// </summary>
        public List<string> Errors { get; set; }

        /// <summary>
        /// Error timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
