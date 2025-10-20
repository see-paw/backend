using Application.Animals.Commands;
using FluentValidation;

namespace WebAPI.Validators;
//public class CreateAnimalValidator : AbstractValidator<CreateAnimal.Command> 
//{
//    public CreateAnimalValidator()
//    {
//        RuleFor(x => x.Animal.Name).NotEmpty().WithMessage("Name is required");
//        RuleFor(x => x.Animal.ShelterId).MustBeValidGuidString("Shelter ID");
//        RuleFor(x => x.Animal.BreedId).MustBeValidGuidString("Shelter ID");
//    }
//}
