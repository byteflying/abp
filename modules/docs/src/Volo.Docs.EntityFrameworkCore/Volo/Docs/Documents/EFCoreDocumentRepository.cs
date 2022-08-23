﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Docs.Documents.Filter;
using Volo.Docs.EntityFrameworkCore;

namespace Volo.Docs.Documents
{
    public class EFCoreDocumentRepository : EfCoreRepository<IDocsDbContext, Document>, IDocumentRepository
    {
        public EFCoreDocumentRepository(IDbContextProvider<IDocsDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<List<DocumentWithoutDetails>> GetListWithoutDetailsByProjectId(Guid projectId, CancellationToken cancellationToken = default)
        {
            return await (await GetDbSetAsync())
                .Where(d => d.ProjectId == projectId)
                .Select(x => new DocumentWithoutDetails
                {
                    Id = x.Id,
                    Version = x.Version,
                    LanguageCode = x.LanguageCode,
                    Format = x.Format,
                })
                .ToListAsync(GetCancellationToken(cancellationToken));
        }

        public async Task<List<Document>> GetListByProjectId(Guid projectId,
            CancellationToken cancellationToken = default)
        {
            return await (await GetDbSetAsync()).Where(d => d.ProjectId == projectId).ToListAsync(GetCancellationToken(cancellationToken));
        }

        public async Task<List<Document>> GetListAsync(Guid? projectId, string version, string name, CancellationToken cancellationToken = default)
        {
            return await (await GetDbSetAsync())
                .WhereIf(version != null, x => x.Version == version)
                .WhereIf(name != null, x => x.Name == name)
                .WhereIf(projectId.HasValue, x => x.ProjectId == projectId)
                .ToListAsync(GetCancellationToken(cancellationToken));
        }

        public async Task<List<DocumentWithoutContent>> GetAllAsync(
            Guid? projectId,
            string name,
            string version,
            string languageCode,
            string fileName,
            string format,
            DateTime? creationTimeMin,
            DateTime? creationTimeMax,
            DateTime? lastUpdatedTimeMin,
            DateTime? lastUpdatedTimeMax,
            DateTime? lastSignificantUpdateTimeMin,
            DateTime? lastSignificantUpdateTimeMax,
            DateTime? lastCachedTimeMin,
            DateTime? lastCachedTimeMax,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query =await ApplyFilterForGetAll(
                await GetDbSetAsync(),
                projectId: projectId,
                name: name,
                version: version,
                languageCode: languageCode,
                fileName: fileName,
                format: format,
                creationTimeMin: creationTimeMin,
                creationTimeMax: creationTimeMax,
                lastUpdatedTimeMin: lastUpdatedTimeMin,
                lastUpdatedTimeMax: lastUpdatedTimeMax,
                lastSignificantUpdateTimeMin: lastSignificantUpdateTimeMin,
                lastSignificantUpdateTimeMax: lastSignificantUpdateTimeMax,
                lastCachedTimeMin: lastCachedTimeMin, lastCachedTimeMax: lastCachedTimeMax);

            query = query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? nameof(Document.Name) : sorting);
            return await query.PageBy(skipCount, maxResultCount).ToListAsync(GetCancellationToken(cancellationToken));
        }

        public async Task<long> GetAllCountAsync(
            Guid? projectId,
            string name,
            string version,
            string languageCode,
            string fileName,
            string format,
            DateTime? creationTimeMin,
            DateTime? creationTimeMax,
            DateTime? lastUpdatedTimeMin,
            DateTime? lastUpdatedTimeMax,
            DateTime? lastSignificantUpdateTimeMin,
            DateTime? lastSignificantUpdateTimeMax,
            DateTime? lastCachedTimeMin,
            DateTime? lastCachedTimeMax,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query = await ApplyFilterForGetAll(
                await GetDbSetAsync(),
                projectId: projectId,
                name: name,
                version: version,
                languageCode: languageCode,
                fileName: fileName,
                format: format,
                creationTimeMin: creationTimeMin,
                creationTimeMax: creationTimeMax,
                lastUpdatedTimeMin: lastUpdatedTimeMin,
                lastUpdatedTimeMax: lastUpdatedTimeMax,
                lastSignificantUpdateTimeMin: lastSignificantUpdateTimeMin,
                lastSignificantUpdateTimeMax: lastSignificantUpdateTimeMax,
                lastCachedTimeMin: lastCachedTimeMin, lastCachedTimeMax: lastCachedTimeMax);

            return await query.LongCountAsync(GetCancellationToken(cancellationToken));
        }

        public async Task<Document> FindAsync(Guid projectId, string name, string languageCode, string version,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            return await (await GetDbSetAsync()).IncludeDetails(includeDetails)
                .FirstOrDefaultAsync(x =>
                    x.ProjectId == projectId && x.Name == name && x.LanguageCode == languageCode &&
                    x.Version == version,
                GetCancellationToken(cancellationToken));
        }

        public async Task DeleteAsync(Guid projectId, string name, string languageCode, string version, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            await DeleteAsync(x =>
                x.ProjectId == projectId && x.Name == name && x.LanguageCode == languageCode &&
                x.Version == version, autoSave, cancellationToken: cancellationToken);
        }

        public async Task<Document> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await (await GetDbSetAsync()).Where(x => x.Id == id).SingleAsync(cancellationToken: GetCancellationToken(cancellationToken));
        }

