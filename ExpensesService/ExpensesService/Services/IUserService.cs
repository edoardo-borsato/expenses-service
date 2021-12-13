namespace ExpensesService.Services
{
    public interface IUserService
    {
        bool Validate(string username, string password);
    }
}
