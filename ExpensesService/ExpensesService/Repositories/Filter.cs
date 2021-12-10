using System;
using System.Collections.Generic;
using System.Linq;
using ExpensesService.Repositories.Entities;
using PaymentMethod = ExpensesService.Models.PaymentMethod;

namespace ExpensesService.Repositories
{
    public class Filter : IFilter
    {
        public Filter()
        {
            ResetValues();
        }

        private string _from;
        private string _in;
        private Tuple<string, string> _between;
        private PaymentMethod? _paymentMethod;

        public IFilter From(string startDate)
        {
            _from = startDate;
            return this;
        }

        public IFilter In(string date)
        {
            _in = date;
            return this;
        }

        public IFilter Between(string startDate, string endDate)
        {
            _between = new Tuple<string, string>(startDate, endDate);
            return this;
        }

        public IFilter WithPaymentMethod(PaymentMethod paymentMethod)
        {
            _paymentMethod = paymentMethod;
            return this;
        }

        public IEnumerable<ExpenseEntity> Apply(IEnumerable<ExpenseEntity> items)
        {
            var expenseEntities = items.ToList();
            if (_from is not null)
            {
                expenseEntities = expenseEntities.Where(i => string.Compare(i.Date, _from, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            if (_in is not null)
            {
                expenseEntities = expenseEntities.Where(i => i.Date.StartsWith(_in)).ToList();
            }

            if (_between is not null)
            {
                expenseEntities = expenseEntities.Where(i => 
                    string.Compare(i.Date, _between.Item1, StringComparison.OrdinalIgnoreCase) >= 0 &&
                    string.Compare(i.Date, _between.Item2, StringComparison.OrdinalIgnoreCase) <= 0)
                    .ToList();
            }

            if (_paymentMethod is not null)
            {
                expenseEntities = expenseEntities.Where(i => i.PaymentMethod == ToEntityPaymentMethod(_paymentMethod.Value)).ToList();
            }

            ResetValues();

            return expenseEntities;
        }

        #region Utility Methods

        private void ResetValues()
        {
            _from = null;
            _in = null;
            _between = null;
            _paymentMethod = null;
        }

        private static Entities.PaymentMethod ToEntityPaymentMethod(PaymentMethod paymentMethod)
        {
            return paymentMethod switch
            {
                PaymentMethod.Cash => Entities.PaymentMethod.Cash,
                PaymentMethod.DebitCard => Entities.PaymentMethod.DebitCard,
                PaymentMethod.CreditCard => Entities.PaymentMethod.CreditCard,
                _ => Entities.PaymentMethod.Undefined
            };
        }

        #endregion
    }
}