using API.DTOs;

namespace API.Validators
{
    /// <summary>
    /// Validator for the <see cref="CreateAnimalDTO"/> class.
    /// </summary>
    /// <remarks>
    /// Inherits all common validation rules from <see cref="BaseAnimalValidator{T, TDto}"/>.
    /// This validator is used specifically when creating a new animal entry through the API.
    /// </remarks>
    public class CreateAnimalValidator : BaseAnimalValidator<CreateAnimalDTO, CreateAnimalDTO>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAnimalValidator"/> class
        /// and applies all base validation rules defined in <see cref="BaseAnimalValidator{T, TDto}"/>.
        /// </summary>
        public CreateAnimalValidator() : base(x => x)
        {
        }
    }
}
