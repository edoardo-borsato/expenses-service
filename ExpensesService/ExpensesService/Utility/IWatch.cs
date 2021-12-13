using System;

namespace ExpensesService.Utility
{
    public interface IWatch
    {
        DateTimeOffset Now();
    }
}
