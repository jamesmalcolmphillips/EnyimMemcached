using System;
using System.Collections.Generic;
using System.Net;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using Enyim.Reflection;

namespace NorthScale.Store.Configuration
{
	/// <summary>
	/// Configuration class
	/// </summary>
	public class NorthScaleClientConfiguration : INorthScaleClientConfiguration
	{
		private List<Uri> urls;
		private ISocketPoolConfiguration socketPool;
		private ICredentials credentials;
		private string bucket;
		private BucketPortType port = BucketPortType.Proxy;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:MemcachedClientConfiguration"/> class.
		/// </summary>
		public NorthScaleClientConfiguration()
		{
			this.urls = new List<Uri>();
			this.socketPool = new SocketPoolConfiguration();
		}

		public string Bucket
		{
			get { return this.bucket; }
			set { this.bucket = value; }
		}

		/// <summary>
		/// Gets a list of <see cref="T:IPEndPoint"/> each representing a Memcached server in the pool.
		/// </summary>
		public IList<Uri> Urls
		{
			get { return this.urls; }
		}

		public ICredentials Credentials
		{
			get { return this.credentials; }
			set { this.credentials = value; }
		}

		/// <summary>
		/// Gets the configuration of the socket pool.
		/// </summary>
		public ISocketPoolConfiguration SocketPool
		{
			get { return this.socketPool; }
		}

		/// <summary>
		/// Gets or sets the <see cref="T:Enyim.Caching.Memcached.IMemcachedKeyTransformer"/> which will be used to convert item keys for Memcached.
		/// </summary>
		public IMemcachedKeyTransformer KeyTransformer { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="T:Enyim.Caching.Memcached.IMemcachedNodeLocator"/> which will be used to assign items to Memcached nodes.
		/// </summary>
		public IMemcachedNodeLocator NodeLocator { get; set;}

		/// <summary>
		/// Gets or sets the <see cref="T:Enyim.Caching.Memcached.ITranscoder"/> which will be used serialzie or deserialize items.
		/// </summary>
		public ITranscoder Transcoder { get; set; }

		/// <summary>
		/// Determines which port the client should use to connect to the nodes
		/// </summary>
		public BucketPortType Port
		{
			get { return this.port; }
			set { this.port = value; }
		}

		#region [ interface                     ]
		IList<Uri> INorthScaleClientConfiguration.Urls
		{
			get { return this.Urls; }
		}

		ICredentials INorthScaleClientConfiguration.Credentials
		{
			get { return this.Credentials; }
		}

		ISocketPoolConfiguration INorthScaleClientConfiguration.SocketPool
		{
			get { return this.SocketPool; }
		}

		IMemcachedKeyTransformer INorthScaleClientConfiguration.CreateKeyTransformer()
		{
			return this.KeyTransformer;
		}

		IMemcachedNodeLocator INorthScaleClientConfiguration.CreateNodeLocator()
		{
			return this.NodeLocator;
		}

		ITranscoder INorthScaleClientConfiguration.CreateTranscoder()
		{
			return this.Transcoder;
		}

		string INorthScaleClientConfiguration.Bucket
		{
			get { return this.bucket; }
		}

		BucketPortType INorthScaleClientConfiguration.Port
		{
			get { return this.port; }
		}

		#endregion
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
