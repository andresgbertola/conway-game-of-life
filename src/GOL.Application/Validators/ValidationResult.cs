namespace GOL.Application.Validators
{
    /// <summary>
    /// Validation Result.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// List of errors found.
        /// </summary>
        public List<string> Errors { get; } = new List<string>();

        /// <summary>
        /// Return if valid.
        /// </summary>
        public bool IsValid => !Errors.Any();
    }
}
