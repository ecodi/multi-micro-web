using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CandidateDocuments.Application.Core;
using Microsoft.AspNetCore.Http;

namespace CandidateDocuments.API.Core
{
    /// <summary>
    /// Manages request-scoped cache.
    /// </summary>
    public class PerRequestCacheManager : ICacheManager
    {
        private readonly IHttpContextAccessor _accessor;
        private HttpContext Context => _accessor.HttpContext;

        public PerRequestCacheManager(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        protected IDictionary<object, object> GetItems()
        {
            return Context?.Items;
        }

        public T Get<T>(string key)
        {
            var items = GetItems();
            if (items == null || !items.ContainsKey(key)) return default(T);
            return (T)items[key];
        }

        public void Set(string key, object data, TimeSpan? cacheTime = null)
        {
            var items = GetItems();
            if (items == null) return;
            items[key] = data;
        }

        public bool IsSet(string key)
        {
            var items = GetItems();
            return items != null && items.ContainsKey(key);
        }

        public void Remove(string key)
        {
            var items = GetItems();
            items?.Remove(key);
        }

        public void RemoveByPattern(string pattern)
        {
            var items = GetItems();
            if (items == null) return;

            var regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            foreach (var key in items.Keys.Where(k => regex.IsMatch(k.ToString())).ToList())
                items.Remove(key);
        }

        public void Clear()
        {
            var items = GetItems();
            items?.Clear();
        }
    }
}
