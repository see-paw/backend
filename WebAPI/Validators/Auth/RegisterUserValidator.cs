using FluentValidation;

public class ReqRegisterUserValidator : AbstractValidator<ReqRegisterUserDto>
{
    public ReqRegisterUserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(255);

        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("BirthDate is required.")
            .LessThan(DateTime.UtcNow).WithMessage("BirthDate cannot be in the future.")
            .GreaterThan(DateTime.UtcNow.AddYears(-100)).WithMessage("BirthDate is unrealistic."); // sanity limit

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street is required.")
            .MaximumLength(255);

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(255);

        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("PostalCode is required.")
            .MaximumLength(255);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
            // Optional but recommended strength checks:
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.SelectedRole)
            .NotEmpty().WithMessage("SelectedRole is required.")
            .Must(role => role == "User" || role == "AdminCAA")
            .WithMessage("SelectedRole must be either 'User' or 'AdminCAA'.");
    }
}
