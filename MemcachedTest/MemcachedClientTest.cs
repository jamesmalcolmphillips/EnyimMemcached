using System;
using System.Net;
using System.Threading;
using Enyim.Caching;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using NUnit.Framework;
using System.Collections.Generic;
using System.Text;

namespace MemcachedTest
{
	public abstract class MemcachedClientTest
	{
		public const string TestObjectKey = "Hello_World";

		protected abstract MemcachedClient GetClient();

		[global::System.Serializable]
		public class TestData
		{
			public TestData() { }

			public string FieldA;
			public string FieldB;
			public int FieldC;
			public bool FieldD;
		}

		/// <summary>
		///A test for Store (StoreMode, string, byte[], int, int)
		///</summary>
		[TestCase]
		public void StoreObjectTest()
		{
			TestData td = new TestData();
			td.FieldA = "Hello";
			td.FieldB = "World";
			td.FieldC = 19810619;
			td.FieldD = true;

			using (MemcachedClient client = GetClient())
			{
				Assert.IsTrue(client.Store(StoreMode.Set, TestObjectKey, td));
			}
		}

		[TestCase]
		public void GetObjectTest()
		{
			TestData td = new TestData();
			td.FieldA = "Hello";
			td.FieldB = "World";
			td.FieldC = 19810619;
			td.FieldD = true;

			using (MemcachedClient client = GetClient())
			{
				Assert.IsTrue(client.Store(StoreMode.Set, TestObjectKey, td), "Initialization failed.");

				TestData td2 = client.Get<TestData>(TestObjectKey);

				Assert.IsNotNull(td2, "Get returned null.");
				Assert.AreEqual(td2.FieldA, "Hello", "Object was corrupted.");
				Assert.AreEqual(td2.FieldB, "World", "Object was corrupted.");
				Assert.AreEqual(td2.FieldC, 19810619, "Object was corrupted.");
				Assert.AreEqual(td2.FieldD, true, "Object was corrupted.");
			}
		}

		[TestCase]
		public void DeleteObjectTest()
		{
			using (MemcachedClient client = GetClient())
			{
				TestData td = new TestData();
				Assert.IsTrue(client.Store(StoreMode.Set, TestObjectKey, td), "Initialization failed.");

				Assert.IsTrue(client.Remove(TestObjectKey), "Remove failed.");
				Assert.IsNull(client.Get(TestObjectKey), "Remove failed.");
			}
		}

		[TestCase]
		public void StoreStringTest()
		{
			using (MemcachedClient client = GetClient())
			{
				Assert.IsTrue(client.Store(StoreMode.Set, "TestString", "Hello world!"), "StoreString failed.");

				Assert.AreEqual("Hello world!", client.Get<string>("TestString"));
			}
		}

		[TestCase]
		public void StoreLongTest()
		{
			using (MemcachedClient client = GetClient())
			{
				Assert.IsTrue(client.Store(StoreMode.Set, "TestLong", 65432123456L), "StoreString long.");

				Assert.AreEqual(65432123456L, client.Get<long>("TestLong"));
			}
		}

		[TestCase]
		public void StoreArrayTest()
		{
			byte[] bigBuffer = new byte[200 * 1024];

			for (int i = 0; i < bigBuffer.Length / 256; i++)
			{
				for (int j = 0; j < 256; j++)
				{
					bigBuffer[i * 256 + j] = (byte)j;
				}
			}

			using (MemcachedClient client = GetClient())
			{
				Assert.IsTrue(client.Store(StoreMode.Set, "BigBuffer", bigBuffer), "StoreArray failed");

				byte[] bigBuffer2 = client.Get<byte[]>("BigBuffer");

				for (int i = 0; i < bigBuffer.Length / 256; i++)
				{
					for (int j = 0; j < 256; j++)
					{
						if (bigBuffer2[i * 256 + j] != (byte)j)
						{
							Assert.AreEqual(j, bigBuffer[i * 256 + j], "Data should be {0} but its {1}");
							break;
						}
					}
				}
			}
		}

		[TestCase]
		public void ExpirationTestTimeSpan()
		{
			using (MemcachedClient client = GetClient())
			{
				Assert.IsTrue(client.Store(StoreMode.Set, "ExpirationTest:TimeSpan", "ExpirationTest:TimeSpan", new TimeSpan(0, 0, 5)), "Expires:Timespan failed");
				Assert.AreEqual("ExpirationTest:TimeSpan", client.Get("ExpirationTest:TimeSpan"), "Expires:Timespan store failed");

				Thread.Sleep(8000);
				Assert.IsNull(client.Get("ExpirationTest:TimeSpan"), "ExpirationTest:TimeSpan item did not expire");
			}
		}

