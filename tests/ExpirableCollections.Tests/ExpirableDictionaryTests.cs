using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace ExpirableCollections.Tests
{
    public class ExpirableDictionaryTests
    {
        [Fact]
        public void Initialize_ShouldSucceed()
        {
            var demoDict = new Dictionary<string, string>
            {
                ["Test1"] = "Test",
                ["Test2"] = "AlsoTest"
            };
            var dictionary = new ExpirableDictionary<string, string>(50, TimeSpan.FromHours(1), demoDict);

            Assert.NotEmpty(dictionary);
            Assert.Equal(demoDict.Count, dictionary.Count);
            Assert.Equal(demoDict, dictionary.ToDictionary());
        }

        [Fact]
        public void ItemExpired()
        {
            var demoDict = new Dictionary<string, string>
            {
                ["Test1"] = "Test"
            };
            var dictionary = new ExpirableDictionary<string, string>(50, TimeSpan.FromMilliseconds(500), demoDict);
            Assert.NotEmpty(dictionary);
            Thread.Sleep(700);
            Assert.Empty(dictionary);
        }
    }
}
