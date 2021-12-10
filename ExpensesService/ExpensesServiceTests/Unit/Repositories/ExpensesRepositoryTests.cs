using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExpensesService.Models;
using ExpensesService.Repositories;
using ExpensesService.Repositories.Entities;
using ExpensesService.Utility;
using ExpensesService.Utility.CosmosDB;
using ExpensesServiceTests.Utility;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using NUnit.Framework;
using PaymentMethod = ExpensesService.Repositories.Entities.PaymentMethod;

namespace ExpensesServiceTests.Unit.Repositories
{
    [TestFixture]
    internal class ExpensesRepositoryTests
    {
        #region Fixture

        private IContainer _container;
        private IExpensesRepository _repository;

        #endregion

        #region Setup & Teardown

        [SetUp]
        public void SetUp()
        {
            _container = A.Fake<IContainer>();

            _repository = new ExpensesRepository(_container);
        }

        #endregion

        #region GetAllAsync

        [Test]
        public async Task IfContainerThrows_GetAllAsync_ShouldFail()
        {
            var exception = new Exception();
            var filter = A.Fake<IFilter>();
            var cancellationToken = new CancellationToken();
            A.CallTo(() => _container.GetItemLinqQueryable<ExpenseEntity>(cancellationToken)).Throws(exception);

            (await _repository.Awaiting(r => r.GetAllAsync(filter, cancellationToken))
                    .Should().ThrowExactlyAsync<Exception>())
                    .And.Should().Be(exception);

            A.CallTo(() => filter.Apply(null)).WithAnyArguments().MustNotHaveHappened();
        }

