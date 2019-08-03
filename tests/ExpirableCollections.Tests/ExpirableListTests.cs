using System;
using System.Threading;
using Xunit;

namespace ExpirableCollections.Tests
{
    public class ExpirableListTests
    {
        [Fact]
        public void AddItem_ShouldSucceed()
        {
            var list = new ExpirableList<string>(50, TimeSpan.FromSeconds(1))
                { "Test1" };

            Assert.Single(list);
            Thread.Sleep(1100);
            Assert.Empty(list);
        }

        [Fact]
        public void AddMultipleItems_ShouldSucceed()
        {
            var list = new ExpirableList<string>(50, TimeSpan.FromSeconds(1))
                { "Test1" };

            Assert.Single(list);
            Thread.Sleep(1100);
            Assert.Empty(list);
            list.Add("Test2");
            list.Add("Test3");
            Assert.Equal(2, list.Count);
            Thread.Sleep(1100);
            Assert.Empty(list);
        }

        [Fact]
        public void ChangeInterval_ShouldSucceed()
        {
            var list = new ExpirableList<string>(50, TimeSpan.FromSeconds(1))
                { "Test1" };

            list.Interval = 10000;
            list.Add("Test 2");
            Thread.Sleep(1500);
            Assert.Equal(2, list.Count);
        }
    }
}
