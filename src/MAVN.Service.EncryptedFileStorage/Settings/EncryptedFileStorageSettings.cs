using System.Collections.Generic;
using JetBrains.Annotations;

namespace MAVN.Service.EncryptedFileStorage.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class EncryptedFileStorageSettings
    {
        public DbSettings Db { get; set; }

        public KeyVaultSettings KeyVault { get; set; }
    }
}
