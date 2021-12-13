using ExpensesService.Repositories;

namespace ExpensesService.Registries
{
    public interface IFilterFactory
    {
        IFilter Create(FilterParameters parameters);
    }
}