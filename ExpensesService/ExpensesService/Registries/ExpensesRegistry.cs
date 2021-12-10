using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExpensesService.Models;
using ExpensesService.Repositories;
using ExpensesService.Utility;
using Microsoft.Extensions.Logging;

namespace ExpensesService.Registries
{
    public class ExpensesRegistry : IExpensesRegistry
    {
        #region Private fields

        private readonly ILogger<ExpensesRegistry> _logger;
        private readonly IExpensesRepository _repository;
        private readonly IFilterFactory _filterFactory;
        private readonly IWatch _watch;

        #endregion

        #region Initialization

        public ExpensesRegistry(ILoggerFactory loggerFactory, IExpensesRepository repository, IFilterFactory filterFactory, IWatch watch)
        {
            _logger = loggerFactory is not null ? loggerFactory.CreateLogger<ExpensesRegistry>() : throw new ArgumentNullException(nameof(loggerFactory));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _filterFactory = filterFactory ?? throw new ArgumentNullException(nameof(filterFactory));
            _watch = watch ?? throw new ArgumentNullException(nameof(watch));
        }

        #endregion

        public async Task<IEnumerable<Expense>> GetAllAsync(FilterParameters filterParameters, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug($"{nameof(GetAllAsync)} invoked");
            var sw = Stopwatch.StartNew();

            var filter = _filterFactory.Create(filterParameters);

            var expenses = (await _repository.GetAllAsync(filter, cancellationToken)).ToList();

            _logger.LogDebug($"{nameof(GetAllAsync)} completed. Expenses Count: {expenses.Count}. Elapsed: {sw.Elapsed}");

            return expenses;
        }

        public async Task<Expense> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug($"{nameof(GetAsync)} invoked. ID: {id}");
            var sw = Stopwatch.StartNew();

            var expense = await _repository.GetAsync(id, cancellationToken);

            _logger.LogDebug($"{nameof(GetAsync)} completed. ID: {id}. Elapsed: {sw.Elapsed}");

            return expense;
        }

        public async Task<Expense> InsertAsync(ExpenseDetails expenseDetails, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug($"{nameof(InsertAsync)} invoked");
            var sw = Stopwatch.StartNew();

            ValidateDetails(expenseDetails);

            var newGuid = Guid.NewGuid();
            var newExpense = new Expense
            {
                Id = newGuid,
                ExpenseDetails = new ExpenseDetails
                {
                    Value = expenseDetails.Value,
                    Reason = expenseDetails.Reason,
                    Date = expenseDetails.Date ?? _watch.Now(),
                    PaymentMethod = expenseDetails.PaymentMethod ?? PaymentMethod.Undefined
                }
            };

            await _repository.InsertAsync(newExpense, cancellationToken);

            _logger.LogDebug($"{nameof(InsertAsync)} completed. ID: {newGuid}. Elapsed: {sw.Elapsed}");

            return newExpense;
        }

        public async Task<Expense> UpdateAsync(Guid id, ExpenseDetails expenseDetails, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug($"{nameof(UpdateAsync)} invoked. ID: {id}");
            var sw = Stopwatch.StartNew();

            ValidateDetails(expenseDetails);

            var updatedExpense = await _repository.UpdateAsync(id, expenseDetails, cancellationToken);

            _logger.LogDebug($"{nameof(UpdateAsync)} completed. ID: {id}. Elapsed: {sw.Elapsed}");

            return updatedExpense;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug($"{nameof(DeleteAsync)} invoked");
            var sw = Stopwatch.StartNew();

            await _repository.DeleteAsync(id, cancellationToken);

            _logger.LogDebug($"{nameof(DeleteAsync)} completed. Expenses ID: {id}. Elapsed: {sw.Elapsed}");
        }

        #region Utility Methods

        private static void ValidateDetails(ExpenseDetails expenseDetails)
        {
            if (expenseDetails.Value < 0)
            {
                throw new ArgumentException(nameof(expenseDetails.Value));
            }
        }

        #endregion
    }
}