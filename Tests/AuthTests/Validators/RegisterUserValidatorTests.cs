using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.TestHelper;
using WebAPI.DTOs.Auth;
using WebAPI.Validators.Auth;

namespace Tests.AuthTests.Validators
{
    //codacy: ignore[complexity]

    /// <summary>
    /// Unit tests for <see cref="ReqRegisterUserValidator"/>.
    /// </summary>
    public class RegisterUserValidatorTests
    {
        /// <summary>
        /// Validator under test.
        /// </summary>
        private readonly RegisterUserValidator _validator = new();

        /// <summary>
        /// Creates a valid DTO for a standard user.
        /// </summary>
        private static ReqRegisterUserDto CreateValidUser(string role = "User")
        {
            return new ReqRegisterUserDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "Aa!123456",
                Street = "Rua de Teste",
                City = "Porto",
                PostalCode = "4000-123",
                BirthDate = new DateTime(1990, 1, 1),
                SelectedRole = role
            };
        }


        /// <summary>
        /// Creates a valid DTO for an AdminCAA user including required shelter fields.
        /// </summary>
        private static ReqRegisterUserDto CreateValidAdminCAA()
        {
            return new ReqRegisterUserDto
            {
                Name = "Admin",
                Email = "admin@example.com",
                Password = "Aa!123456",
                Street = "Rua de Teste",
                City = "Porto",
                PostalCode = "4000-123",
                BirthDate = new DateTime(1990, 1, 1),
                SelectedRole = "AdminCAA",
                ShelterName = "Abrigo da Serra",
                ShelterStreet = "Rua Verde",
                ShelterCity = "Porto",
                ShelterPostalCode = "4000-200",
                ShelterPhone = "912345678",
                ShelterNIF = "123456789",
                ShelterOpeningTime = "09:00",
                ShelterClosingTime = "18:00"
            };
        }

        [Fact]
        public void Should_Pass_When_Normal_User()
        {
            var dto = CreateValidUser("User");

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_AdminCAA_Without_Shelter_Fields()
        {
            var dto = CreateValidUser("AdminCAA");

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ShelterName);
            result.ShouldHaveValidationErrorFor(x => x.ShelterStreet);
            result.ShouldHaveValidationErrorFor(x => x.ShelterCity);
            result.ShouldHaveValidationErrorFor(x => x.ShelterPostalCode);
            result.ShouldHaveValidationErrorFor(x => x.ShelterPhone);
            result.ShouldHaveValidationErrorFor(x => x.ShelterNIF);
            result.ShouldHaveValidationErrorFor(x => x.ShelterOpeningTime);
            result.ShouldHaveValidationErrorFor(x => x.ShelterClosingTime);
        }

        [Fact]
        public void Should_Pass_When_AdminCAA_With_All_Shelter_Fields()
        {
            var dto = CreateValidAdminCAA();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }


        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("A")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\r\n")]
        public void Should_Fail_When_Normal_User_Has_Invalid_Name(String name)
        {
            var dto = CreateValidUser("User");
            dto.Name = name;

            var result = _validator.TestValidate(dto);

            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("Zé")]
        [InlineData("Maria")]
        [InlineData("Ana Paula Fonseca Lopes Dias")]
        public void Should_Success_When_Normal_User_Has_Valid_Name(String name)
        {
            var dto = CreateValidUser("User");
            dto.Name = name;

            var result = _validator.TestValidate(dto);

            Assert.True(result.IsValid);
        }


