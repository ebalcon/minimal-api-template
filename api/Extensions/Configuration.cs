using api.Domaine.Data;
using Microsoft.EntityFrameworkCore;

namespace api.Extensions
{
    public static class Configuration
    {
        public static void RegisterServices(this WebApplicationBuilder builder)
        {
            var configuration = builder.Configuration;

            builder.Services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen();

            builder.Services.AddDbContext<SqlContext>(options => options.UseSqlServer(configuration.GetConnectionString("Default")));
        }

        public static void RegisterMiddlewares(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger()
                   .UseSwaggerUI();
            }

            app.UseHttpsRedirection();
        }
    }
}
