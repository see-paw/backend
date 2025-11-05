using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.TestHelper;

namespace Tests.AuthTests.Validators
{
    //codacy: ignore[complexity]
    public class RegisterUserValidatorTests
    {
        private readonly ReqRegisterUserValidator _validator = new();

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
            var dto = new ReqRegisterUserDto
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

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }

}