        [Fact]
        public void Should_Fail_When_Normal_User_Has_Future_BirthDate()
        {
            var dto = CreateValidUser("User");
            dto.BirthDate = DateTime.UtcNow.AddDays(1);

            var result = _validator.TestValidate(dto);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_Normal_User_Has_BirthDate_Older_Than_100_Years()
        {
            var dto = CreateValidUser("User");
            dto.BirthDate = DateTime.UtcNow.AddYears(-101);

            var result = _validator.TestValidate(dto);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void Should_Success_When_Normal_User_Has_Past_BirthDate()
        {
            var dto = CreateValidUser("User");
            dto.BirthDate = new DateTime(1999, 5, 15);

            var result = _validator.TestValidate(dto);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\r\n")]
        public void Should_Fail_When_Normal_User_Has_Invalid_Street(String street)
        {
            var dto = CreateValidUser("User");
            dto.Street = street;

            var result = _validator.TestValidate(dto);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void Should_Success_When_Normal_User_Has_Valid_Street()
        {
            var dto = CreateValidUser("User");
            dto.Street = "Rua de Cima";

            var result = _validator.TestValidate(dto);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\r\n")]
        public void Should_Fail_When_Normal_User_Has_Invalid_City(String city)
        {
            var dto = CreateValidUser("User");
            dto.City = city;

            var result = _validator.TestValidate(dto);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void Should_Success_When_Normal_User_Has_Valid_City()
        {
            var dto = CreateValidUser("User");
            dto.City = "Porto";

            var result = _validator.TestValidate(dto);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("12-25")]
        public void Should_Fail_When_Normal_User_Has_Invalid_PostalCode(String postalCode)
        {
            var dto = CreateValidUser("User");
            dto.PostalCode = postalCode;

            var result = _validator.TestValidate(dto);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void Should_Success_When_Normal_User_Has_Valid_PostalCode()
        {
            var dto = CreateValidUser("User");
            dto.PostalCode = "1425-263";

            var result = _validator.TestValidate(dto);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("abc")]
        [InlineData("user@")]
        [InlineData("@domain.com")]
        public void Should_Fail_When_Normal_User_Has_Invalid_Email(string email)
        {
            var dto = CreateValidUser("User");
            dto.Email = email;

            var result = _validator.TestValidate(dto);

            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("user@test.com")]
        [InlineData("person123@domain.pt")]
        [InlineData("my.email+tag@gmail.com")]
        public void Should_Success_When_Normal_User_Has_Valid_Email(string email)
        {
            var dto = CreateValidUser("User");
            dto.Email = email;

            var result = _validator.TestValidate(dto);

            Assert.True(result.IsValid);
        }



        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("aswed")]//too short
        [InlineData("asmin1!")]//without uppercase letters
        [InlineData("ASDMI1!")]//withou lowercase letters
        [InlineData("admin!")]//without numbers
        [InlineData("Aeisw5")]//without special characters

        public void Should_Fail_When_Normal_User_Has_Invalid_Password(String password)
        {
            var dto = CreateValidUser("User");
            dto.Password = password;

            var result = _validator.TestValidate(dto);

            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("aswedA2!")]
        [InlineData("asmin1!A")]
        [InlineData("ASDMI1!a")]
        [InlineData("Admin!10")]
        [InlineData("Aeisw5!!")]

        public void Should_Fail_When_Normal_User_Has_Valid_Password(String password)
        {
            var dto = CreateValidUser("User");
            dto.Password = password;

            var result = _validator.TestValidate(dto);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_Invalid_Role()
        {
            var dto = CreateValidUser("InvalidRole");

            var result = _validator.TestValidate(dto);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void Should_Success_With_Valid_User_Role()

        {
            var dto = CreateValidUser();

            var result = _validator.TestValidate(dto);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Success_With_Valid_AdminCAA_Role()

        {
            var dto = CreateValidAdminCAA();

            var result = _validator.TestValidate(dto);

            Assert.True(result.IsValid);
        }



        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("A")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\r\n")]
        public void Should_Fail_When_AdminCAA_Has_Invalid_ShelterName(String shelterName)
        {
            var dto = CreateValidAdminCAA();
            dto.ShelterName = shelterName;

            var result = _validator.TestValidate(dto);

            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("Abrigo 1")]
        [InlineData("Tarecos Amigos")]
        public void Should_Success_When_AdminCAA_Has_Valid_ShelterName(String shelterName)
        {
            var dto = CreateValidAdminCAA();
            dto.ShelterName = shelterName;

            var result = _validator.TestValidate(dto);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\r\n")]
        public void Should_Fail_When_AdminCAA_Has_Invalid_ShelterStreet(String shelterStreet)
        {
            var dto = CreateValidAdminCAA(); 
            dto.ShelterStreet = shelterStreet;

            var result = _validator.TestValidate(dto);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void Should_Success_AdminCAA_Has_Valid_ShelterStreet()
        {
            var dto = CreateValidAdminCAA();
            dto.ShelterStreet = "Rua de Cima";

            var result = _validator.TestValidate(dto);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\r\n")]
        public void Should_Fail_When_AdminCAA_Has_Invalid_ShelterCity(String shelterCity)
        {
            var dto = CreateValidAdminCAA();
            dto.ShelterCity = shelterCity;

            var result = _validator.TestValidate(dto);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void Should_Success_When_AdminCAA_Has_Valid_ShelterCity()
        {
            var dto = CreateValidAdminCAA();
            dto.ShelterCity = "Porto";

            var result = _validator.TestValidate(dto);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("12-25")]
        public void Should_Fail_When_AdminCAA_Has_Invalid_ShelterPostalCode(String ShelterPostalCode)
        {
            var dto = CreateValidAdminCAA();
            dto.ShelterPostalCode = ShelterPostalCode;

            var result = _validator.TestValidate(dto);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void Should_Success_When_AdminCAA_Has_Valid_ShelterPostalCode()
        {
            var dto = CreateValidAdminCAA();
            dto.ShelterPostalCode = "1425-263";

            var result = _validator.TestValidate(dto);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("111111111")] //starts with 1
        [InlineData("9154263")] //less than 9 digits
        public void Should_Fail_When_AdminCAA_Has_Invalid_PhoneNumber(String phoneMumber)
        {
            var dto = CreateValidAdminCAA();
            dto.ShelterPhone = phoneMumber;

            var result = _validator.TestValidate(dto);

            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("211111111")] 
        [InlineData("915426359")] 
        public void Should_Success_When_AdminCAA_Has_Valid_PhoneNumber(String phoneMumber)
        {
            var dto = CreateValidAdminCAA();
            dto.ShelterPhone = phoneMumber;

            var result = _validator.TestValidate(dto);

            Assert.True(result.IsValid);
        }


        [Theory]
        [InlineData("4666")] //less then 9 digits
        [InlineData("")]
        [InlineData(null)]
        public void Should_Fail_When_AdminCAA_Has_InvalidNIF(String nif)
        {
            var dto = CreateValidAdminCAA();
            dto.ShelterNIF = nif;

            var result = _validator.TestValidate(dto);

            Assert.False(result.IsValid);

        }

        [Fact]

        public void Should_Success_When_AdminCAA_Has_ValidNIF()
        {
            var dto = CreateValidAdminCAA();
            dto.ShelterNIF = "954156328";

            var result = _validator.TestValidate(dto);

            Assert.True(result.IsValid);

        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Fail_When_AdminCAA_Has_Invalid_ShelterOpeningTime(string openingTime)
        {
            var dto = CreateValidAdminCAA();
            dto.ShelterOpeningTime = openingTime;

            var result = _validator.TestValidate(dto);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void Should_Success_When_AdminCAA_Has_Valid_ShelterOpeningTime()
        {
            var dto = CreateValidAdminCAA();
            dto.ShelterOpeningTime = "08:30";

            var result = _validator.TestValidate(dto);

            Assert.True(result.IsValid);
        }



        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Fail_When_AdminCAA_Has_Invalid_ShelterClosingTime(string closingTime)
        {
            var dto = CreateValidAdminCAA();
            dto.ShelterClosingTime = closingTime;

            var result = _validator.TestValidate(dto);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void Should_Success_When_AdminCAA_Has_Valid_ShelterClosingTime()
        {
            var dto = CreateValidAdminCAA();
            dto.ShelterClosingTime = "18:00";

            var result = _validator.TestValidate(dto);

            Assert.True(result.IsValid);
        }


    }

}

