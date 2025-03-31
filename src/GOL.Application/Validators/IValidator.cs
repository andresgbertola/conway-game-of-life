namespace GOL.Application.Validators
{
    /// <summary>
    /// Validator interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IValidator<T>
    {
        /// <summary>
        /// Validates the input and returns a Validation Result.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        ValidationResult Validate(T input);
    }
}