using Domain.Enums; 
using WebAPI.DTOs; 
using WebAPI.Validators; 

namespace Tests
{
    //codacy: ignore[complexity]
    public class CreateAnimalTests
    {
        private readonly CreateAnimalValidator _validator;

        // Constructor runs before each test to ensure test isolation
        // Each test gets a fresh _validator instance with no shared state
        public CreateAnimalTests()
        {
            _validator = new CreateAnimalValidator();
        }

        [Theory]
        [InlineData(null)] // null
        [InlineData("")] // empty string
        [InlineData("   ")] // only spaces
        [InlineData("Rex123")] // contains numbers
        [InlineData("Max@")] // contains special character
        [InlineData("Luna#")] // contains special character
        [InlineData("Bella!")] // contains special character
        [InlineData("Rocky99")] // contains numbers
        [InlineData("A")] // too short (1 character)
        [InlineData("ThisIsAnExtremelyLongNameThatExceedsTheMaximumAllowedCharacterLimitForAnimalNamesInTheDatabaseBecauseItHasMoreThanOneHundredCharacters")] // too long
        public void AnimalInvalidName(string name)
        {
            var dto = CreateValidAnimalDTO();
            dto.Name = name;

            // Validate is a FluentValidation method that executes all validation rules and returns a ValidationResult
            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
            
        }

        [Theory]
        [InlineData("Rex")]
        [InlineData("Luna")]
        [InlineData("Max")]
        [InlineData("Bella Maria")] // space
        [InlineData("O'Malley")] // apostrophe
        [InlineData("Jean-Pierre")] // hyphen
        [InlineData("José")] // accented character
        [InlineData("François")] // accented character
        public void AnimalNameValid(string name)
        {
            var dto = CreateValidAnimalDTO();
            dto.Name = name;

            var result = _validator.Validate(dto);
            Assert.True(result.IsValid); ;
        }

        // Note: It is not possible to test null for non-nullable enums because:
        // 1. C# doesn't allow assigning null to non-nullable enums at compile time
        // 2. JSON deserialization fails with 400 Bad Request before reaching the _validator
        // It is only possible to test valid enum values to ensure IsInEnum() validation works correctly

        [Theory]
        [InlineData(Species.Dog)]
        [InlineData(Species.Cat)]
        public void AnimalSpeciesValid(Species species)
        {
            //Use a valid base DTO and modify only the property being tested.
            var dto = CreateValidAnimalDTO();
            dto.Species = species;

            var result = _validator.Validate(dto);
            Assert.True(result.IsValid);
        }


        [Theory]
        [InlineData(SizeType.Small)]
        [InlineData(SizeType.Medium)]
        [InlineData(SizeType.Large)]
        public void AnimalSizeValid(SizeType size)
        {
            var dto = CreateValidAnimalDTO();
            dto.Size = size;

            var result = _validator.Validate(dto);
            Assert.True(result.IsValid);
        }


        [Theory]
        [InlineData(SexType.Male)]
        [InlineData(SexType.Female)]
        public void AnimalSexValid(SexType sex)
        {
            var dto = CreateValidAnimalDTO();
            dto.Sex = sex;

            var result = _validator.Validate(dto);
            Assert.True(result.IsValid);
        }


        [Theory]
        [InlineData(null)] // null
        [InlineData("")] // empty string
        [InlineData("   ")] // only spaces
        public void AnimalBreedInvalid(string breedId)
        {
            var dto = CreateValidAnimalDTO();
            dto.BreedId = breedId;

            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
        }

        [Fact]
        public void AnimalBreedValid()
        {
            var dto = CreateValidAnimalDTO();
            dto.BreedId = Guid.NewGuid().ToString(); 

            var result = _validator.Validate(dto);
            Assert.True(result.IsValid);
        }



        [Theory]
        [InlineData(null)] // null
        [InlineData("")] // empty string
        [InlineData("   ")] // only spaces
        public void AnimalColourInvalid(string colour)
        {
            var dto = CreateValidAnimalDTO();
            dto.Colour = colour;

            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("Brown")]
        [InlineData("Light Brown")] // space
        [InlineData("Dark-Gray")] // hyphen
        public void AnimalColourValid(string colour)
        {
            var dto = CreateValidAnimalDTO();
            dto.Colour = colour;

            var result = _validator.Validate(dto);
            Assert.True(result.IsValid);
        }


