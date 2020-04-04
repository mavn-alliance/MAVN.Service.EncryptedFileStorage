using JetBrains.Annotations;
using Lykke.Sdk;
using MAVN.Service.EncryptedFileStorage.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using AutoMapper;
using MAVN.Service.EncryptedFileStorage.Auth;
using MAVN.Service.EncryptedFileStorage.Middleware;
using MAVN.Service.EncryptedFileStorage.Profiles;
using Microsoft.AspNetCore.Mvc;

namespace MAVN.Service.EncryptedFileStorage
{
    [UsedImplicitly]
    public class Startup
    {
        private readonly LykkeSwaggerOptions _swaggerOptions = new LykkeSwaggerOptions
        {
            ApiTitle = "EncryptedFileStorage API", ApiVersion = "v1"
        };

        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return services.BuildServiceProvider<AppSettings>(options =>
            {
                options.SwaggerOptions = _swaggerOptions;

                options.Logs = logs =>
                {
                    logs.AzureTableName = "EncryptedFileStorageLog";
                    logs.AzureTableConnectionStringResolver =
                        settings => settings.EncryptedFileStorageService.Db.LogsConnString;

                    options.Extend = (sc, settings) =>
                    {
                        sc.Configure<ApiBehaviorOptions>(apiBehaviorOptions =>
                            {
                                apiBehaviorOptions.SuppressModelStateInvalidFilter = true;
                            })
                            .AddAuthentication(KeyAuthOptions.AuthenticationScheme)
                            .AddScheme<KeyAuthOptions, KeyAuthHandler>(KeyAuthOptions.AuthenticationScheme, "",
                                opts => { });

                        sc.AddAutoMapper(typeof(AutoMapperProfile));
                    };

                    options.Swagger = swagger =>
                    {
                        swagger.OperationFilter<ApiKeyHeaderOperationFilter>();
                    };
                };
            });
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app)
        {
            app.UseLykkeConfiguration(options =>
            {
                options.SwaggerOptions = _swaggerOptions;

                options.WithMiddleware = x =>
                {
                    x.UseMiddleware<BadRequestExceptionMiddleware>();
                    x.UseAuthentication();
                };
            });
        }
    }
}
