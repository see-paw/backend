using Domain.Enums;
using FluentValidation;
using WebAPI.DTOs;

namespace WebAPI.Validators
{
    /// <summary>
    /// Provides validation rules for the <see cref="ReqEditAnimalDto"/> object.
    /// </summary>
    /// <remarks>
    /// This validator extends the <see cref="CreateAnimalValidator"/> by including all 
    /// creation rules and adding specific validation logic for editable fields.
    /// <para>
    /// It ensures that:
    /// <list type="bullet">
    ///   <item><description>All creation constraints are respected (via <c>Include()</c>).</description></item>
    ///   <item><description>The <see cref="ReqEditAnimalDto.AnimalState"/> property contains a valid enumeration value.</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public class EditAnimalValidator : AbstractValidator<ReqEditAnimalDto>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditAnimalValidator"/> class.
        /// </summary>
        /// <remarks>
        /// The validator includes the base rules from <see cref="CreateAnimalValidator"/> 
        /// and adds an additional validation for <see cref="ReqEditAnimalDto.AnimalState"/>.
        /// </remarks>
        public EditAnimalValidator()
        {
            // Reuse all validation rules from the CreateAnimalValidator
            Include(new CreateAnimalValidator());

            // Additional rule specific to editing
            RuleFor(x => x.AnimalState)
                .IsInEnum()
                .WithMessage("Invalid animal state.");
        }
    }
}