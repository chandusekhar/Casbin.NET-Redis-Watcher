using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Redis.Casbin.NET;
using StackExchange.Redis;

namespace Casbin.NET.Watcher.Redis.UnitTests
{
    [TestClass]
    public class RedisWatcherTests : RedisTestBase
    {
        public IConnectionMultiplexer GetConnection(SubscriptionType subscriptionType)
        {
#if TRUEREDIS
            return ConnectionMultiplexer.Connect("localhost:6379");
#else
            return GetMockedConnection(subscriptionType);
#endif
        }

        [TestMethod]
        public void BadConnectionStringTest()
        {
            var connectionString = "unkonwnhost:6379";

            Assert.ThrowsException<StackExchange.Redis.RedisConnectionException>(() => new RedisWatcher(connectionString));
        }

        [TestMethod]
        public void NominalTest()
        {
            var callback = new TaskCompletionSource<int>();
            
            var watcher = new RedisWatcher(GetConnection(SubscriptionType.Subscriber));
            watcher.SetUpdateCallback(() => callback.TrySetResult(1));

            var watcher2 = new RedisWatcher(GetConnection(SubscriptionType.Publisher));
            watcher2.Update();

            Assert.IsTrue(callback.Task.Wait(300), "The first watcher didn't receive the notification");
        }

        [TestMethod]
        public void IgnoreSelfMessageTest()
        {
            var callback = new TaskCompletionSource<int>();

            var watcher = new RedisWatcher(GetConnection(SubscriptionType.Both));
            watcher.SetUpdateCallback(() => callback.TrySetResult(1));

            watcher.Update();

            Assert.IsFalse(callback.Task.Wait(500), "The watcher shouldn't receive its self messages");
        }
    }
}
