using FluentValidation;
using WebAPI.DTOs.Ownership;

namespace WebAPI.Validators.Ownership;

/// <summary>
/// Validator for creating ownership requests.
/// </summary>
public class CreateOwnershipRequestValidator : AbstractValidator<ReqCreateOwnershipRequestDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateOwnershipRequestValidator"/> class  
    /// and defines validation rules for creating ownership requests.
    /// </summary>
    public CreateOwnershipRequestValidator()
    {
        RuleFor(x => x.AnimalId)
            .NotNull().WithMessage("Animal ID cannot be null")
            .NotEmpty().WithMessage("Animal ID is required")
            .Must(BeAValidGuid).WithMessage("Animal ID must be a valid GUID");
    }
    private static bool BeAValidGuid(string? id)
    {
        return Guid.TryParse(id, out _);
    }
}