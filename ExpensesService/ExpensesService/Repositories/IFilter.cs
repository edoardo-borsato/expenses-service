using System.Collections.Generic;
using ExpensesService.Models;

namespace ExpensesService.Repositories
{
    public interface IFilter
    {
        IFilter From(string startDate);
        IFilter In(string date);
        IFilter Between(string startDate, string endDate);
        IFilter WithPaymentMethod(PaymentMethod paymentMethod);
        IEnumerable<ScanCondition> GetScanConditions();
    }

    public class ScanCondition
    {
    }
}