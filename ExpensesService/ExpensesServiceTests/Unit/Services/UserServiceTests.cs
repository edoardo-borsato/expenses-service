using System;
using ExpensesService.Services;
using ExpensesServiceTests.Utility;
using FluentAssertions;
using NUnit.Framework;

namespace ExpensesServiceTests.Unit.Services
{
    [TestFixture]
    internal class UserServiceTests
    {
        #region Fixture

        private IUserService _userService;

        #endregion

        #region Setup & Teardown

        [SetUp]
        public void SetUp()
        {
            _userService = new UserService("username", "password");
        }

        #endregion

        #region Constructor

        [Test]
        public void IfUserNameIsNull_UserService_ShouldFail()
        {
            this.Invoking(_ => new UserService(null, RandomData.String.Alphanumeric()))
                .Should().Throw<ArgumentException>();
        }

        [Test]
        public void IfPasswordIsNull_UserService_ShouldFail()
        {
            this.Invoking(_ => new UserService(RandomData.String.Alphanumeric(), null))
                .Should().Throw<ArgumentException>();
        }

        #endregion

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void IfUsernameIsNullOrWhiteSpace_Validate_ShouldFail(string username)
        {
            _userService.Invoking(s => s.Validate(username, RandomData.String.Alphanumeric()))
                .Should().Throw<ArgumentException>();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void IfPasswordIsNullOrWhiteSpace_Validate_ShouldFail(string password)
        {
            _userService.Invoking(s => s.Validate(RandomData.String.Alphanumeric(), password))
                .Should().Throw<ArgumentException>();
        }

        [TestCase("auihdog", "password")]
        [TestCase("username", "uyahrg")]
        [TestCase("wrthrt", "uyahrg")]
        public void IfCredentialsAreInvalid_Validate_ShouldReturnFalse(string username, string password)
        {
            var result = _userService.Validate(username, password);

            result.Should().BeFalse();
        }

        [Test]
        public void IfCredentialsAreValid_Validate_ShouldReturnTrue()
        {
            var result = _userService.Validate("username", "password");

            result.Should().BeTrue();
        }
    }
}
