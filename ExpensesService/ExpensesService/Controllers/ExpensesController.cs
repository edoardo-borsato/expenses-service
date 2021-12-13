using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExpensesService.Models;
using ExpensesService.Registries;
using ExpensesService.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ExpensesService.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/expenses", Name = "expenses")]
    public class ExpensesController : ControllerBase
    {
        #region Private fields

        private readonly IQueryParametersValidator _validator;
        private readonly IExpensesRegistry _registry;
        private readonly ILogger<ExpensesController> _logger;

        #endregion

        #region Initialization

        public ExpensesController(ILoggerFactory loggerFactory, IExpensesRegistry registry, IQueryParametersValidator validator)
        {
            _logger = loggerFactory is not null ? loggerFactory.CreateLogger<ExpensesController>() : throw new ArgumentNullException(nameof(loggerFactory));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        #endregion

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Expense>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(499, Type = typeof(Error))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Error))]
        public async Task<ActionResult> GetAllAsync([FromQuery] GetAllQueryParameters queryParameters, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"{nameof(GetAllAsync)} invoked");
            var sw = Stopwatch.StartNew();

            try
            {
                var filterParameters = _validator.Validate(queryParameters);

                var expenses = await _registry.GetAllAsync(filterParameters, cancellationToken);

                _logger.LogInformation($"{nameof(GetAllAsync)} completed. Expenses count: {expenses.Count()}. Elapsed time: {sw.Elapsed}");

                return Ok(expenses);
            }
            catch (FormatException e)
            {
                _logger.LogError(e, $"Invalid query parameter. Elapsed time: {sw.Elapsed}");
                return BadRequest(new Error { ErrorMessage = $"Invalid query parameter: {e.Message}" });
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"Operation cancelled by client. Elapsed time: {sw.Elapsed}");
                return Cancelled(new Error { ErrorMessage = "Operation cancelled by client" });
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"An error occurred while retrieving expenses. Elapsed time: {sw.Elapsed}");
                return InternalServerError(new Error { ErrorMessage = $"An error occurred: {e.Message}" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Expense))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Error))]
        [ProducesResponseType(499, Type = typeof(Error))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Error))]
        public async Task<ActionResult> GetAsync([FromRoute(Name = "id")] Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"{nameof(GetAsync)} invoked");
            var sw = Stopwatch.StartNew();

            try
            {
                var expense = await _registry.GetAsync(id, cancellationToken);
                if (expense is null)
                {
                    var errorMessage = $"No expense found with given ID: {id}";
                    _logger.LogError($"{errorMessage}. Elapsed time: {sw.Elapsed}");
                    return NotFound(new Error { ErrorMessage = errorMessage });
                }

                _logger.LogInformation($"{nameof(GetAsync)} completed. Expense: {expense}. Elapsed time: {sw.Elapsed}");

                return Ok(expense);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"Operation cancelled by client. Elapsed time: {sw.Elapsed}");
                return Cancelled(new Error { ErrorMessage = "Operation cancelled by client" });
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"An error occurred while retrieving expense. Elapsed time: {sw.Elapsed}");
                return InternalServerError(new Error { ErrorMessage = $"An error occurred: {e.Message}" });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Expense))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(499, Type = typeof(Error))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Error))]
        public async Task<ActionResult> CreateAsync([FromBody] ExpenseDetails expenseDetails, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"{nameof(CreateAsync)} invoked");
            var sw = Stopwatch.StartNew();

            try
            {
                var createdExpense = await _registry.InsertAsync(expenseDetails, cancellationToken);

                _logger.LogInformation($"{nameof(CreateAsync)} completed. Expense: {createdExpense}. Elapsed time: {sw.Elapsed}");

                return CreatedAtRoute("expenses", createdExpense);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e, $"Invalid input argument. Elapsed time: {sw.Elapsed}");
                return BadRequest(new Error { ErrorMessage = e.Message });
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"Operation cancelled by client. Elapsed time: {sw.Elapsed}");
                return Cancelled(new Error { ErrorMessage = "Operation cancelled by client" });
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"An error occurred while retrieving expenses. Elapsed time: {sw.Elapsed}");
                return InternalServerError(new Error { ErrorMessage = $"An error occurred: {e.Message}" });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Expense))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Error))]
        [ProducesResponseType(499, Type = typeof(Error))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Error))]
        public async Task<ActionResult> UpdateAsync([FromRoute(Name = "id")] Guid id, [FromBody] ExpenseDetails expenseDetails, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"{nameof(UpdateAsync)} invoked");
            var sw = Stopwatch.StartNew();

            try
            {
                var updatedRecord = await _registry.UpdateAsync(id, expenseDetails, cancellationToken);

                _logger.LogInformation($"{nameof(UpdateAsync)} completed. Updated record: {updatedRecord}. Elapsed time: {sw.Elapsed}");

                return Ok(updatedRecord);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e, $"Invalid input argument. Elapsed time: {sw.Elapsed}");
                return BadRequest(new Error { ErrorMessage = e.Message });
            }
            catch (NotFoundException e)
            {
                var errorMessage = $"No expense found with given ID: {id}";
                _logger.LogError(e, $"{errorMessage}. Elapsed time: {sw.Elapsed}");
                return NotFound(new Error { ErrorMessage = errorMessage });
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"Operation cancelled by client. Elapsed time: {sw.Elapsed}");
                return Cancelled(new Error { ErrorMessage = "Operation cancelled by client" });
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"An error occurred while updating expense with ID: {id}. Elapsed time: {sw.Elapsed}");
                return InternalServerError(new Error { ErrorMessage = $"An error occurred: {e.Message}" });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(499, Type = typeof(Error))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Error))]
        public async Task<ActionResult> DeleteAsync([FromRoute(Name = "id")] Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"{nameof(DeleteAsync)} invoked");
            var sw = Stopwatch.StartNew();

            try
            {
                await _registry.DeleteAsync(id, cancellationToken);

                _logger.LogInformation($"{nameof(DeleteAsync)} completed. Elapsed time: {sw.Elapsed}");

                return NoContent();
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"Operation cancelled by client. Elapsed time: {sw.Elapsed}");
                return Cancelled(new Error { ErrorMessage = "Operation cancelled by client" });
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"An error occurred while deleting expense with ID: {id}. Elapsed time: {sw.Elapsed}");
                return InternalServerError(new Error { ErrorMessage = $"An error occurred: {e.Message}" });
            }
        }

        #region Utility Methods

        private ObjectResult Cancelled(object value)
        {
            return StatusCode(499, value);
        }

        private ObjectResult InternalServerError(object value)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, value);
        }

        #endregion
    }
}
