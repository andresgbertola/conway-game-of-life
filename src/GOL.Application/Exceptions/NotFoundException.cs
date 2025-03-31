namespace GOL.Application.Exceptions
{
    /// <summary>
    /// Represents a NotFoundException.
    /// </summary>
    public class NotFoundException : Exception
    {
        /// <summary>
        /// C-tor.
        /// </summary>
        /// <param name="message"></param>
        public NotFoundException(string message)
            : base(message)
        {
        }
    }
}
