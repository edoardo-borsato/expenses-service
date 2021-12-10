using ExpensesService.Repositories;

namespace ExpensesService.Registries
{
    public class FilterFactory : IFilterFactory
    {
        public IFilter Create(FilterParameters parameters)
        {
            // TODO: create IFilter implementation class and substitute
            IFilter filter = null;

            if (parameters is not null)
            {
                AddPaymentMethodFilter(parameters, filter);
                AddDateFilters(parameters, filter);
            }

            return filter;
        }

        #region Utility Methods

        private static void AddDateFilters(FilterParameters parameters, IFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(parameters.From))
            {
                if (!string.IsNullOrWhiteSpace(parameters.To))
                {
                    filter.Between(parameters.From, parameters.To);
                }
                else
                {
                    filter.From(parameters.From);
                }
            }

            if (!string.IsNullOrWhiteSpace(parameters.In))
            {
                filter.In(parameters.In);
            }
        }

        private static void AddPaymentMethodFilter(FilterParameters parameters, IFilter filter)
        {
            if (parameters.PaymentMethod is not null)
            {
                filter.WithPaymentMethod(parameters.PaymentMethod.Value);
            }
        }

        #endregion
    }
}