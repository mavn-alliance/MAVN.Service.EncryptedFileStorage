using System;
using Autofac;
using AzureStorage;
using AzureStorage.Blob;
using JetBrains.Annotations;
using MAVN.Common.MsSql;
using MAVN.Service.EncryptedFileStorage.Auth;
using MAVN.Service.EncryptedFileStorage.AzureRepositories;
using MAVN.Service.EncryptedFileStorage.Client;
using MAVN.Service.EncryptedFileStorage.Domain.Repositories;
using MAVN.Service.EncryptedFileStorage.Domain.Services;
using MAVN.Service.EncryptedFileStorage.DomainServices;
using MAVN.Service.EncryptedFileStorage.MsSqlRepositories;
using MAVN.Service.EncryptedFileStorage.MsSqlRepositories.Repositories;
using MAVN.Service.EncryptedFileStorage.Services;
using MAVN.Service.EncryptedFileStorage.Settings;
using Lykke.SettingsReader;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;

namespace MAVN.Service.EncryptedFileStorage.Modules
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
