using Application.Images.Commands;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Persistence;
using WebAPI.Validators.Activities;

namespace WebAPI.Validators.Images;

/// <summary>
/// Validator for the <see cref="AddImagesAnimal.Command"/> class.
/// Validates business rules at the MediatR command level, including database-dependent validations.
/// </summary>
public class AddImagesAnimalCommandValidator : AbstractValidator<AddImagesAnimal.Command>
{

    /// <summary>
    /// Initializes a new instance of the <see cref="AddImagesAnimalCommandValidator"/> class.
    /// </summary>
    public AddImagesAnimalCommandValidator()
    {
        RuleFor(x => x.AnimalId)
            .NotEmpty().WithMessage("AnimalId is required.")
            .MustBeValidGuidString("Id with wrong format");

        RuleFor(x => x)
            .Must(x => x.Files.Count == x.Images.Count)
            .WithMessage("Mismatch between files and image metadata.");

        RuleFor(x => x.Files)
            .NotEmpty().WithMessage("At least one image file is required.");

        RuleFor(x => x.Images)
            .NotEmpty().WithMessage("At least one image metadata is required.");
    }
    
}