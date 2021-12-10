using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ExpensesService.Models;
using ExpensesService.Utility.CosmosDB;

namespace ExpensesService.Repositories
{
    public class ExpensesRepository : IExpensesRepository
    {
        private readonly IContainer _container;

        public ExpensesRepository(IContainer container)
        {
            _container = container;
        }

        public Task<IEnumerable<Expense>> GetAllAsync(IFilter filter, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Expense> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task InsertAsync(Expense expense, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Expense> UpdateAsync(Guid id, ExpenseDetails expenseDetails, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}