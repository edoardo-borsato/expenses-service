using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpensesService.Controllers;
using ExpensesService.Registries;
using ExpensesService.Repositories;
using ExpensesService.Utility;

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
            var awsAccessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY");
            var awsSecretKey = Environment.GetEnvironmentVariable("AWS_SECRET_KEY");
            var loggerFactory = LoggerFactory.Create(builder => builder
                .AddConsole()
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Information)
                .AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Information)
                .SetMinimumLevel(LogLevel.Debug));

            var watch = new Watch();
            // TODO: create ExpensesRepository implementation class and substitute
            IExpensesRepository repository = null;
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
