using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using NUnit.Framework;

namespace Neurotoxin.Godspeed.Core.Tests
{
    [TestFixture]
    public class CachingTests
    {
        private string RandomString(int min, int max)
        {
            var r = new Random();
            var n = r.Next(min, max);
            var sb = new StringBuilder();
            for (var i = 0; i < n; i++)
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * r.NextDouble() + 65)));
                sb.Append(ch);
            }
            return sb.ToString();
        }

        [Test]
        public void DeserializeTest()
        {
            //var store = new EsentPersistentDictionary(@"c:\merc");
            //var sw = new Stopwatch();
            //sw.Start();
            //for (var i = 0; i < 100; i++)
            //{
            //    var item = new CacheEntry<string>
            //        {
            //            Content = RandomString(1000, 2000),
            //            Date = DateTime.Now,
            //            Expiration = DateTime.Now,
            //            Size = long.MaxValue,
            //            TempFilePath = RandomString(10, 100)
            //        };
            //    store.Put(i.ToString(CultureInfo.InvariantCulture), item);
            //}
            //store.Flush();
            //sw.Stop();
            //Debug.WriteLine(sw.Elapsed);
            //sw.Reset();
            //sw.Start();
            //for (var i = 0; i < 100; i++)
            //{
            //    var item = store.Get<CacheEntry<string>>(i.ToString(CultureInfo.InvariantCulture));
            //}
            //sw.Stop();
            //Debug.WriteLine(sw.Elapsed);
        }

    }
}