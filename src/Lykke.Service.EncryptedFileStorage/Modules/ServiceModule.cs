using System;
using Autofac;
using AzureStorage;
using AzureStorage.Blob;
using JetBrains.Annotations;
using Lykke.Common.MsSql;
using Lykke.Service.EncryptedFileStorage.Auth;
using Lykke.Service.EncryptedFileStorage.AzureRepositories;
using Lykke.Service.EncryptedFileStorage.Client;
using Lykke.Service.EncryptedFileStorage.Domain.Repositories;
using Lykke.Service.EncryptedFileStorage.Domain.Services;
using Lykke.Service.EncryptedFileStorage.DomainServices;
using Lykke.Service.EncryptedFileStorage.MsSqlRepositories;
using Lykke.Service.EncryptedFileStorage.MsSqlRepositories.Repositories;
using Lykke.Service.EncryptedFileStorage.Services;
using Lykke.Service.EncryptedFileStorage.Settings;
using Lykke.SettingsReader;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;

namespace Lykke.Service.EncryptedFileStorage.Modules
{
    [UsedImplicitly]
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;

        public ServiceModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            //Services
            builder.RegisterType<EncryptedFileService>()
                .As<IEncryptedFileService>()
                .SingleInstance();

            builder
                .Register(ctx =>
                {
                    var authTokenProvider = new AzureServiceTokenProvider(_appSettings.CurrentValue
                        .EncryptedFileStorageService.KeyVault.KeyVaultConnectionString);
                    var authCallback = new KeyVaultClient.AuthenticationCallback(authTokenProvider.KeyVaultTokenCallback);

                    return new KeyVaultClient(authCallback);
                })
                .As<IKeyVaultClient>()
                .SingleInstance();

            builder
                .Register(ctx =>
                {
                    var account = CloudStorageAccount.Parse(_appSettings.CurrentValue.EncryptedFileStorageService.Db.FilesContentConnString);
                    return account.CreateCloudBlobClient();
                })
                .As<CloudBlobClient>()
                .SingleInstance();

            builder
                .Register(ctx =>
                {
                    return AzureBlobStorage.Create(
                        _appSettings.Nested(x => x.EncryptedFileStorageService.Db.FilesContentConnString),
                        TimeSpan.FromMinutes(15));
                })
                .As<IBlobStorage>()
                .SingleInstance();



            var apiKeysPairs = Environment.GetEnvironmentVariable("EfsApiKeysPairs");
            builder.RegisterInstance(new ApiKeyService(apiKeysPairs))
                .As<IApiKeyService>()
                .SingleInstance();

            //Repositories
            builder.RegisterMsSql(
                _appSettings.CurrentValue.EncryptedFileStorageService.Db.DataConnString,
                connString => new DatabaseContext(connString, false),
                dbConn => new DatabaseContext(dbConn));

            builder.RegisterType<EncryptedFileInfoRepository>()
                .As<IEncryptedFileInfoRepository>()
                .SingleInstance();

            builder.RegisterType<EncryptedFileContentRepository>()
                .As<IEncryptedFileContentRepository>()
                .SingleInstance()
                .WithParameter("fileStorageKey",
                    _appSettings.CurrentValue.EncryptedFileStorageService.KeyVault.FileStorageKey);
        }
    }
}
