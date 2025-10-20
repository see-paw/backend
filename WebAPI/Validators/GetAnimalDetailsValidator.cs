using Application.Animals.Queries;
using FluentValidation;

namespace WebAPI.Validators;

/// <summary>
/// Validator responsible for ensuring the validity of the input parameters
/// of the <see cref="GetAnimalDetails.Query"/> request.
/// </summary>
/// <remarks>
/// This validator specifically checks whether the <c>Id</c> field provided in the query
/// represents a valid GUID string, ensuring the correct format before the request
/// is processed by the application layer.
/// </remarks>
public class GetAnimalDetailsValidator : AbstractValidator<GetAnimalDetails.Query>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetAnimalDetailsValidator"/> class.
    /// </summary>
    /// <remarks>
    /// Defines the validation rules applied to the <see cref="GetAnimalDetails.Query"/> request.
    /// In this case, it enforces the <c>MustBeValidGuidString("Animal ID")</c> rule,
    /// which validates that the provided animal identifier has a valid GUID format.
    /// </remarks>
    public GetAnimalDetailsValidator()
    {
        RuleFor(x => x.Id).MustBeValidGuidString("Animal ID");
    }
}