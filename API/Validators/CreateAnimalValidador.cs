using API.DTOs;

namespace API.Validators;


public class CreateAnimalValidator : BaseAnimalValidator<CreateAnimalDTO, CreateAnimalDTO>
{
    public CreateAnimalValidator() : base(x => x) { }
}
