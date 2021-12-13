using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExpensesService.Controllers;
using ExpensesService.Models;
using ExpensesService.Registries;
using ExpensesService.Utility;
using ExpensesServiceTests.Utility;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace ExpensesServiceTests.Unit.Controllers
{
    [TestFixture]
    internal class ExpensesControllerTests
    {
        #region Fixture

        private IQueryParametersValidator _validator;
        private IExpensesRegistry _registry;
        private ILoggerFactory _loggerFactory;
        private ExpensesController _controller;

        #endregion

        #region Setup & Teardown

        [SetUp]
        public void SetUp()
        {
            _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole()
                .SetMinimumLevel(LogLevel.Trace));
            _validator = A.Fake<IQueryParametersValidator>();
            _registry = A.Fake<IExpensesRegistry>();

            _controller = new ExpensesController(_loggerFactory, _registry, _validator);
        }

        #endregion

        #region Constructor

        [Test]
        public void IfRegistryIsNull_ExpensesController_ShouldFail()
        {
            this.Invoking(_ => new ExpensesController(_loggerFactory, null, _validator))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void IfValidatorIsNull_ExpensesController_ShouldFail()
        {
            this.Invoking(_ => new ExpensesController(_loggerFactory, _registry, null))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void IfLoggerFactoryIsNUll_ExpensesController_ShouldFail()
        {
            this.Invoking(_ => new ExpensesController(null, _registry, _validator))
                .Should().Throw<ArgumentNullException>();
        }

        #endregion

        #region GetAllAsync

        [Test]
        public async Task IfValidatorThrowsFormatException_GetAllAsync_ShouldFail()
        {
            var cancellationToken = new CancellationToken();
            var queryParams = AGetAllQueryParameters();
            A.CallTo(() => _validator.Validate(queryParams)).Throws<FormatException>();

            var result = await _controller.GetAllAsync(queryParams, cancellationToken);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task IfValidatorThrows_GetAllAsync_ShouldFail()
        {
            var cancellationToken = new CancellationToken();
            var queryParams = AGetAllQueryParameters();
            A.CallTo(() => _validator.Validate(queryParams)).Throws<Exception>();

            var result = await _controller.GetAllAsync(queryParams, cancellationToken);

            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Test]
        public async Task IfRegistryThrows_GetAllAsync_ShouldFail()
        {
            var cancellationToken = new CancellationToken();
            var queryParams = AGetAllQueryParameters();
            var filterParameters = AFilterParameters();
            A.CallTo(() => _validator.Validate(queryParams)).Returns(filterParameters);
            A.CallTo(() => _registry.GetAllAsync(filterParameters, cancellationToken)).Throws<Exception>();

            var result = await _controller.GetAllAsync(queryParams, cancellationToken);

            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            A.CallTo(() => _validator.Validate(queryParams)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task IfRegistryHasBeenCanceled_GetAllAsync_ShouldFail()
        {
            var cancellationToken = new CancellationToken();
            var queryParams = AGetAllQueryParameters();
            var filterParameters = AFilterParameters();
            A.CallTo(() => _validator.Validate(queryParams)).Returns(filterParameters);
            A.CallTo(() => _registry.GetAllAsync(filterParameters, cancellationToken)).Throws<OperationCanceledException>();

            var result = await _controller.GetAllAsync(queryParams, cancellationToken);

            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(499);
            A.CallTo(() => _validator.Validate(queryParams)).MustHaveHappenedOnceExactly();
        }

        [Test]
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public async Task GetAllAsync_ShouldReturnExpenses()
        {
            var cancellationToken = new CancellationToken();
            var expenses = new List<Expense>
            {
                ARandomExpense(),
                ARandomExpense(),
                ARandomExpense()
            };
            var queryParams = AGetAllQueryParameters();
            var filterParameters = AFilterParameters();
            A.CallTo(() => _validator.Validate(queryParams)).Returns(filterParameters);
            A.CallTo(() => _registry.GetAllAsync(filterParameters, cancellationToken)).Returns(expenses);

            var result = await _controller.GetAllAsync(queryParams, cancellationToken);

            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<IEnumerable<Expense>>()
                .Which.Should().HaveCount(3)
                .And.Subject.ToList().Should().Match(args =>
                    args.ToList()[0] == expenses.ToList()[0] &&
                    args.ToList()[1] == expenses.ToList()[1] &&
                    args.ToList()[2] == expenses.ToList()[2]
                );
        }

        #endregion

        #region GetAsync

        [Test]
        public async Task IfRegistryThrows_GetAsync_ShouldFail()
        {
            var cancellationToken = new CancellationToken();
            var id = Guid.NewGuid();
            A.CallTo(() => _registry.GetAsync(id, cancellationToken)).Throws<Exception>();

            var result = await _controller.GetAsync(id, cancellationToken);

            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Test]
        public async Task IfRegistryHasBeenCancelled_GetAsync_ShouldFail()
        {
            var cancellationToken = new CancellationToken();
            var id = Guid.NewGuid();
            A.CallTo(() => _registry.GetAsync(id, cancellationToken)).Throws<OperationCanceledException>();

            var result = await _controller.GetAsync(id, cancellationToken);

            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(499);
        }

        [Test]
        public async Task IfRegistryDoesNotFoundMatchingItem_GetAsync_ShouldFail()
        {
            var cancellationToken = new CancellationToken();
            var id = Guid.NewGuid();
            A.CallTo(() => _registry.GetAsync(id, cancellationToken)).Returns((Expense)null);

            var result = await _controller.GetAsync(id, cancellationToken);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Test]
        public async Task GetAsync_ShouldReturnMatchingItem()
        {
            var cancellationToken = new CancellationToken();
            var id = Guid.NewGuid();
            var expense = ARandomExpense();
            A.CallTo(() => _registry.GetAsync(id, cancellationToken)).Returns(expense);

            var result = await _controller.GetAsync(id, cancellationToken);

            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<Expense>()
                .Which.Should().Be(expense);
        }

        #endregion

        #region CreateAsync

        [Test]
        public async Task IfRegistryThrows_CreateAsync_ShouldFail()
        {
            var cancellationToken = new CancellationToken();
            var expenseDetails = ARandomExpenseDetails();
            A.CallTo(() => _registry.InsertAsync(expenseDetails, cancellationToken)).Throws<Exception>();

            var result = await _controller.CreateAsync(expenseDetails, cancellationToken);

            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Test]
        public async Task IfRegistryThrowsArgumentException_CreateAsync_ShouldFail()
        {
            var cancellationToken = new CancellationToken();
            var expenseDetails = ARandomExpenseDetails();
            A.CallTo(() => _registry.InsertAsync(expenseDetails, cancellationToken)).Throws<ArgumentException>();

            var result = await _controller.CreateAsync(expenseDetails, cancellationToken);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task IfRegistryHasBeenCancelled_CreateAsync_ShouldFail()
        {
            var cancellationToken = new CancellationToken();
            var expenseDetails = ARandomExpenseDetails();
            A.CallTo(() => _registry.InsertAsync(expenseDetails, cancellationToken)).Throws<OperationCanceledException>();

            var result = await _controller.CreateAsync(expenseDetails, cancellationToken);

            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(499);
        }

        [Test]
        public async Task CreateAsync_ShouldCreateExpense()
        {
            var cancellationToken = new CancellationToken();
            var expenseDetails = ARandomExpenseDetails();
            var createdExpense = ARandomExpense();
            A.CallTo(() => _registry.InsertAsync(expenseDetails, cancellationToken)).Returns(createdExpense);

            var result = await _controller.CreateAsync(expenseDetails, cancellationToken);

            result.Should().BeOfType<CreatedAtRouteResult>()
                .Which.Value.Should().BeAssignableTo<Expense>()
                .Which.Should().Be(createdExpense);
        }

        #endregion

        #region UpdateAsync

        [Test]
        public async Task IfRegistryThrows_UpdateAsync_ShouldFail()
        {
            var cancellationToken = new CancellationToken();
            var id = Guid.NewGuid();
            var expenseDetails = ARandomExpenseDetails();
            A.CallTo(() => _registry.UpdateAsync(id, expenseDetails, cancellationToken)).Throws<Exception>();

            var result = await _controller.UpdateAsync(id, expenseDetails, cancellationToken);

            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Test]
        public async Task IfRegistryThrowsArgumentException_UpdateAsync_ShouldFail()
        {
            var id = Guid.NewGuid();
            var cancellationToken = new CancellationToken();
            var expenseDetails = ARandomExpenseDetails();
            A.CallTo(() => _registry.UpdateAsync(id, expenseDetails, cancellationToken)).Throws<ArgumentException>();

            var result = await _controller.UpdateAsync(id, expenseDetails, cancellationToken);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task IfRegistryHasBeenCancelled_UpdateAsync_ShouldFail()
        {
            var cancellationToken = new CancellationToken();
            var id = Guid.NewGuid();
            var expenseDetails = ARandomExpenseDetails();
            A.CallTo(() => _registry.UpdateAsync(id, expenseDetails, cancellationToken)).Throws<OperationCanceledException>();

            var result = await _controller.UpdateAsync(id, expenseDetails, cancellationToken);

            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(499);
        }

        [Test]
        public async Task IfRegistryDoesNotFoundMatchingItem_UpdateAsync_ShouldFail()
        {
            var cancellationToken = new CancellationToken();
            var id = Guid.NewGuid();
            var expenseDetails = ARandomExpenseDetails();
            A.CallTo(() => _registry.UpdateAsync(id, expenseDetails, cancellationToken)).Throws<NotFoundException>();

            var result = await _controller.UpdateAsync(id, expenseDetails, cancellationToken);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateExpense()
        {
            var cancellationToken = new CancellationToken();
            var id = Guid.NewGuid();
            var expenseDetails = ARandomExpenseDetails();
            var updatedExpense = ARandomExpense();
            A.CallTo(() => _registry.UpdateAsync(id, expenseDetails, cancellationToken)).Returns(updatedExpense);

            var result = await _controller.UpdateAsync(id, expenseDetails, cancellationToken);

            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<Expense>()
                .Which.Should().Be(updatedExpense);
        }

        #endregion

        #region DeleteAsync

        [Test]
        public async Task IfRegistryThrows_DeleteAsync_ShouldFail()
        {
            var cancellationToken = new CancellationToken();
            var id = Guid.NewGuid();
            A.CallTo(() => _registry.DeleteAsync(id, cancellationToken)).Throws<Exception>();

            var result = await _controller.DeleteAsync(id, cancellationToken);

            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Test]
        public async Task IfRegistryHasBeenCancelled_DeleteAsync_ShouldFail()
        {
            var cancellationToken = new CancellationToken();
            var id = Guid.NewGuid();
            A.CallTo(() => _registry.DeleteAsync(id, cancellationToken)).Throws<OperationCanceledException>();

            var result = await _controller.DeleteAsync(id, cancellationToken);

            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(499);
        }

        [Test]
        public async Task DeleteAsync_ShouldUpdateExpense()
        {
            var cancellationToken = new CancellationToken();
            var id = Guid.NewGuid();
            A.CallTo(() => _registry.DeleteAsync(id, cancellationToken)).Returns(Task.CompletedTask);

            var result = await _controller.DeleteAsync(id, cancellationToken);

            result.Should().BeOfType<NoContentResult>();
        }

        #endregion

        #region Utility Methods

        private static Expense ARandomExpense()
        {
            return new Expense
            {
                Id = Guid.NewGuid(),
                ExpenseDetails = ARandomExpenseDetails()
            };
        }

        private static ExpenseDetails ARandomExpenseDetails()
        {
            return new ExpenseDetails
            {
                Value = RandomData.Double.Positive(),
                Date = RandomData.DateTimeOffset.Past(),
                Reason = RandomData.String.Alphanumeric(),
                PaymentMethod = RandomData.Enum.Any<PaymentMethod>()
            };
        }

        private static GetAllQueryParameters AGetAllQueryParameters()
        {
            return new GetAllQueryParameters
            {
                From = RandomData.String.Alphanumeric(),
                In = RandomData.String.Alphanumeric(),
                To = RandomData.String.Alphanumeric(),
                PaymentMethod = RandomData.Enum.Any<PaymentMethod>()
            };
        }

        private static FilterParameters AFilterParameters()
        {
            return new FilterParameters
            {
                From = RandomData.String.Alphanumeric(),
                In = RandomData.String.Alphanumeric(),
                To = RandomData.String.Alphanumeric(),
                PaymentMethod = RandomData.Enum.Any<PaymentMethod>()
            };
        }

        #endregion
    }
}
