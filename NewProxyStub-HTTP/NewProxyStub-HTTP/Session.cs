using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using StackExchange.Redis;
using System.Text.Json;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Google.Protobuf.WellKnownTypes;

namespace NewProxyStub_HTTP
{
    public struct Session
    {
        public string userId { get; set; }
        public string username { get; set; }
    }
    public class RandomStringMaker
    {
        static string randomPool = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVMXIZ1234567890";
        static Random random = new Random();
        public static string GenerateSessionID()
        {
            string randomString = "";
            while (randomString.Length < 10)
            {
                int idx = random.Next(0, randomPool.Length);
                randomString += randomPool[idx];
            }
            return randomString;
        }
    }
    public class SessionManager
    {
        static ConnectionMultiplexer _redis;
        static TimeSpan _timeout = new TimeSpan(0, 0, 30);
        public static ConnectionMultiplexer GetRedis()
        {
            if (_redis != null && _redis.IsConnected == true)
                return _redis;

            ConfigurationOptions co = new ConfigurationOptions()
            {
                SyncTimeout = 500000,
                EndPoints =
                {
                    {"localhost", 6379}
                },
                AbortOnConnectFail = false // this prevents that error
            };
            _redis = ConnectionMultiplexer.Connect(co);
            return _redis;
        }
        public static void PutSessionKey(Session session)
        {
            IDatabase db = GetRedis().GetDatabase();
            string sessionId = RandomStringMaker.GenerateSessionID();

            db.StringSetAsync(sessionId, new RedisValue(JsonSerializer.Serialize<Session>(session)));
            db.KeyExpireAsync(sessionId, _timeout);
        }
        public static async Task<string> PutAndGetSessionIdAsync(Session session)
        {
            IDatabase db = GetRedis().GetDatabase();
            string sessionId = RandomStringMaker.GenerateSessionID();
            await db.StringSetAsync(sessionId, new RedisValue(JsonSerializer.Serialize<Session>(session)));
            await db.KeyExpireAsync(sessionId, _timeout);
            return sessionId;
        }
        public static async Task<Session> GetSessionAsync(string sessionId = "")
        {
            IDatabase db = GetRedis().GetDatabase();
            var value = await db.StringGetAsync(sessionId);
        }
        public static Session GetSession(string sessionId = "")
        {
            IDatabase db = GetRedis().GetDatabase();
            var value = db.StringGet(sessionId);
            return value;
        }
    }
}


