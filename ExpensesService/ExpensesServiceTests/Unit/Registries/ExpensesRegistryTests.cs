using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExpensesService.Models;
using ExpensesService.Registries;
using ExpensesService.Repositories;
using ExpensesService.Utility;
using ExpensesServiceTests.Utility;
using FakeItEasy;
using FakeItEasy.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace ExpensesServiceTests.Unit.Registries
{
    [TestFixture]
    internal class ExpensesRegistryTests
    {
        #region Fixture

        private ILoggerFactory _loggerFactory;
        private IExpensesRepository _repository;
        private IFilterFactory _filterFactory;
        private IWatch _watch;

        private IExpensesRegistry _registry;

        #endregion

        #region Setup & Teardown

        [SetUp]
        public void SetUp()
        {
            _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole()
                .SetMinimumLevel(LogLevel.Trace));
            _repository = A.Fake<IExpensesRepository>();
            _filterFactory = A.Fake<IFilterFactory>();
            _watch = A.Fake<IWatch>();

            _registry = new ExpensesRegistry(_loggerFactory, _repository, _filterFactory, _watch);
        }

        #endregion

        #region Constructor

        [Test]
        public void IfLoggerFactoryIsNull_ExpensesRegistry_ShouldFail()
        {
            this.Invoking(_ => new ExpensesRegistry(null, _repository, _filterFactory, _watch))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void IfRepositoryIsNull_ExpensesRegistry_ShouldFail()
        {
            this.Invoking(_ => new ExpensesRegistry(_loggerFactory, null, _filterFactory, _watch))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void IfFilterFactoryIsNull_ExpensesRegistry_ShouldFail()
        {
            this.Invoking(_ => new ExpensesRegistry(_loggerFactory, _repository, null, _watch))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void IfWatchIsNull_ExpensesRegistry_ShouldFail()
        {
            this.Invoking(_ => new ExpensesRegistry(_loggerFactory, _repository, _filterFactory, null))
                .Should().Throw<ArgumentNullException>();
        }

        #endregion

        #region GetAllAsync

        [Test]
        public async Task IfFilterFactoryThrows_GetAllAsync_ShouldFail()
        {
            var exception = new Exception();
            var cancellationToken = new CancellationToken();
            var filterParameters = AFilterParameters();
            A.CallTo(() => _filterFactory.Create(filterParameters)).Throws(exception);

            (await _registry.Awaiting(r => r.GetAllAsync(filterParameters, cancellationToken))
                    .Should().ThrowExactlyAsync<Exception>())
                    .And.Should().Be(exception);
        }

        [Test]
        public async Task IfRepositoryThrows_GetAllAsync_ShouldFail()
        {
            var exception = new Exception();
            var cancellationToken = new CancellationToken();
            var filterParameters = AFilterParameters();
            var filter = A.Fake<IFilter>();
            A.CallTo(() => _filterFactory.Create(filterParameters)).Returns(filter);
            A.CallTo(() => _repository.GetAllAsync(filter, cancellationToken)).Throws(exception);

            (await _registry.Awaiting(r => r.GetAllAsync(filterParameters, cancellationToken))
                    .Should().ThrowExactlyAsync<Exception>())
                    .And.Should().Be(exception);
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnExpenses()
        {
            var cancellationToken = new CancellationToken();
            var expenses = new List<Expense>
            {
                AnExpense(),
                AnExpense(),
                AnExpense()
            };
            var filterParameters = AFilterParameters();
            var filter = A.Fake<IFilter>();
            A.CallTo(() => _filterFactory.Create(filterParameters)).Returns(filter);
            A.CallTo(() => _repository.GetAllAsync(filter, cancellationToken)).Returns(expenses);

            var result = (await _registry.GetAllAsync(filterParameters, cancellationToken)).ToList();

            result.Should().HaveCount(3);
            result[0].Should().Be(expenses[0]);
            result[1].Should().Be(expenses[1]);
            result[2].Should().Be(expenses[2]);
            A.CallTo(() => _filterFactory.Create(filterParameters)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => _repository.GetAllAsync(filter, cancellationToken)).MustHaveHappenedOnceExactly());
        }

        #endregion

        #region GetAsync

        [Test]
        public async Task IfRepositoryThrows_GetAsync_ShouldFail()
        {
            var exception = new Exception();
            var id = Guid.NewGuid();
            var cancellationToken = new CancellationToken();
            A.CallTo(() => _repository.GetAsync(id, cancellationToken)).Throws(exception);

            (await _registry.Awaiting(r => r.GetAsync(id, cancellationToken))
                    .Should().ThrowExactlyAsync<Exception>())
                    .And.Should().Be(exception);
        }

        [Test]
        public async Task GetAsync_ShouldReturnMatchingExpense()
        {
            var id = Guid.NewGuid();
            var cancellationToken = new CancellationToken();
            var expense = AnExpense();
            A.CallTo(() => _repository.GetAsync(id, cancellationToken)).Returns(expense);

            var result = await _repository.GetAsync(id, cancellationToken);

            result.Should().Be(expense);
            A.CallTo(() => _repository.GetAsync(id, cancellationToken)).MustHaveHappenedOnceExactly();
        }

        #endregion

        #region InsertAsync

        [Test]
        public async Task IfValueIsNegative_InsertAsync_ShouldFail()
        {
            var expenseDetails = new ExpenseDetails
            {
                Value = -1.0,
                Reason = RandomData.String.Alphanumeric(),
                Date = RandomData.DateTimeOffset.Past(),
                PaymentMethod = RandomData.Enum.Any<PaymentMethod>()
            };
            var cancellationToken = new CancellationToken();

            await _registry.Awaiting(r => r.InsertAsync(expenseDetails, cancellationToken))
                .Should().ThrowExactlyAsync<ArgumentException>();
        }

        [Test]
        public async Task IfRepositoryThrows_InsertAsync_ShouldFail()
        {
            var exception = new Exception();
            var expenseDetails = AnExpenseDetails();
            var cancellationToken = new CancellationToken();
            var expense = new Expense { Id = Guid.NewGuid(), ExpenseDetails = expenseDetails };
            ACallToInsertAsync(expense, cancellationToken).Throws(exception);

            (await _registry.Awaiting(r => r.InsertAsync(expenseDetails, cancellationToken))
                    .Should().ThrowExactlyAsync<Exception>())
                .And.Should().Be(exception);
        }

        [Test]
        public async Task IfDateIsNotProvided_InsertAsync_ShouldInsertWithCurrentDate()
        {
            var expenseDetails = new ExpenseDetails
            {
                Value = RandomData.Double.Positive(),
                Reason = RandomData.String.Alphanumeric(),
                PaymentMethod = RandomData.Enum.Any<PaymentMethod>()
            };
            var newGuid = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;
            var expense = new Expense
            {
                Id = newGuid,
                ExpenseDetails = new ExpenseDetails
                {
                    Value = expenseDetails.Value,
                    Reason = expenseDetails.Reason,
                    Date = now,
                    PaymentMethod = expenseDetails.PaymentMethod
                }
            };
            var cancellationToken = new CancellationTokenSource().Token;
            A.CallTo(() => _watch.Now()).Returns(now);
            ACallToInsertAsync(expense, cancellationToken).Returns(Task.CompletedTask);

            var result = await _registry.InsertAsync(expenseDetails, cancellationToken);

            ACallToInsertAsync(expense, cancellationToken).MustHaveHappenedOnceExactly();
            result.ExpenseDetails.Should().Be(expense.ExpenseDetails);
        }

        [Test]
        public async Task IPaymentMethodIsNotProvided_InsertAsync_ShouldInsertWithUndefined()
        {
            var expenseDetails = new ExpenseDetails
            {
                Value = RandomData.Double.Positive(),
                Reason = RandomData.String.Alphanumeric(),
                Date = RandomData.DateTimeOffset.Past()
            };
            var newGuid = Guid.NewGuid();
            var expense = new Expense
            {
                Id = newGuid,
                ExpenseDetails = new ExpenseDetails
                {
                    Value = expenseDetails.Value,
                    Reason = expenseDetails.Reason,
                    Date = expenseDetails.Date,
                    PaymentMethod = PaymentMethod.Undefined
                }
            };
            var cancellationToken = new CancellationTokenSource().Token;
            ACallToInsertAsync(expense, cancellationToken).Returns(Task.CompletedTask);

            var result = await _registry.InsertAsync(expenseDetails, cancellationToken);

            ACallToInsertAsync(expense, cancellationToken).MustHaveHappenedOnceExactly();
            result.ExpenseDetails.Should().Be(expense.ExpenseDetails);
        }

        [Test]
        public async Task InsertAsync_ShouldInsertNewExpense()
        {
            var expenseDetails = AnExpenseDetails();
            var cancellationToken = new CancellationToken();
            var expense = new Expense { Id = Guid.NewGuid(), ExpenseDetails = expenseDetails };
            ACallToInsertAsync(expense, cancellationToken).Returns(Task.CompletedTask);

            var createdExpense = await _registry.InsertAsync(expenseDetails, cancellationToken);

            createdExpense.ExpenseDetails.Should().Be(expenseDetails);
        }

        #endregion

        #region UpdateAsync

        [Test]
        public async Task IfValueIsNegative_UpdateAsync_ShouldFail()
        {
            var id = Guid.NewGuid();
            var expenseDetails = new ExpenseDetails
            {
                Value = -1.0,
                Reason = RandomData.String.Alphanumeric(),
                Date = RandomData.DateTimeOffset.Past(),
                PaymentMethod = RandomData.Enum.Any<PaymentMethod>()
            };
            var cancellationToken = new CancellationToken();

            await _registry.Awaiting(r => r.UpdateAsync(id, expenseDetails, cancellationToken))
                .Should().ThrowExactlyAsync<ArgumentException>();
        }

        [Test]
        public async Task IfRepositoryThrows_UpdateAsync_ShouldFail()
        {
            var exception = new Exception();
            var id = Guid.NewGuid();
            var expenseDetails = AnExpenseDetails();
            var cancellationToken = new CancellationToken();
            A.CallTo(() => _repository.UpdateAsync(id, expenseDetails, cancellationToken)).Throws(exception);

            (await _registry.Awaiting(r => r.UpdateAsync(id, expenseDetails, cancellationToken))
                    .Should().ThrowExactlyAsync<Exception>())
                    .And.Should().Be(exception);
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateExpense()
        {
            var id = Guid.NewGuid();
            var expenseDetails = AnExpenseDetails();
            var cancellationToken = new CancellationToken();
            var expense = new Expense { Id = id, ExpenseDetails = expenseDetails };
            A.CallTo(() => _repository.UpdateAsync(id, expenseDetails, cancellationToken)).Returns(expense);

            var result = await _repository.UpdateAsync(id, expenseDetails, cancellationToken);

            result.Should().Be(expense);
            A.CallTo(() => _repository.UpdateAsync(id, expenseDetails, cancellationToken)).MustHaveHappenedOnceExactly();
        }

        #endregion

        #region DeleteAsync

        [Test]
        public async Task IfRepositoryThrows_DeleteAsync_ShouldFail()
        {
            var exception = new Exception();
            var id = Guid.NewGuid();
            var cancellationToken = new CancellationToken();
            A.CallTo(() => _repository.DeleteAsync(id, cancellationToken)).Throws(exception);

            (await _registry.Awaiting(r => r.DeleteAsync(id, cancellationToken))
                    .Should().ThrowExactlyAsync<Exception>())
                    .And.Should().Be(exception);
        }

        [Test]
        public async Task DeleteAsync_ShouldDeleteExpense()
        {
            var id = Guid.NewGuid();
            var cancellationToken = new CancellationToken();
            A.CallTo(() => _repository.DeleteAsync(id, cancellationToken)).Returns(Task.CompletedTask);

            await _repository.DeleteAsync(id, cancellationToken);

            A.CallTo(() => _repository.DeleteAsync(id, cancellationToken)).MustHaveHappenedOnceExactly();
        }

        #endregion

        #region Utility Methods

        private static Expense AnExpense()
        {
            return new Expense
            {
                Id = Guid.NewGuid(),
                ExpenseDetails = AnExpenseDetails()
            };
        }

        private static ExpenseDetails AnExpenseDetails()
        {
            return new ExpenseDetails
            {
                Value = RandomData.Double.Positive(),
                Reason = RandomData.String.Alphanumeric(),
                Date = RandomData.DateTimeOffset.Past(),
                PaymentMethod = RandomData.Enum.Any<PaymentMethod>()
            };
        }

        private IReturnValueArgumentValidationConfiguration<Task> ACallToInsertAsync(Expense expense, CancellationToken cancellationToken)
        {
            return A.CallTo(() => _repository.InsertAsync(A<Expense>.That.Matches(e => Math.Abs(e.ExpenseDetails.Value - expense.ExpenseDetails.Value) < 0.01 &&
                                                                                                e.ExpenseDetails.Reason == expense.ExpenseDetails.Reason &&
                                                                                                e.ExpenseDetails.Date == expense.ExpenseDetails.Date &&
                                                                                                e.ExpenseDetails.PaymentMethod == expense.ExpenseDetails.PaymentMethod),
                cancellationToken));
        }

        private static FilterParameters AFilterParameters()
        {
            return new FilterParameters
            {
                From = RandomData.String.Alphanumeric(),
                To = RandomData.String.Alphanumeric(),
                In = RandomData.String.Alphanumeric(),
                PaymentMethod = RandomData.Enum.Any<PaymentMethod>()
            };
        }

        #endregion
    }
}