		[TestCase]
		public void ExpirationTestDateTime()
		{
			using (MemcachedClient client = GetClient())
			{
				DateTime expiresAt = DateTime.Now.AddSeconds(5);

				Assert.IsTrue(client.Store(StoreMode.Set, "Expires:DateTime", "Expires:DateTime", expiresAt), "Expires:DateTime failed");
				Assert.AreEqual("Expires:DateTime", client.Get("Expires:DateTime"), "Expires:DateTime store failed");

				Thread.Sleep(8000);

				Assert.IsNull(client.Get("Expires:DateTime"), "Expires:DateTime item did not expire");
			}
		}

		[TestCase]
		public void AddSetReplaceTest()
		{
			using (MemcachedClient client = GetClient())
			{
				Assert.IsTrue(client.Store(StoreMode.Set, "VALUE", "1"), "Initialization failed");

				Assert.AreEqual("1", client.Get("VALUE"), "Store failed");

				Assert.IsFalse(client.Store(StoreMode.Add, "VALUE", "2"), "Add should have failed");
				Assert.AreEqual("1", client.Get("VALUE"), "Item should not have been Added");

				Assert.IsTrue(client.Store(StoreMode.Replace, "VALUE", "4"), "Replace failed");
				Assert.AreEqual("4", client.Get("VALUE"), "Item should have been replaced");

				Assert.IsTrue(client.Remove("VALUE"), "Remove failed");

				Assert.IsFalse(client.Store(StoreMode.Replace, "VALUE", "8"), "Replace should not have succeeded");
				Assert.IsNull(client.Get("VALUE"), "Item should not have been Replaced");

				Assert.IsTrue(client.Store(StoreMode.Add, "VALUE", "16"), "Item should have been Added");
				Assert.AreEqual("16", client.Get("VALUE"), "Add failed");
			}
		}

		class NonSerializableObject
		{
			public string Value;
		}

		[TestCase]
		public void NonSerializableTest()
		{
			using (MemcachedClient client = GetClient())
			{
				Assert.IsFalse(client.Store(StoreMode.Set, "VALUE", new NonSerializableObject()), "Storing a non serializable object should have failed");
			}
		}

		private string[] keyParts = { "multi", "get", "test", "key", "parts", "test", "values" };

		protected string MakeRandomKey(int partCount)
		{
			var sb = new StringBuilder();
			var rnd = new Random();

			for (var i = 0; i < partCount; i++)
			{
				sb.Append(keyParts[rnd.Next(keyParts.Length)]).Append(":");
			}

			sb.Length--;

			return sb.ToString();
		}

		[TestCase]
		public void MultiGetTest()
		{
			

			// note, this test will fail, if memcached version is < 1.2.4
			using (var client = GetClient())
			{
				var keys = new List<string>();

				for (int i = 0; i < 100; i++)
				{
					string k = MakeRandomKey(4) + i;
					keys.Add(k);

					client.Store(StoreMode.Set, k, i);
				}

				IDictionary<string, object> retvals = client.Get(keys);

				Assert.AreEqual(100, retvals.Count, "MultiGet should have returned 100 items.");

				object value;

				for (int i = 0; i < retvals.Count; i++)
				{
					string key = keys[i];

					Assert.IsTrue(retvals.TryGetValue(key, out value), "missing key: " + key);
					Assert.AreEqual(value, i, "Invalid value returned: " + value);
				}
			}
		}

		[TestCase]
		public void FlushTest()
		{
			using (MemcachedClient client = GetClient())
			{
				Assert.IsTrue(client.Store(StoreMode.Set, "qwer", "1"), "Initialization failed");
				Assert.IsTrue(client.Store(StoreMode.Set, "tyui", "1"), "Initialization failed");
				Assert.IsTrue(client.Store(StoreMode.Set, "polk", "1"), "Initialization failed");
				Assert.IsTrue(client.Store(StoreMode.Set, "mnbv", "1"), "Initialization failed");
				Assert.IsTrue(client.Store(StoreMode.Set, "zxcv", "1"), "Initialization failed");
				Assert.IsTrue(client.Store(StoreMode.Set, "gfsd", "1"), "Initialization failed");

				Assert.AreEqual("1", client.Get("mnbv"), "Setup for FlushAll() failed");

				client.FlushAll();

				Assert.IsNull(client.Get("qwer"), "FlushAll() failed.");
				Assert.IsNull(client.Get("tyui"), "FlushAll() failed.");
				Assert.IsNull(client.Get("polk"), "FlushAll() failed.");
				Assert.IsNull(client.Get("mnbv"), "FlushAll() failed.");
				Assert.IsNull(client.Get("zxcv"), "FlushAll() failed.");
				Assert.IsNull(client.Get("gfsd"), "FlushAll() failed.");
			}
		}
	}
}

#region [ License information          ]
/* ************************************************************
 * 
 *    Copyright (c) 2010 Attila Kisk�, enyim.com
 *    
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *    
 *        http://www.apache.org/licenses/LICENSE-2.0
 *    
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *    
 * ************************************************************/
#endregion
