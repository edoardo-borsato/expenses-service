using System;
using ExpensesService.Controllers;
using ExpensesService.Models;
using ExpensesServiceTests.Utility;
using FluentAssertions;
using NUnit.Framework;

namespace ExpensesServiceTests.Unit.Controllers
{
    [TestFixture]
    internal class QueryParametersValidatorTests
    {
        #region Fixture

        private IQueryParametersValidator _validator;

        #endregion

        #region Setup & Teardown

        [SetUp]
        public void SetUp()
        {
            _validator = new QueryParametersValidator();
        }

        #endregion

        [Test]
        public void IfQueryParamsIsNullObject_Validate_ShouldReturnNull()
        {
            var filterParameters = _validator.Validate(null);

            filterParameters.Should().BeNull();
        }

        [Test]
        public void IfQueryParamsHasNullProperties_Validate_ShouldReturnFilterParamsWithNullProperties()
        {
            var filterParameters = _validator.Validate(new GetAllQueryParameters());

            filterParameters.From.Should().BeNull();
            filterParameters.To.Should().BeNull();
            filterParameters.In.Should().BeNull();
            filterParameters.PaymentMethod.Should().BeNull();
        }

        [TestCase("adrgjiorg")]
        [TestCase("2000/12/31")]
        [TestCase("12/31/2000")]
        [TestCase("31/12/2000")]
        [TestCase("31-12-2000")]
        [TestCase("12-31-2000")]
        public void IfFromIsInvalid_Validate_ShouldFail(string date)
        {
            _validator.Invoking(v => v.Validate(new GetAllQueryParameters { From = date }))
                .Should().ThrowExactly<FormatException>();

        }

        [TestCase("2000")]
        [TestCase("2000-12")]
        [TestCase("2000-12-31")]
        public void IfFromIsValid_Validate_ShouldReturnFilterParameters(string date)
        {
            var queryParameters = new GetAllQueryParameters { From = date };
            var filterParameters = _validator.Validate(queryParameters);

            filterParameters.From.Should().Be(queryParameters.From);
            filterParameters.In.Should().BeNull();
            filterParameters.To.Should().BeNull();
            filterParameters.PaymentMethod.Should().BeNull();
        }

        [TestCase("adrgjiorg")]
        [TestCase("2000/12/31")]
        [TestCase("12/31/2000")]
        [TestCase("31/12/2000")]
        [TestCase("31-12-2000")]
        [TestCase("12-31-2000")]
        public void IfToIsInvalid_Validate_ShouldFail(string date)
        {
            _validator.Invoking(v => v.Validate(new GetAllQueryParameters { To = date }))
                .Should().ThrowExactly<FormatException>();

        }

        [TestCase("2000")]
        [TestCase("2000-12")]
        [TestCase("2000-12-31")]
        public void IfToIsValid_Validate_ShouldReturnFilterParameters(string date)
        {
            var queryParameters = new GetAllQueryParameters { To = date };
            var filterParameters = _validator.Validate(queryParameters);

            filterParameters.To.Should().Be(queryParameters.To);
            filterParameters.In.Should().BeNull();
            filterParameters.From.Should().BeNull();
            filterParameters.PaymentMethod.Should().BeNull();
        }

        [TestCase("adrgjiorg")]
        [TestCase("2000/12/31")]
        [TestCase("12/31/2000")]
        [TestCase("31/12/2000")]
        [TestCase("31-12-2000")]
        [TestCase("12-31-2000")]
        public void IfInIsInvalid_Validate_ShouldFail(string date)
        {
            _validator.Invoking(v => v.Validate(new GetAllQueryParameters { In = date }))
                .Should().ThrowExactly<FormatException>();

        }

        [TestCase("2000")]
        [TestCase("2000-12")]
        [TestCase("2000-12-31")]
        public void IfInIsValid_Validate_ShouldReturnFilterParameters(string date)
        {
            var queryParameters = new GetAllQueryParameters { In = date };
            var filterParameters = _validator.Validate(queryParameters);

            filterParameters.In.Should().Be(queryParameters.In);
            filterParameters.To.Should().BeNull();
            filterParameters.From.Should().BeNull();
            filterParameters.PaymentMethod.Should().BeNull();
        }

        [Test]
        public void IfPaymentMethodIsPresent_Validate_ShouldReturnFilterParameters()
        {
            var paymentMethod = RandomData.Enum.Any<PaymentMethod>();
            var queryParameters = new GetAllQueryParameters { PaymentMethod = paymentMethod };

            var filterParameters = _validator.Validate(queryParameters);

            filterParameters.In.Should().BeNull();
            filterParameters.To.Should().BeNull();
            filterParameters.From.Should().BeNull();
            filterParameters.PaymentMethod.Should().Be(paymentMethod);
        }
    }
}