        [Test]
        public async Task IfFilterThrows_GetAllAsync_ShouldFail()
        {
            var exception = new Exception();
            var filter = A.Fake<IFilter>();
            var cancellationToken = new CancellationToken();
            var expensesEntities = new List<ExpenseEntity>
            {
                AnExpenseEntity(),
                AnExpenseEntity(),
                AnExpenseEntity()
            };
            A.CallTo(() => _container.GetItemLinqQueryable<ExpenseEntity>(cancellationToken)).Returns(expensesEntities);
            A.CallTo(() => filter.Apply(expensesEntities)).Throws(exception);

            (await _repository.Awaiting(r => r.GetAllAsync(filter, cancellationToken))
                    .Should().ThrowExactlyAsync<Exception>())
                    .And.Should().Be(exception);
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnExpenses()
        {
            var filter = A.Fake<IFilter>();
            var cancellationToken = new CancellationToken();
            var expensesEntities = new List<ExpenseEntity>
            {
                AnExpenseEntity(),
                AnExpenseEntity(),
                AnExpenseEntity()
            };
            var filteredExpesensEntities = new List<ExpenseEntity>
            {
                AnExpenseEntity(),
                AnExpenseEntity()
            };
            A.CallTo(() => _container.GetItemLinqQueryable<ExpenseEntity>(cancellationToken)).Returns(expensesEntities);
            A.CallTo(() => filter.Apply(expensesEntities)).Returns(filteredExpesensEntities);

            var expenses = (await _repository.GetAllAsync(filter, cancellationToken)).ToList();

            expenses.Should().HaveCount(2);
            ShouldMatch(expenses[0], filteredExpesensEntities[0]);
            ShouldMatch(expenses[1], filteredExpesensEntities[1]);
        }

        #endregion

        #region GetAsync

        [Test]
        public async Task IfContainerThrowsNotFound_GetAsync_ShouldFail()
        {
            var exception = new NotFoundException();
            var id = Guid.NewGuid();
            var partitionKey = new PartitionKey(id.ToString());
            var cancellationToken = new CancellationToken();
            A.CallTo(() => _container.ReadItemAsync<ExpenseEntity>(id.ToString(), partitionKey, null, cancellationToken)).Throws(exception);

            var expense = await _repository.GetAsync(id, cancellationToken);

            expense.Should().BeNull();
        }

        [Test]
        public async Task IfContainerThrows_GetAsync_ShouldFail()
        {
            var exception = new Exception();
            var id = Guid.NewGuid();
            var partitionKey = new PartitionKey(id.ToString());
            var cancellationToken = new CancellationToken();
            A.CallTo(() => _container.ReadItemAsync<ExpenseEntity>(id.ToString(), partitionKey, null, cancellationToken)).Throws(exception);

            (await _repository.Awaiting(r => r.GetAsync(id, cancellationToken))
                    .Should().ThrowExactlyAsync<Exception>())
                    .And.Should().Be(exception);
        }

        [Test]
        public async Task GetAsync_ShouldReturnExpense()
        {
            var id = Guid.NewGuid();
            var partitionKey = new PartitionKey(id.ToString());
            var cancellationToken = new CancellationToken();
            var expenseEntity = AnExpenseEntity();
            A.CallTo(() => _container.ReadItemAsync<ExpenseEntity>(id.ToString(), partitionKey, null, cancellationToken)).Returns(expenseEntity);

            var expense = await _repository.GetAsync(id, cancellationToken);

            ShouldMatch(expense, expenseEntity);
            A.CallTo(() => _container.ReadItemAsync<ExpenseEntity>(id.ToString(), partitionKey, null, cancellationToken)).MustHaveHappenedOnceExactly();
        }

        #endregion

        #region InsertAsync

        [Test]
        public async Task IfContainerThrows_InsertAsync_ShouldFail()
        {
            var exception = new Exception();
            var cancellationToken = new CancellationToken();
            var expense = AnExpense();
            var expenseEntity = ToExpenseEntity(expense);
            A.CallTo(() => _container.CreateItemAsync(expenseEntity, new PartitionKey(expenseEntity.Id), null, cancellationToken)).Throws(exception);

            (await _repository.Awaiting(r => r.InsertAsync(expense, cancellationToken))
                    .Should().ThrowExactlyAsync<Exception>())
                    .And.Should().Be(exception);
        }

        [Test]
        public async Task InsertAsync_ShouldInsertExpense()
        {
            var cancellationToken = new CancellationToken();
            var expense = AnExpense();
            var expenseEntity = ToExpenseEntity(expense);
            A.CallTo(() => _container.CreateItemAsync(expenseEntity, new PartitionKey(expenseEntity.Id), null, cancellationToken)).Returns(Task.CompletedTask);

            await _repository.InsertAsync(expense, cancellationToken);

            A.CallTo(() => _container.CreateItemAsync(expenseEntity, new PartitionKey(expenseEntity.Id), null, cancellationToken)).MustHaveHappenedOnceExactly();
        }

        #endregion

        #region DeleteAsync

        [Test]
        public async Task IfContainerThrows_DeleteAsync_ShouldFail()
        {
            var exception = new Exception();
            var id = Guid.NewGuid();
            var partitionKey = new PartitionKey(id.ToString());
            var cancellationToken = new CancellationToken();
            A.CallTo(() => _container.DeleteItemAsync<ExpenseEntity>(id.ToString(), partitionKey, null, cancellationToken)).Throws(exception);

            (await _repository.Awaiting(r => r.DeleteAsync(id, cancellationToken))
                    .Should().ThrowExactlyAsync<Exception>())
                    .And.Should().Be(exception);
        }

        [Test]
        public async Task DeleteAsync_ShouldDeleteExpense()
        {
            var id = Guid.NewGuid();
            var partitionKey = new PartitionKey(id.ToString());
            var cancellationToken = new CancellationToken();
            A.CallTo(() => _container.DeleteItemAsync<ExpenseEntity>(id.ToString(), partitionKey, null, cancellationToken)).Returns(Task.CompletedTask);

            await _repository.DeleteAsync(id, cancellationToken);

            A.CallTo(() => _container.DeleteItemAsync<ExpenseEntity>(id.ToString(), partitionKey, null, cancellationToken)).MustHaveHappenedOnceExactly();
        }

        #endregion

        #region UpdateAsync

        [Test]
        public async Task IfContainerReadThrows_UpdateAsync_ShouldFail()
        {
            var exception = new Exception();
            var id = Guid.NewGuid();
            var partitionKey = new PartitionKey(id.ToString());
            var expense = AnExpense();
            var cancellationToken = new CancellationToken();
            A.CallTo(() => _container.ReadItemAsync<ExpenseEntity>(id.ToString(), partitionKey, null, cancellationToken)).Throws(exception);

            (await _repository.Awaiting(r => r.UpdateAsync(id, expense.ExpenseDetails, cancellationToken))
                    .Should().ThrowExactlyAsync<Exception>())
                    .And.Should().Be(exception);

            A.CallTo(() => _container.UpsertItemAsync(expense, null, null, cancellationToken)).MustNotHaveHappened();
        }

        [Test]
        public async Task IfContainerUpsertThrows_UpdateAsync_ShouldFail()
        {
            var exception = new Exception();
            var id = Guid.NewGuid();
            var partitionKey = new PartitionKey(id.ToString());
            var expenseDetails = AnExpenseDetails();
            var expenseEntity = AnExpenseEntityWithGiven(id, AnExpenseDetails());
            var cancellationToken = new CancellationToken();
            A.CallTo(() => _container.ReadItemAsync<ExpenseEntity>(id.ToString(), partitionKey, null, cancellationToken)).Returns(expenseEntity);
            A.CallTo(() => _container.UpsertItemAsync(A<ExpenseEntity>.That.Matches(e => e.Id == id.ToString()), partitionKey, null, cancellationToken)).Throws(exception);

            (await _repository.Awaiting(r => r.UpdateAsync(id, expenseDetails, cancellationToken))
                    .Should().ThrowExactlyAsync<Exception>())
                    .And.Should().Be(exception);
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateExpense()
        {
            var id = Guid.NewGuid();
            var partitionKey = new PartitionKey(id.ToString());
            var originalExpenseDetails = AnExpenseDetails();
            var expenseEntity = AnExpenseEntityWithGiven(id, originalExpenseDetails);
            var cancellationToken = new CancellationToken();
            var newExpenseDetails = AnExpenseDetails();
            var updatedExpenseEntity = AnExpenseEntityWithGiven(id, newExpenseDetails);
            A.CallTo(() => _container.ReadItemAsync<ExpenseEntity>(id.ToString(), partitionKey, null, cancellationToken)).Returns(expenseEntity);
            A.CallTo(() => _container.UpsertItemAsync(updatedExpenseEntity, partitionKey, null, cancellationToken)).Returns(Task.CompletedTask);

            var updatedExpense = await _repository.UpdateAsync(id, newExpenseDetails, cancellationToken);

            ShouldMatch(updatedExpense, updatedExpenseEntity);
            A.CallTo(() => _container.ReadItemAsync<ExpenseEntity>(id.ToString(), partitionKey, null, cancellationToken)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => _container.UpsertItemAsync(updatedExpenseEntity, partitionKey, null, cancellationToken)).MustHaveHappenedOnceExactly());
        }

        #endregion

        #region Utility Methods

        private static ExpenseEntity ToExpenseEntity(Expense expense)
        {
            return new ExpenseEntity
            {
                Id = expense.Id.ToString(),
                Value = expense.ExpenseDetails.Value,
                Reason = expense.ExpenseDetails.Reason,
                // ReSharper disable once PossibleInvalidOperationException
                Date = expense.ExpenseDetails.Date.Value.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                PaymentMethod = ToEntityPaymentMethod(expense.ExpenseDetails.PaymentMethod)
            };
        }

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
                PaymentMethod = RandomData.Enum.Any<ExpensesService.Models.PaymentMethod>()
            };
        }