        [Fact]
        public void AnimalBirthDateEmpty()
        {
            // default DateOnly is 0001-01-01 - equivalent to "empty" for other types like string or int
            var animalDTO = new ReqCreateAnimalDto { BirthDate = default };
            var result = _validator.Validate(animalDTO);

            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData(1)] // 1 day in the future
        [InlineData(7)] // 1 week in the future
        [InlineData(30)] // 1 month in the future
        [InlineData(365)] // 1 year in the future
        public void AnimalBirthDateInFuture(int daysInFuture)
        {
            var animalDTO = new ReqCreateAnimalDto
            {
                BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(daysInFuture))
            };
            var result = _validator.Validate(animalDTO);

            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData(31)] // more than 30 years
        [InlineData(35)]
        [InlineData(50)]
        public void AnimalBirthDateTooOld(int years)
        {
            var dto = CreateValidAnimalDTO();
            dto.BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-years));

            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData(0)] // today
        [InlineData(1)] // 1 year ago
        [InlineData(5)] // 5 years ago
        [InlineData(10)] // 10 years ago
        [InlineData(29)] // 29 years ago (boundary limit)
        public void AnimalBirthDateValid(int yearsAgo)
        {
            var dto = CreateValidAnimalDTO();
            dto.BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-yearsAgo));
            var result = _validator.Validate(dto);
            Assert.True(result.IsValid);
        }

        // Note: Sterilized is a non-nullable bool, so null values are rejected at deserialization level
        // bool is non-nullable, so it always has a value (default is false)
        // It's not possible to test null because C# won't allow assigning null to a non-nullable bool
        // And JSON deserialization fails with 400 Bad Request before reaching the _validator
        // So it's only possible to test valid values to ensure no validation errors occur

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AnimalSterilizedValid(bool sterilized)
        {
            var dto = CreateValidAnimalDTO();
            dto.Sterilized = sterilized;

            var result = _validator.Validate(dto);
            Assert.True(result.IsValid);
        }

        // Note: Cost is a non-nullable decimal, so null values cannot be tested because:
        // 1. C# doesn't allow assigning null to non-nullable value types at compile time
        // 2. JSON deserialization fails with 400 Bad Request before reaching the _validator

        [Theory]
        [InlineData(-1)] // negative value
        [InlineData(-10)]
        [InlineData(-100.50)]
        public void AnimalCostNegative(decimal cost)
        {
            var dto = CreateValidAnimalDTO();
            dto.Cost = cost;

            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData(1001)] // exceeds maximum
        [InlineData(1500)]
        [InlineData(9999.99)]
        public void AnimalCostExceedsMaximum(decimal cost)
        {
            var dto = CreateValidAnimalDTO();
            dto.Cost = cost;

            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData(10.123)] // 3 decimal places
        [InlineData(50.9999)] // 4 decimal places
        public void AnimalCostMoreThan2DecimalPlaces(decimal cost)
        {
            var dto = CreateValidAnimalDTO();
            dto.Cost = cost;

            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData(0)] // boundary minimum value
        [InlineData(10)]
        [InlineData(50.50)] // 2 decimal places
        [InlineData(99.99)]
        [InlineData(1000.00)] // boundary maximum value
        public void AnimalCostValid(decimal cost)
        {
            var dto = CreateValidAnimalDTO();
            dto.Cost = cost;

            var result = _validator.Validate(dto);
            Assert.True(result.IsValid);
        }

        public void AnimalsWithImagesListEmpty()//Images list needs 1 image min
        {
            var dto = CreateValidAnimalDTO();
            dto.Images = null;

            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
        }

        public void AnimalsListCannotHaveToIsPrincipalImages()
        {
            var dto = CreateValidAnimalDTO();
            dto.Images = new List<ReqImageDTO>
            {
                new()
                {
                    Url = "https://example.com/img1.jpg",
                    Description = "Main photo",
                    isPrincipal = true
                },
                new()
                {
                    Url = "https://example.com/img1.jpg",
                    Description = "Main photo",
                    isPrincipal = true
                }
            };

            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
        }

        [Fact]
        public void NoPrincipalImage()
        {
            var dto = CreateValidAnimalDTO();
            dto.Images = new List<ReqImageDTO>
            {
                new() { Url = "https://example.com/img1.jpg", Description = "Main photo", isPrincipal = false },
                new() { Url = "https://example.com/img2.jpg", Description = "Second photo", isPrincipal = false }
            };

            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
        }

        [Fact]
        public void ValidListOfImages()
        {
            var dto = CreateValidAnimalDTO();
            dto.Images = new List<ReqImageDTO>
            {
                new() { Url = "https://example.com/img1.jpg", isPrincipal = true },
                new() { Url = "https://example.com/img2.jpg", isPrincipal = false }
            };

            var result = _validator.Validate(dto);
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(null)]//null
        [InlineData("")]//empty string
        [InlineData("   ")]
        public void ImageUrlEmptyOrNull(string url)
        {
            var dto = CreateValidAnimalDTO();
            dto.Images = new List<ReqImageDTO>
            {
                new() { Url = url, isPrincipal = true }
            };

            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("not-a-url")]
        [InlineData("just text")]
        [InlineData("www.example.com")] // missing protocol
        [InlineData("ftp://example.com/image.jpg")] // wrong protocol
        public void ImageUrlInvalidFormat(string url)
        {
            var dto = CreateValidAnimalDTO();
            dto.Images = new List<ReqImageDTO>
            {
                new() { Url = url, isPrincipal = true }
            };

            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("https://example.com/image.jpg")]
        [InlineData("http://example.com/image.jpg")]
        [InlineData("https://cdn.example.com/path/to/image.png")]
        [InlineData("https://s3.amazonaws.com/bucket/image.jpg")]
        public void ImageUrlValidFormat(string url)
        {
            var dto = CreateValidAnimalDTO();
            dto.Images = new List<ReqImageDTO>
            {
                new() { Url = url, isPrincipal = true }
            };

            var result = _validator.Validate(dto);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ImageUrlTooLong()
        {
            var dto = CreateValidAnimalDTO();
            var longUrl = "https://example.com/" + new string('a', 500); // > 500 chars

            dto.Images = new List<ReqImageDTO>
            {
                new() { Url = longUrl, isPrincipal = true }
            };

            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
        }



        private ReqCreateAnimalDto CreateValidAnimalDTO()
        {
            return new ReqCreateAnimalDto
            {
                Name = "Test Animal",
                Species = Species.Dog,
                Size = SizeType.Medium,
                Colour = "Brown",
                BirthDate = new DateOnly(2020, 1, 1),
                Sterilized = true,
                BreedId = Guid.NewGuid().ToString(),
                Cost = 100m,
                Features = "Healthy and friendly",
                Description = "Healthy and friendly",
                Sex = SexType.Male,
                Images = new List<ReqImageDTO>
                {
                    new ReqImageDTO
                    {
                        Url = "https://example.com/valid-image.jpg",
                        isPrincipal = true
                    }
                }
            };
        }

    }
}