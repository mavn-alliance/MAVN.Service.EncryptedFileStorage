using System.Data.Common;
using JetBrains.Annotations;
using MAVN.Persistence.PostgreSQL.Legacy;
using MAVN.Service.EncryptedFileStorage.Domain.Models;
using MAVN.Service.EncryptedFileStorage.MsSqlRepositories.EntityConfigurations;
using Microsoft.EntityFrameworkCore;

namespace MAVN.Service.EncryptedFileStorage.MsSqlRepositories
{
    public class DatabaseContext : PostgreSQLContext
    {
        private const string Schema = "encrypted_files_storage";

        public DbSet<EncryptedFile> EncryptedFiles { get; set; }

        // empty constructor needed for migrations
        [UsedImplicitly]
        public DatabaseContext() : base(Schema)
        {
        }

        public DatabaseContext(int commandTimeoutSeconds = 30) : base(Schema, commandTimeoutSeconds)
        {
        }

        public DatabaseContext(string connectionString, bool isTraceEnabled, int commandTimeoutSeconds = 30) : base(
            Schema, connectionString, isTraceEnabled, commandTimeoutSeconds)
        {
        }

        public DatabaseContext(DbContextOptions contextOptions) : base(Schema, contextOptions)
        {
        }

        public DatabaseContext(DbContextOptions options, bool isForMocks = false, int commandTimeoutSeconds = 30) :
            base(Schema, options, isForMocks, commandTimeoutSeconds)
        {
        }

        public DatabaseContext(DbConnection dbConnection, bool isForMocks = false,
            int commandTimeoutSeconds = 30) : base(Schema, dbConnection, isForMocks, commandTimeoutSeconds)
        {
        }

        protected override void OnMAVNModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new EncryptedFileConfiguration());
        }
    }
}
