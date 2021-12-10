using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExpensesService.Models;
using ExpensesService.Repositories.Entities;
using ExpensesService.Utility;
using ExpensesService.Utility.CosmosDB;
using Microsoft.Azure.Cosmos;

namespace ExpensesService.Repositories
{
    public class ExpensesRepository : IExpensesRepository
    {
        private readonly IContainer _container;

        public ExpensesRepository(IContainer container)
        {
            _container = container;
        }

        public async Task<IEnumerable<Expense>> GetAllAsync(IFilter filter, CancellationToken cancellationToken = default)
        {
            var expensesEntities = await _container.GetItemLinqQueryable<ExpenseEntity>(cancellationToken);

            expensesEntities = filter.Apply(expensesEntities);

            return expensesEntities.Select(ToExpense);
        }

        public async Task<Expense> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var partitionKeyValue = id.ToString();
            ExpenseEntity expenseEntity = null;
            try
            {
                expenseEntity = await _container.ReadItemAsync<ExpenseEntity>(partitionKeyValue, new PartitionKey(partitionKeyValue), null, cancellationToken);
            }
            catch (NotFoundException)
            {
                // ignored
            }

            return ToExpense(expenseEntity);
        }

        public async Task InsertAsync(Expense expense, CancellationToken cancellationToken = default)
        {
            var expenseEntity = ToExpenseEntity(expense);
            await _container.CreateItemAsync(expenseEntity, new PartitionKey(expenseEntity.Id), null, cancellationToken);
        }

        public async Task<Expense> UpdateAsync(Guid id, ExpenseDetails expenseDetails, CancellationToken cancellationToken = default)
        {
            var partitionKeyValue = id.ToString();
            var partitionKey = new PartitionKey(partitionKeyValue);
            var expenseEntity = await _container.ReadItemAsync<ExpenseEntity>(partitionKeyValue, partitionKey, null, cancellationToken);

            UpdateExpenseEntity(expenseDetails, expenseEntity);

            await _container.UpsertItemAsync(expenseEntity, partitionKey, null, cancellationToken);

            return ToExpense(expenseEntity);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var partitionKeyValue = id.ToString();
            await _container.DeleteItemAsync<ExpenseEntity>(partitionKeyValue, new PartitionKey(partitionKeyValue), null, cancellationToken);
        }

        #region Utility Methods

        private static Expense ToExpense(ExpenseEntity expenseEntity)
        {
            if (expenseEntity is null)
            {
                return null;
            }

            return new Expense
            {
                Id = Guid.Parse(expenseEntity.Id),
                ExpenseDetails = new ExpenseDetails
                {
                    Value = expenseEntity.Value,
                    Reason = expenseEntity.Reason,
                    Date = DateTimeOffset.Parse(expenseEntity.Date),
                    PaymentMethod = ToModelPaymentMethod(expenseEntity.PaymentMethod)
                }
            };
        }

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

        private static Entities.PaymentMethod ToEntityPaymentMethod(Models.PaymentMethod? paymentMethod)
        {
            return paymentMethod switch
            {
                Models.PaymentMethod.Cash => Entities.PaymentMethod.Cash,
                Models.PaymentMethod.DebitCard => Entities.PaymentMethod.DebitCard,
                Models.PaymentMethod.CreditCard => Entities.PaymentMethod.CreditCard,
                _ => Entities.PaymentMethod.Undefined
            };
        }

        private static Models.PaymentMethod? ToModelPaymentMethod(Entities.PaymentMethod paymentMethod)
        {
            return paymentMethod switch
            {
                Entities.PaymentMethod.Cash => Models.PaymentMethod.Cash,
                Entities.PaymentMethod.DebitCard => Models.PaymentMethod.DebitCard,
                Entities.PaymentMethod.CreditCard => Models.PaymentMethod.CreditCard,
                _ => Models.PaymentMethod.Undefined
            };
        }

        private static void UpdateExpenseEntity(ExpenseDetails expense, ExpenseEntity expenseEntity)
        {
            expenseEntity.Value = expense.Value;
            expenseEntity.Reason = expense.Reason;
            // ReSharper disable once PossibleInvalidOperationException
            expenseEntity.Date = expense.Date.Value.ToString("yyyy-MM-ddTHH:mm:ssZ");
            expenseEntity.PaymentMethod = ToEntityPaymentMethod(expense.PaymentMethod);
        }

        #endregion
    }
}