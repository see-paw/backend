﻿using FluentValidation;
using WebAPI.DTOs;

namespace WebAPI.Validators
{
    /// <summary>
    /// Validator responsible for enforcing business rules and data integrity
    /// on the <see cref="ReqUserProfileDto"/> used in user profile updates.
    /// </summary>
    public class UserProfileValidator : AbstractValidator<ReqUserProfileDto>
    {
        public UserProfileValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(255).WithMessage("Name cannot exceed 255 characters.")
                .Matches(@"^[A-Za-zÀ-ÖØ-öø-ÿ\s'\-’]+$")
                .WithMessage("Name can only contain letters, spaces, hyphens, and apostrophes.");



            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage("Birth date is required.")
                .LessThan(DateTime.UtcNow.Date).WithMessage("Birth date cannot be in the future.")
                .GreaterThan(DateTime.UtcNow.AddYears(-100)).WithMessage("Birth date is unrealistically old.");

            
            RuleFor(x => x.Street)
                .NotEmpty().WithMessage("Street is required.")
                .MaximumLength(255).WithMessage("Street cannot exceed 255 characters.");

            
            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required.")
                .MaximumLength(255).WithMessage("City cannot exceed 255 characters.");

            RuleFor(x => x.PostalCode)
                .NotEmpty().WithMessage("Postal code is required.")
                .Matches(@"^[A-Za-z0-9\- ]{4,10}$").WithMessage("Invalid postal code format.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^9\d{8}$").WithMessage("Phone number must start with 9 and contain exactly 9 digits.")
                .Length(9).WithMessage("Phone number must have exactly 9 digits.");

        }
    }
}
