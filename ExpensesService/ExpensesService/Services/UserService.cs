using System;

namespace ExpensesService.Services
{
    public class UserService : IUserService
    {
        #region Private fields

        private readonly string _username;
        private readonly string _password;

        #endregion

        #region Initialization

        public UserService(string username, string password)
        {
            _username = username ?? throw new ArgumentException(nameof(username));
            _password = password ?? throw new ArgumentException(nameof(password));
        }

        #endregion

        public bool Validate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException(nameof(username));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException(nameof(password));
            }

            return username.Equals(_username) && password.Equals(_password);
        }
    }
}