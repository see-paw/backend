using API.DTOs;
using Application.Animals.DTOs;
using Application.Animals.Validators;

public class CreateAnimalValidator : BaseAnimalValidator<CreateAnimalDTO, CreateAnimalDTO>
{
    public CreateAnimalValidator() : base(x => x) { }
}
