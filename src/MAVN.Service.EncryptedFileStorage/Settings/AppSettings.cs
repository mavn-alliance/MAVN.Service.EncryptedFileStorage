using JetBrains.Annotations;
using Lykke.Sdk.Settings;

namespace MAVN.Service.EncryptedFileStorage.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public EncryptedFileStorageSettings EncryptedFileStorageService { get; set; }
    }
}