        private static ExpenseEntity AnExpenseEntity()
        {
            return new ExpenseEntity
            {
                Id = Guid.NewGuid().ToString(),
                Value = RandomData.Double.Positive(),
                Reason = RandomData.String.Alphanumeric(),
                Date = RandomData.DateTimeOffset.Past().ToString("yyyy-MM-ddTHH:mm:ssZ"),
                PaymentMethod = RandomData.Enum.Any<PaymentMethod>()
            };
        }

        private static ExpenseEntity AnExpenseEntityWithGiven(Guid id, ExpenseDetails expenseDetails)
        {
            return new ExpenseEntity
            {
                Id = id.ToString(),
                Value = expenseDetails.Value,
                Reason = expenseDetails.Reason,
                // ReSharper disable once PossibleInvalidOperationException
                Date = expenseDetails.Date.Value.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                PaymentMethod = ToEntityPaymentMethod(expenseDetails.PaymentMethod)
            };
        }

        private static void ShouldMatch(Expense expense, ExpenseEntity expenseEntity)
        {
            expense.Id.Should().Be(expenseEntity.Id);
            expense.ExpenseDetails.Value.Should().Be(expenseEntity.Value);
            expense.ExpenseDetails.Reason.Should().Be(expenseEntity.Reason);
            // ReSharper disable once PossibleInvalidOperationException
            expense.ExpenseDetails.Date.Value.ToString("yyyy-MM-ddTHH:mm:ssZ").Should().Be(expenseEntity.Date);
            expense.ExpenseDetails.PaymentMethod.Should().Be(ToModelPaymentMethod(expenseEntity.PaymentMethod));
        }

        private static ExpensesService.Models.PaymentMethod? ToModelPaymentMethod(PaymentMethod paymentMethod)
        {
            return paymentMethod switch
            {
                PaymentMethod.Cash => ExpensesService.Models.PaymentMethod.Cash,
                PaymentMethod.DebitCard => ExpensesService.Models.PaymentMethod.DebitCard,
                PaymentMethod.CreditCard => ExpensesService.Models.PaymentMethod.CreditCard,
                _ => ExpensesService.Models.PaymentMethod.Undefined
            };
        }

        private static PaymentMethod ToEntityPaymentMethod(ExpensesService.Models.PaymentMethod? paymentMethod)
        {
            return paymentMethod switch
            {
                ExpensesService.Models.PaymentMethod.Cash => PaymentMethod.Cash,
                ExpensesService.Models.PaymentMethod.DebitCard => PaymentMethod.DebitCard,
                ExpensesService.Models.PaymentMethod.CreditCard => PaymentMethod.CreditCard,
                _ => PaymentMethod.Undefined
            };
        }

        #endregion
    }
}