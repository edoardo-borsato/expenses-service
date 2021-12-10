using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using ExpensesService.Controllers;
using ExpensesService.Registries;
using ExpensesService.Repositories;
using ExpensesService.Settings;
using ExpensesService.Utility;
using ExpensesService.Utility.CosmosDB;
using Microsoft.Azure.Cosmos;

namespace ExpensesService
{
    public class Startup
    {
        #region Private fields

        #endregion

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var loggerFactory = LoggerFactory.Create(builder => builder
                .AddConsole()
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Information)
                .AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Information)
                .SetMinimumLevel(LogLevel.Debug));

            var cosmosDbSettings = Configuration.GetSection("CosmosDB").Get<CosmosDb>();
            var cosmosClient = new CosmosClient(cosmosDbSettings.Account, cosmosDbSettings.Key);
            var cosmosClientWrapper = new CosmosClientWrapper(cosmosClient);
            var expensesContainer = cosmosClientWrapper.GetContainer(cosmosDbSettings.DatabaseName, cosmosDbSettings.ContainerName);
            var watch = new Watch();
            var repository = new ExpensesRepository(expensesContainer);
            var filterFactory = new FilterFactory();
            var validator = new QueryParametersValidator();
            var registry = new ExpensesRegistry(loggerFactory, repository, filterFactory, watch);

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ExpensesTracker", Version = "v1" });
            });
            services.AddSingleton(_ => loggerFactory);
            services.AddSingleton<IExpensesRegistry>(_ => registry);
            services.AddSingleton<IQueryParametersValidator>(_ => validator);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ExpensesTracker v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
