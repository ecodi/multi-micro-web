using System;
using System.Threading.Tasks;

namespace CandidateDocuments.Application.Core
{
    public interface ICacheManager
    {
        /// <summary>
        /// Gets value associated with the specified key.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">The key of the value to get</param>
        /// <returns>Cached object associated with the specified key</returns>
        T Get<T>(string key);

        /// <summary>
        /// Sets object to the cache under specified key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="data">Data</param>
        /// <param name="cacheTime">Cache time</param>
        void Set(string key, object data, TimeSpan? cacheTime = null);

        /// <summary>
        /// Checks if object associated with the specified key is cached.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Result</returns>
        bool IsSet(string key);

        /// <summary>
        /// Removes object with the specified key from cache.
        /// </summary>
        /// <param name="key">Key</param>
        void Remove(string key);

        /// <summary>
        /// Removes object with keys matched by pattern.
        /// </summary>
        /// <param name="pattern">Pattern</param>
        void RemoveByPattern(string pattern);

        /// <summary>
        /// Removes all cached object.
        /// </summary>
        void Clear();
    }

    public static class CacheExtensions
    {
        /// <summary>
        /// Gets value associated with the specified key if cached, otherwise executes function and caches result.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="cacheManager"></param>
        /// <param name="key">The key of the value to get/set.</param>
        /// <param name="acquire">Function executed if object not cached.</param>
        /// <returns>Cached/acquired object associated with the specified key.</returns>
        public static T Get<T>(this ICacheManager cacheManager, string key, Func<T> acquire)
        {
            return Get(cacheManager, key, TimeSpan.FromHours(1), acquire);
        }

        /// <summary>
        /// Gets value associated with the specified key if cached, otherwise executes function and caches result.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="cacheManager"></param>
        /// <param name="key">The key of the value to get/set.</param>
        /// <param name="cacheTime">Cache time</param>
        /// <param name="acquire">Function executed if object not cached.</param>
        /// <returns>Cached/acquired object associated with the specified key.</returns>
        public static T Get<T>(this ICacheManager cacheManager, string key, TimeSpan? cacheTime, Func<T> acquire)
        {
            if (cacheManager.IsSet(key)) return cacheManager.Get<T>(key);

            var result = acquire();
            cacheManager.Set(key, result, cacheTime);
            return result;
        }

        /// <summary>
        /// Gets value associated with the specified key if cached, otherwise executes function and caches result.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="cacheManager"></param>
        /// <param name="key">The key of the value to get/set.</param>
        /// <param name="acquire">Function executed if object not cached.</param>
        /// <returns>Cached/acquired object associated with the specified key.</returns>
        public static async Task<T> GetAsync<T>(this ICacheManager cacheManager, string key, Func<Task<T>> acquire)
        {
            return await GetAsync(cacheManager, key, TimeSpan.FromHours(1), acquire);
        }

        /// <summary>
        /// Gets value associated with the specified key if cached, otherwise executes function and caches result.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="cacheManager"></param>
        /// <param name="key">The key of the value to get/set.</param>
        /// <param name="cacheTime">Cache time</param>
        /// <param name="acquire">Function executed if object not cached.</param>
        /// <returns>Cached/acquired object associated with the specified key.</returns>
        public static async Task<T> GetAsync<T>(this ICacheManager cacheManager, string key, TimeSpan? cacheTime, Func<Task<T>> acquire)
        {
            if (cacheManager.IsSet(key)) return cacheManager.Get<T>(key);

            var result = await acquire();
            cacheManager.Set(key, result, cacheTime);
            return result;
        }
    }
}
