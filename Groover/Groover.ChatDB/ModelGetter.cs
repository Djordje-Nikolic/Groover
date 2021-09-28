using Cassandra;
using Cassandra.Mapping;
using Groover.ChatDB.Interfaces;
using Groover.ChatDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.ChatDB
{
    internal class ModelGetter<T> : IModelGetter<T> where T : BaseCassandraModel
    {
        public readonly IMapper _mapper;

        public ModelGetter(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<ICollection<T>> GetAsync(object columnValue, string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentNullException(nameof(columnName));

            if (columnValue == null)
                throw new ArgumentNullException(nameof(columnValue));

            var results = await _mapper.FetchAsync<T>($"WHERE {columnName} = ?", columnValue);
            return results.ToList();
        }

        public async Task<ICollection<T>> GetAsync(object columnValue, string columnName, PageParams pageParams)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentNullException(nameof(columnName));

            if (columnValue == null)
                throw new ArgumentNullException(nameof(columnValue));

            if (pageParams == null)
                throw new ArgumentNullException(nameof(pageParams));

            if (pageParams.PageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageParams.PageSize));

            byte[] pagingStateBytes = PageParams.ConvertPagingState(pageParams.PagingState);

            IPage<T> page = await _mapper.FetchPageAsync<T>(Cql.New($"WHERE {columnName} = ?", columnValue)
                                                .WithOptions(options =>
                                                {
                                                    options.SetPageSize(pageParams.PageSize);
                                                    options.SetPagingState(pagingStateBytes);
                                                }));
            ICollection<T> results = page.ToList();

            pageParams.NextPagingState = PageParams.ConvertPagingState(page.PagingState);
            return results;
        }

        public async Task<ICollection<T>> GetAfterAsync(object columnValue, string columnName, DateTime afterDateTime, string timeUuidColumnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentNullException(nameof(columnName));

            if (columnValue == null)
                throw new ArgumentNullException(nameof(columnValue));

            if (string.IsNullOrWhiteSpace(timeUuidColumnName))
                throw new ArgumentNullException(nameof(timeUuidColumnName));

            TimeUuid afterTimeUuid = TimeUuid.Min(afterDateTime);

            var results = await _mapper.FetchAsync<T>($"WHERE {columnName} = ? AND {timeUuidColumnName} > ?", columnValue, afterTimeUuid);
            return results.ToList();
        }

        public async Task<ICollection<T>> GetAfterAsync(object columnValue, string columnName, DateTime afterDateTime, string timeUuidColumnName, PageParams pageParams)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentNullException(nameof(columnName));

            if (columnValue == null)
                throw new ArgumentNullException(nameof(columnValue));

            if (pageParams == null)
                throw new ArgumentNullException(nameof(pageParams));

            if (pageParams.PageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageParams.PageSize));

            if (string.IsNullOrWhiteSpace(timeUuidColumnName))
                throw new ArgumentNullException(nameof(timeUuidColumnName));

            TimeUuid afterTimeUuid = TimeUuid.Min(afterDateTime);

            byte[] pagingStateBytes = PageParams.ConvertPagingState(pageParams.PagingState);

            IPage<T> page = await _mapper.FetchPageAsync<T>(Cql.New($"WHERE {columnName} = ? AND {timeUuidColumnName} > ?", columnValue, afterTimeUuid)
                                    .WithOptions(options =>
                                    {
                                        options.SetPageSize(pageParams.PageSize);
                                        options.SetPagingState(pagingStateBytes);
                                    }));
            ICollection<T> results = page.ToList();

            pageParams.NextPagingState = PageParams.ConvertPagingState(page.PagingState);
            return results.ToList();
        }
    }
}