        protected virtual async Task<IQueryable<DocumentWithoutContent>> ApplyFilterForGetAll(
            IQueryable<Document> query,
            Guid? projectId,
            string name,
            string version,
            string languageCode,
            string fileName,
            string format,
            DateTime? creationTimeMin,
            DateTime? creationTimeMax,
            DateTime? lastUpdatedTimeMin,
            DateTime? lastUpdatedTimeMax,
            DateTime? lastSignificantUpdateTimeMin,
            DateTime? lastSignificantUpdateTimeMax,
            DateTime? lastCachedTimeMin,
            DateTime? lastCachedTimeMax,
            CancellationToken cancellationToken = default)
        {
            return query
                .WhereIf(projectId.HasValue,
                    d => d.ProjectId == projectId.Value)
                .WhereIf(name != null,
                    d => d.Name != null && d.Name.Contains(name))
                .WhereIf(version != null,
                    d => d.Version != null && d.Version == version)
                .WhereIf(languageCode != null,
                    d => d.LanguageCode != null && d.LanguageCode == languageCode)
                .WhereIf(fileName != null,
                    d => d.FileName != null && d.FileName.Contains(fileName))
                .WhereIf(format != null,
                    d => d.Format != null && d.Format == format)
                .WhereIf(creationTimeMin.HasValue,
                    d => d.CreationTime.Date >= creationTimeMin.Value.Date)
                .WhereIf(creationTimeMax.HasValue,
                    d => d.CreationTime.Date <= creationTimeMax.Value.Date)
                .WhereIf(lastUpdatedTimeMin.HasValue,
                    d => d.LastUpdatedTime.Date >= lastUpdatedTimeMin.Value.Date)
                .WhereIf(lastUpdatedTimeMax.HasValue,
                    d => d.LastUpdatedTime.Date <= lastUpdatedTimeMax.Value.Date)
                .WhereIf(lastSignificantUpdateTimeMin.HasValue,
                    d => d.LastSignificantUpdateTime != null && d.LastSignificantUpdateTime.Value.Date >= lastSignificantUpdateTimeMin.Value.Date)
                .WhereIf(lastSignificantUpdateTimeMax.HasValue,
                    d => d.LastSignificantUpdateTime != null && d.LastSignificantUpdateTime.Value.Date <= lastSignificantUpdateTimeMax.Value.Date)
                .WhereIf(lastCachedTimeMin.HasValue,
                    d => d.LastCachedTime.Date >= lastCachedTimeMin.Value.Date)
                .WhereIf(lastCachedTimeMax.HasValue,
                    d => d.LastCachedTime.Date <= lastCachedTimeMax.Value.Date)
                .Join( (await GetDbContextAsync()).Projects,
                    d => d.ProjectId,
                    p => p.Id,
                    (d, p) => new { d, p })
                .Select(x => new DocumentWithoutContent
                {
                    Id = x.d.Id,
                    ProjectId = x.d.ProjectId,
                    ProjectName = x.p.Name,
                    Name = x.d.Name,
                    Version = x.d.Version,
                    LanguageCode = x.d.LanguageCode,
                    FileName = x.d.FileName,
                    Format = x.d.Format,
                    CreationTime = x.d.CreationTime,
                    LastUpdatedTime = x.d.LastUpdatedTime,
                    LastSignificantUpdateTime = x.d.LastSignificantUpdateTime,
                    LastCachedTime = x.d.LastCachedTime
                });
        }
        public async Task<FilterItems> GetFilterItemsAsync(CancellationToken cancellationToken = default)
        {
            var filterItems = new FilterItems();
            
            filterItems.Formats = await GetFormats(cancellationToken);

            filterItems.Projects = await GetFilterProjectItems(cancellationToken);

            filterItems.Versions = await GetFilterVersionItems(cancellationToken);

            filterItems.Languages = await GetFilterLanguageCodeItems(cancellationToken);
            
            return filterItems;
        }

        private async Task<List<string>> GetFormats(CancellationToken cancellationToken)
        {
            return await (await GetDbSetAsync()).Select(x => x.Format).Distinct().ToListAsync(cancellationToken);
        }

        private async Task<List<FilterProjectItem>> GetFilterProjectItems(CancellationToken cancellationToken)
        {
            return await (await GetDbContextAsync())
                .Projects
                .Select(x=>new FilterProjectItem
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .OrderBy(x=>x.Name)
                .ToListAsync(GetCancellationToken(cancellationToken));
        }

        private async Task<List<FilterLanguageCodeItem>> GetFilterLanguageCodeItems(CancellationToken cancellationToken)
        {
            return await (await GetDbSetAsync())
                .GroupBy(x => x.LanguageCode)
                .Select(x => new FilterLanguageCodeItem 
                {
                    ProjectIds = x.Select(x2 => x2.ProjectId).Distinct(),
                    Code = x.Key,
                    Versions = x.Select(x2 => x2.Version).Distinct()
                })
                .OrderBy(x=>x.Code)
                .ToListAsync(GetCancellationToken(cancellationToken));
        }

        private async Task<List<FilterVersionItem>> GetFilterVersionItems(CancellationToken cancellationToken)
        {
            return await (await GetDbSetAsync())
                .GroupBy(x => x.Version)
                .Select(x => new FilterVersionItem 
                {
                    ProjectIds = x.Select(x2 => x2.ProjectId).Distinct(),
                    Version = x.Key,
                    Languages = x.Select(x2 => x2.LanguageCode).Distinct()
                })
                .OrderByDescending(x => x.Version)
                .ToListAsync(GetCancellationToken(cancellationToken));
        }
    }
}
