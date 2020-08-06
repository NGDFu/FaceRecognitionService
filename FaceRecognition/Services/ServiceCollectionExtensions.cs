using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FaceRecognition.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFaceRecognitionService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ServiceOption>(configuration.GetSection(nameof(ServiceOption)));
            services.AddSingleton<FaceRecognitionService>();
            return services;
        }

        public static IApplicationBuilder UseFaceRecognitionService(this IApplicationBuilder app)
        {
            var service = app.ApplicationServices.GetRequiredService<FaceRecognitionService>();
            service.Start();

            return app;
        }
    }
}
