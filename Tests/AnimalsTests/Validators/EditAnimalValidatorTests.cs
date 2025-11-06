using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Animals.Commands;
using Domain;
using Domain.Enums;
using FluentValidation.TestHelper;
using WebAPI.Validators.Animals;

namespace Tests.AnimalsTests.Validators
{
    //codacy: ignore[complexity]


    /// <summary>
    /// Unit tests for <see cref="EditAnimalValidator"/> to ensure the animal state is valid.
    /// The others properties were tested in CreateAnimalValidatorTests.
    /// </summary>
    public class EditAnimalValidatorTests
    {
        private readonly EditAnimalValidator _validator = new();

        private static Animal CreateValidAnimal()
        {
            return new Animal
            {
                Id = Guid.NewGuid().ToString(),
                Name = "TestAnimal",
                Colour = "Brown",
                BreedId = Guid.NewGuid().ToString(),
                ShelterId = Guid.NewGuid().ToString(),
                BirthDate = new DateOnly(2020, 1, 1),
                Cost = 50.00m,
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                Description = "Test description",
                Features = "Friendly animal",
                Sterilized = true
            };
        }

        [Theory]
        [InlineData(AnimalState.Available)]
        [InlineData(AnimalState.TotallyFostered)]
        [InlineData(AnimalState.PartiallyFostered)]
        [InlineData(AnimalState.HasOwner)]
        [InlineData(AnimalState.Inactive)]
        public void Should_Pass_When_ValidState(AnimalState state)
        {
            var animal = CreateValidAnimal();
            animal.AnimalState = state;

            var command = new EditAnimal.Command
            {
                Animal = animal
            };

            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(c => c.Animal.AnimalState);
        }

        [Theory]
        [InlineData((AnimalState)(-1))]
        [InlineData((AnimalState)6)]
        [InlineData((AnimalState)99)]
        [InlineData((AnimalState)int.MinValue)]
        [InlineData((AnimalState)int.MaxValue)]
        public void Should_Fail_When_InvalidState(AnimalState state)
        {
            var animal = CreateValidAnimal();
            animal.AnimalState = state;

            var command = new EditAnimal.Command
            {
                Animal = animal
            };

            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(c => c.Animal.AnimalState);
        }
    }
}

