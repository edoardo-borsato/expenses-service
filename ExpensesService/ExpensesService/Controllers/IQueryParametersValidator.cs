using ExpensesService.Registries;

namespace ExpensesService.Controllers
{
    public interface IQueryParametersValidator
    {
        FilterParameters Validate(GetAllQueryParameters queryParameters);
    }
}