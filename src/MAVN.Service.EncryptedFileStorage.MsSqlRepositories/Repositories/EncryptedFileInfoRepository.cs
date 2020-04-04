using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Common.MsSql;
using MAVN.Service.EncryptedFileStorage.Domain.Models;
using MAVN.Service.EncryptedFileStorage.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MAVN.Service.EncryptedFileStorage.MsSqlRepositories.Repositories
{
    public class EncryptedFileInfoRepository : IEncryptedFileInfoRepository
    {
        private readonly MsSqlContextFactory<DatabaseContext> _contextFactory;

        public EncryptedFileInfoRepository(MsSqlContextFactory<DatabaseContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Guid> CreateFileInfoAsync(EncryptedFile data)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                context.EncryptedFiles.Add(data);

                await context.SaveChangesAsync();

                return data.FileId;
            }
        }

        public async Task<bool> DeleteFileInfoAsync(Guid fileId)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var entity = await context.EncryptedFiles.FirstOrDefaultAsync(x => x.FileId == fileId);

                if (entity == null)
                    return false;

                context.EncryptedFiles.Remove(entity);

                await context.SaveChangesAsync();

                return true;
            }
        }

        public async Task<bool> UpdateFileInfoAsync(EncryptedFile data)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var entity = await context.EncryptedFiles.AsNoTracking().FirstOrDefaultAsync(x => x.FileId == data.FileId);

                if (entity == null)
                    throw new NullReferenceException($"File info for file Id '{data.FileId}' cannot be found.");

                context.EncryptedFiles.Update(data);

                await context.SaveChangesAsync();

                return true;
            }
        }

        public async Task<EncryptedFile> GetFileInfoAsync(Guid fileId)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var result = await context.EncryptedFiles.FirstOrDefaultAsync(x => x.FileId == fileId);

                return result;
            }
        }

        public async Task<IEnumerable<EncryptedFile>> GetPaginatedFilesInfoAsync(int skip, int take)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var result = await context.EncryptedFiles
                    .OrderBy(x => x.FileName)
                    .Skip(skip)
                    .Take(take)
                    .ToArrayAsync();

                return result;
            }
        }

        public async Task<int> GetTotalAsync()
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                return await context.EncryptedFiles.CountAsync();
            }
        }

        public async Task<IEnumerable<EncryptedFile>> GetPaginatedFilesInfoByOriginAsync(int skip, int take,
            string origin)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var query = context.EncryptedFiles.AsQueryable();

                if (!string.IsNullOrEmpty(origin))
                {
                    query = query.Where(x => x.Origin == origin);
                }

                var result = await query
                    .OrderBy(x => x.FileName)
                    .Skip(skip)
                    .Take(take)
                    .ToArrayAsync();

                return result;
            }
        }

        public async Task<bool> FileInfoByOriginAndFileNameExistsAsync(string origin, string fileName)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                return await context.EncryptedFiles.AnyAsync(x => x.Origin == origin && x.FileName == fileName);
            }
        }

        public async Task<int> GetTotalByOriginAsync(string origin)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var query = context.EncryptedFiles.AsQueryable();

                if (!string.IsNullOrEmpty(origin))
                {
                    query = query.Where(x => x.Origin == origin);
                }

                return await query.CountAsync();
            }
        }
    }
}
