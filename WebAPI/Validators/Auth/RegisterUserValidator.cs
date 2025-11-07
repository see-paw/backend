using Application.Common;
using FluentValidation;
using WebAPI.DTOs.Auth;

namespace WebAPI.Validators.Auth
{
    /// <summary>
    /// Validator for <see cref="ReqRegisterUserDto"/>.
    /// Ensures that:
    /// - All user personal fields are provided and valid.
    /// - Email and password follow authentication constraints.
    /// - Role is either "User" or "AdminCAA".
    /// - If the role is "AdminCAA", all shelter-related fields become required.
    /// </summary>
    public class RegisterUserValidator : AbstractValidator<ReqRegisterUserDto>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterUserValidator"/> class.
        /// </summary>
        public RegisterUserValidator()
        {
            // ----- USER VALIDATION (ALWAYS REQUIRED) -----

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .NotNull().WithMessage("Name cannot be null.")
                .MinimumLength(2)
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
                .Matches(@"^\d{4}-\d{3}$")
                .WithMessage("Postal Code must be in the format 0000-000.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
                // Recommended strong password policy
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.SelectedRole)
                .NotEmpty().WithMessage("SelectedRole is required.")
                .Must(role => role == AppRoles.User || role == AppRoles.AdminCAA)
                .WithMessage("SelectedRole must be either 'User' or 'AdminCAA'.");


            // ----- CONDITIONAL VALIDATION (ONLY IF ROLE == AdminCAA) -----
            When(x => x.SelectedRole == AppRoles.AdminCAA, () =>
            {
                RuleFor(x => x.ShelterName)
                    .NotEmpty().WithMessage("Shelter name is required for AdminCAA accounts.")
                    .Length(2, 255);

                RuleFor(x => x.ShelterStreet)
                    .NotEmpty().WithMessage("Shelter street is required.")
                    .MaximumLength(255);

                RuleFor(x => x.ShelterCity)
                    .NotEmpty().WithMessage("Shelter city is required.")
                    .MaximumLength(100);

                RuleFor(x => x.ShelterPostalCode)
                    .NotEmpty().WithMessage("Shelter postal code is required.")
                    .Matches(@"^\d{4}-\d{3}$")
                    .WithMessage("Shelter Postal Code must be in the format 0000-000.");

                RuleFor(x => x.ShelterPhone)
                    .NotEmpty().WithMessage("Shelter phone number is required.")
                    .Matches(@"^[29]\d{8}$")
                    .WithMessage("Phone must have 9 digits and start with 2 or 9.");

                RuleFor(x => x.ShelterNIF)
                    .NotEmpty().WithMessage("Shelter NIF is required.")
                    .Matches(@"^\d{9}$")
                    .WithMessage("NIF must contain exactly 9 digits.");

                RuleFor(x => x.ShelterOpeningTime)
                    .NotEmpty().WithMessage("Opening time is required.");

                RuleFor(x => x.ShelterClosingTime)
                    .NotEmpty().WithMessage("Closing time is required.");
            });
        }
    }
}
