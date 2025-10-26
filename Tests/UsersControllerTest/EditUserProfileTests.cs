using WebAPI.DTOs;
using WebAPI.Validators;

namespace Tests.UserControllerTest
{
    //codacy: ignore[complexity]
    public class EditUserProfileTests
    {
        private readonly UserProfileValidator _validator;

        public EditUserProfileTests()
        {
            _validator = new UserProfileValidator();
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("12345")]
        [InlineData("@na")]
        [InlineData("ThisIsAnExtremelyLongNameThatExceedsTheMaximumAllowedCharacterLimitBecauseItHasMoreThanTwoHundredAndFiftyFiveCharacters_"
                   + "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
        public void InvalidName(string name)
        {
            var dto = CreateValidUserDto();
            dto.Name = name;

            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("Diana Silva")]
        [InlineData("Ana-Maria")]
        [InlineData("João D’Água")]
        [InlineData("Jean-Pierre")]
        [InlineData("José António")]
        public void ValidName(string name)
        {
            var dto = CreateValidUserDto();
            dto.Name = name;

            var result = _validator.Validate(dto);
            Assert.True(result.IsValid);
        }


        [Fact]
        public void BirthDateInFutureIsInvalid()
        {
            var dto = CreateValidUserDto();
            dto.BirthDate = DateTime.UtcNow.AddDays(2);

            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
        }

        [Fact]
        public void BirthDateTooOldIsInvalid()
        {
            var dto = CreateValidUserDto();
            dto.BirthDate = DateTime.UtcNow.AddYears(-120);

            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
        }

        [Fact]
        public void ValidBirthDate()
        {
            var dto = CreateValidUserDto();
            dto.BirthDate = new DateTime(1995, 6, 10);

            var result = _validator.Validate(dto);
            Assert.True(result.IsValid);
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void InvalidStreet(string street)
        {
            var dto = CreateValidUserDto();
            dto.Street = street;

            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("Rua das Oliveiras 10")]
        [InlineData("Av. da Liberdade 200")]
        public void ValidStreet(string street)
        {
            var dto = CreateValidUserDto();
            dto.Street = street;

            var result = _validator.Validate(dto);
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void InvalidCity(string city)
        {
            var dto = CreateValidUserDto();
            dto.City = city;

            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("Lisboa")]
        [InlineData("Porto")]
        [InlineData("Coimbra")]
        public void ValidCity(string city)
        {
            var dto = CreateValidUserDto();
            dto.City = city;

            var result = _validator.Validate(dto);
            Assert.True(result.IsValid);
        }


        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("123")]
        [InlineData("12-")]
        [InlineData("12345678901")]
        [InlineData("80@0-333")]
        public void InvalidPostalCode(string code)
        {
            var dto = CreateValidUserDto();
            dto.PostalCode = code;

            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("8000-333")]
        public void ValidPostalCode(string code)
        {
            var dto = CreateValidUserDto();
            dto.PostalCode = code;

            var result = _validator.Validate(dto);
            Assert.True(result.IsValid);
        }


        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("123456789")] // doesn’t start with 9
        [InlineData("91234567")] // 8 digits
        [InlineData("9123456789")] // 10 digits
        [InlineData("91234A678")] // contains letter
        public void InvalidPhoneNumber(string phone)
        {
            var dto = CreateValidUserDto();
            dto.PhoneNumber = phone;

            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("912345678")]
        [InlineData("934567890")]
        [InlineData("999999999")]
        public void ValidPhoneNumber(string phone)
        {
            var dto = CreateValidUserDto();
            dto.PhoneNumber = phone;

            var result = _validator.Validate(dto);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidUserProfile_AllFieldsPass()
        {
            var dto = CreateValidUserDto();
            var result = _validator.Validate(dto);
            Assert.True(result.IsValid);
        }

        // Helper method to reduce duplication
        private ReqUserProfileDto CreateValidUserDto()
        {
            return new ReqUserProfileDto
            {
                Name = "Diana Silva",
                BirthDate = new DateTime(1990, 9, 30),
                Street = "Rua das Oliveiras 10",
                City = "Faro",
                PostalCode = "8000-333",
                PhoneNumber = "912345678"
            };
        }
    }
}
