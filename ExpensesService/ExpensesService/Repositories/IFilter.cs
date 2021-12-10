using System.Collections.Generic;
using PaymentMethod = ExpensesService.Models.PaymentMethod;

namespace ExpensesService.Repositories
{
    public interface IFilter
    {
        IFilter From(string startDate);
        IFilter In(string date);
        IFilter Between(string startDate, string endDate);
        IFilter WithPaymentMethod(PaymentMethod paymentMethod);
        IEnumerable<T> Apply<T>(IEnumerable<T> items);
    }
}