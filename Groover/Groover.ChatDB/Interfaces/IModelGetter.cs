using Groover.ChatDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.ChatDB.Interfaces
{
    public interface IModelGetter<T> where T : BaseCassandraModel
    {
        public Task<ICollection<T>> GetAsync(object columnValue, string columnName);
        public Task<ICollection<T>> GetAsync(object columnValue, string columnName, PageParams pageParams);
        public Task<ICollection<T>> GetAfterAsync(object columnValue, string columnName, DateTime afterDateTime, string timeUuidColumnName);
        public Task<ICollection<T>> GetAfterAsync(object columnValue, string columnName, DateTime afterDateTime, string timeUuidColumnName, PageParams pageParams);
    }
}
