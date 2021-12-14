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
using ExpensesService.Services;
using ExpensesService.Settings;
using ExpensesService.Utility;
using ExpensesService.Utility.CosmosDB;
using Microsoft.AspNetCore.Authentication;
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

            var authenticationSettings = Configuration.GetSection("Authentication").Get<Authentication>();
            var cosmosDbSettings = Configuration.GetSection("CosmosDB").Get<CosmosDb>();
            var cosmosClient = new CosmosClient(cosmosDbSettings.AccountEndpoint, cosmosDbSettings.Key);
            var cosmosClientWrapper = new CosmosClientWrapper(cosmosClient);
            var expensesContainer = cosmosClientWrapper.GetContainer(cosmosDbSettings.DatabaseName, cosmosDbSettings.ContainerName);
            var watch = new Watch();
            var repository = new ExpensesRepository(expensesContainer);
            var filterFactory = new FilterFactory();
            var validator = new QueryParametersValidator();
            var registry = new ExpensesRegistry(loggerFactory, repository, filterFactory, watch);
            var userService = new UserService(authenticationSettings.Username, authenticationSettings.Password);

            services.AddControllers();
            services.AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
            services.AddSingleton<IUserService>(_ => userService);
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ExpensesService", Version = "v1" });
                c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "basic",
                    In = ParameterLocation.Header,
                    Description = "Basic Authorization header."
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "basic"
                            }
                        },
                        new string[] {}
                    }
                });
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
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ExpensesService v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
