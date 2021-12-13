using System.Collections.Generic;
using ExpensesService.Repositories.Entities;
using PaymentMethod = ExpensesService.Models.PaymentMethod;

namespace ExpensesService.Repositories
{
    public interface IFilter
    {
        IFilter From(string startDate);
        IFilter In(string date);
        IFilter Between(string startDate, string endDate);
        IFilter WithPaymentMethod(PaymentMethod paymentMethod);
        IEnumerable<ExpenseEntity> Apply(IEnumerable<ExpenseEntity> items);
    }
}