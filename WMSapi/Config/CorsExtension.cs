using Microsoft.Extensions.DependencyInjection;

namespace SampleWebApiAspNetCore.Helpers
{
    public static class CorsExtension
    {
        public static void AddCustomCors(this IServiceCollection services, string policyName)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(policyName,
                    builder =>
                    {
                        builder
                             .WithOrigins("http://192.168.50.56:3000", "http://localhost:3000")
                             .AllowAnyMethod()
                             .AllowAnyHeader()
                             .AllowCredentials();
            });
            });
        }
    }
}
