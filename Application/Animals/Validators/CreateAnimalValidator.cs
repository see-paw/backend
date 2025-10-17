using Application.Animals.Commands;
using FluentValidation;

namespace Application.Animals.Validators;
public class CreateAnimalValidator : AbstractValidator<CreateAnimal.Command> 
{
    public CreateAnimalValidator()
    {
        RuleFor(x => x.AnimalDto.Name).NotEmpty().WithMessage("Name is required");
    }
}
