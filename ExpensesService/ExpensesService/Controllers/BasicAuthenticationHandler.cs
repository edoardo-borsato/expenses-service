using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using ExpensesService.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ExpensesService.Controllers
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        #region Private fields

        private readonly IUserService _userService;

        #endregion

        #region Initialization

        public BasicAuthenticationHandler(
            IUserService userService, 
            IOptionsMonitor<AuthenticationSchemeOptions> options, 
            ILoggerFactory loggerFactory, 
            UrlEncoder encoder, 
            ISystemClock clock) 
            : base(options, loggerFactory, encoder, clock)
        {
            _userService = userService ?? throw new ArgumentException(nameof(userService));
        }

        #endregion

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string username;
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                if (!string.IsNullOrWhiteSpace(authHeader.Parameter))
                {
                    var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter)).Split(':');
                    username = credentials.FirstOrDefault();
                    var password = credentials.LastOrDefault();

                    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || !_userService.Validate(username, password))
                    {
                        throw new ArgumentException("Invalid credentials");
                    }
                }
                else
                {
                    throw new ArgumentException("Missing Authorization header values");
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(AuthenticateResult.Fail($"Authentication failed: {ex.Message}"));
            }

            var claims = new[] { new Claim(ClaimTypes.Name, username) };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}