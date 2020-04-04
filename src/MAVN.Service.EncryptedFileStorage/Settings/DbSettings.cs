using Lykke.SettingsReader.Attributes;

namespace MAVN.Service.EncryptedFileStorage.Settings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }

        public string DataConnString { get; set; }

        [AzureTableCheck]
        public string FilesContentConnString { get; set; }
    }
}
