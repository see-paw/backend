using Domain.Enums;
using WebAPI.DTOs;
using WebAPI.Validators;

namespace Tests.AnimalControllerTests
{
    //codacy: ignore[complexity]
    public class EditAnimalTests
    {
        private readonly EditAnimalValidator _validator;

        public EditAnimalTests()
        {
            _validator = new EditAnimalValidator();
        }

        [Theory]
        [InlineData(AnimalState.Available)]
        [InlineData(AnimalState.PartiallyFostered)]
        [InlineData(AnimalState.TotallyFostered)]
        [InlineData(AnimalState.HasOwner)]
        [InlineData(AnimalState.Inactive)]
        public void ValidAnimalState(AnimalState state)
        {
            var dto = CreateValidEditAnimalDto();
            dto.AnimalState = state;

            var result = _validator.Validate(dto);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        public void InvalidAnimalState(int invalidValue)
        {
            var dto = CreateValidEditAnimalDto();
            dto.AnimalState = (AnimalState)invalidValue;

            var result = _validator.Validate(dto);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(dto.AnimalState));
        }

        [Fact]
        public void ValidEditAnimal()
        {
            var dto = CreateValidEditAnimalDto();

            var result = _validator.Validate(dto);

            Assert.True(result.IsValid);
        }

        private ReqEditAnimalDto CreateValidEditAnimalDto()
        {
            return new ReqEditAnimalDto
            {
                Name = "Buddy",
                Species = Species.Dog,
                BreedId = "3c3c3333-3333-3333-3333-333333333333",
                Size = SizeType.Medium,
                Sex = SexType.Male,
                Colour = "Brown",
                BirthDate = new DateOnly(2020, 5, 1),
                Sterilized = true,
                Cost = 50m,
                Features = "Friendly and playful",
                Description = "A very good boy.",
                AnimalState = AnimalState.PartiallyFostered,
                Images = new List<ReqImageDto>
                {
                    new ReqImageDto
                    {
                        Url = "https://example.com/dog.jpg",
                        isPrincipal = true
                    }
                }
            };
        }
    }
}
