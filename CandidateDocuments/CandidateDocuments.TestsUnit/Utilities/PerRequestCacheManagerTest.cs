using System;
using System.Collections.Generic;
using System.Linq;
using CandidateDocuments.API.Core;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace CandidateDocuments.Tests.Unit.Utilities
{
    public class PerRequestCacheManagerTest
    {
        private readonly PerRequestCacheManager _cacheManager;
        private readonly IDictionary<object, object> _cachedItems;

        public PerRequestCacheManagerTest()
        {
            var accessorMock = new Mock<IHttpContextAccessor>();

            _cachedItems = new Dictionary<object, object>
            {
                { "key1", "value1" },
                { "key2", 2 },
                { "key:other", null },
            };
            var contextMock = new Mock<HttpContext>();
            contextMock.SetupGet(m => m.Items).Returns(_cachedItems);
            accessorMock.SetupGet(m => m.HttpContext).Returns(contextMock.Object);

            _cacheManager = new PerRequestCacheManager(accessorMock.Object);
        }

        public class GetMethod : PerRequestCacheManagerTest
        {
            [Fact]
            public void ReturnsCachedObject()
            {
                Assert.Equal(2, _cacheManager.Get<int>("key2"));
            }

            [Fact]
            public void ReturnsEmptyIfObjectNotCached()
            {
                Assert.Equal(Guid.Empty, _cacheManager.Get<Guid>("invalid key"));
            }
        }

        public class SetMethod : PerRequestCacheManagerTest
        {
            [Fact]
            public void AddsObjectToCache()
            {
                const string key = "key3";
                var item = new object();
                _cacheManager.Set(key, item);
                Assert.Equal(item, _cacheManager.Get<object>(key));
            }

            [Fact]
            public void ReplacesCachedObject()
            {
                const string key = "key2";
                var item = new object();
                _cacheManager.Set(key, item);
                Assert.Equal(item, _cacheManager.Get<object>(key));
            }
        }

        public class IsSetMethod : PerRequestCacheManagerTest
        {
            [Fact]
            public void ReturnsTrueIfObjectCached()
            {
                Assert.True(_cacheManager.IsSet("key2"));
            }

            [Fact]
            public void ReturnsFalseIfObjectNotCached()
            {
                Assert.False(_cacheManager.IsSet("invalid key"));
            }
        }

        public class RemoveMethod : PerRequestCacheManagerTest
        {
            [Fact]
            public void RemovesObjectFromCacheByExactKey()
            {
                const string key = "key1";
                _cacheManager.Remove(key);
                Assert.False(_cacheManager.IsSet(key));
            }
        }

        public class RemoveByPatternMetchod : PerRequestCacheManagerTest
        {
            [Theory,
                InlineData(".*\\:.*", new[] { "key:other" }),
                InlineData("key[0-9]+", new[] { "key1", "key2" }),
                InlineData(".*\\:test", new string[] { })]
            public void RemovesObjectsFromCacheByKeyPattern(string pattern, string[] expectedRemovedKeys)
            {
                var initialKeys = new object[_cachedItems.Count];
                _cachedItems.Keys.CopyTo(initialKeys, 0);
                _cacheManager.RemoveByPattern(pattern);
                Assert.True(new HashSet<object>(initialKeys.Except(expectedRemovedKeys)).SetEquals(_cachedItems.Keys));
            }
        }

        public class ClearMethod : PerRequestCacheManagerTest
        {
            [Fact]
            public void RemovesAllObjectsFromCache()
            {
                _cacheManager.Clear();
                Assert.Empty(_cachedItems);
            }
        }
    }
}
