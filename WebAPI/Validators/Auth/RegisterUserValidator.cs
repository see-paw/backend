using FluentValidation;

public class ReqRegisterUserValidator : AbstractValidator<ReqRegisterUserDto>
{
    public ReqRegisterUserValidator()
    {
        // USER (always required)
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(255);

        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("BirthDate is required.")
            .LessThan(DateTime.UtcNow).WithMessage("BirthDate cannot be in the future.")
            .GreaterThan(DateTime.UtcNow.AddYears(-100)).WithMessage("BirthDate is unrealistic."); 

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

        When(x => x.SelectedRole == "AdminCAA", () =>
        {
            RuleFor(x => x.ShelterName)
                .NotEmpty()
                .Length(2, 255);

            RuleFor(x => x.ShelterStreet)
                .NotEmpty()
                .MaximumLength(255);

            RuleFor(x => x.ShelterCity)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.ShelterPostalCode)
                .NotEmpty()
                .Matches(@"^\d{4}-\d{3}$")
                .WithMessage("Shelter Postal Code must be in the format 0000-000.");

            RuleFor(x => x.ShelterPhone)
                .NotEmpty()
                .Matches(@"^[29]\d{8}$")
                .WithMessage("Phone must have 9 digits and start with 2 or 9.");

            RuleFor(x => x.ShelterNIF)
                .NotEmpty()
                .Matches(@"^\d{9}$")
                .WithMessage("NIF must contain exactly 9 digits.");

            RuleFor(x => x.ShelterOpeningTime)
                .NotEmpty()
                .WithMessage("Opening time is required.");

            RuleFor(x => x.ShelterClosingTime)
                .NotEmpty()
                .WithMessage("Closing time is required.");

        });

    }
}
