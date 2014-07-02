using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Hiredis
{
	public class ConnectionPoolTimeout : System.Exception {
		public ConnectionPoolTimeout(string msg) : base(msg) {}
	}

	public interface IRedisConnectionPool
	{
		string Host { get; }
		int Port { get; }
		int Size { get; }

		void AddClient(PooledRedisClient client);
		void Close();

		PooledRedisClient GetClient (int timeout = -1);
	}

	public class PooledRedisClient : RedisClient, IDisposable
	{
		private IRedisConnectionPool Pool;

		public PooledRedisClient(IRedisConnectionPool pool) : base(pool.Host, pool.Port, false)
		{
			this.Pool = pool;
		}

		public new void Dispose()
		{
			// we return the client to the pool
			this.Pool.AddClient(this);
		}
	}

	public abstract class RedisBaseConnectionPool : IDisposable
	{
		public string Host { get { return this.host; } }
		public int Port { get { return this.port; } }
		public int Size { get { return this.size; } }

		private readonly string host;
		private readonly int port;
		private readonly int size;

		protected HashSet<PooledRedisClient> connectionsInUse = new HashSet<PooledRedisClient>();
		protected bool closing = false;

		public RedisBaseConnectionPool(string host, int port, int size)
		{
			this.host = host;
			this.port = port;
			this.size = size;
		}

		protected void markUsed(PooledRedisClient client)
		{
			this.connectionsInUse.Add (client);
		}

		protected void markUnused(PooledRedisClient client)
		{
			this.connectionsInUse.Remove (client);
		}

		public void Dispose()
		{
			this.Close ();
		}

		public abstract void AddClient (PooledRedisClient client);
		public abstract PooledRedisClient GetClient (int timeout = -1);
		public abstract void Close ();
	}

	public class RedisConnectionPool : RedisBaseConnectionPool, IRedisConnectionPool
	{
		private ConcurrentBag<PooledRedisClient> pool;

		public RedisConnectionPool (string host, int port, int size) : base(host, port, size)
		{
			this.pool = new ConcurrentBag<PooledRedisClient> ();
		}

		public override PooledRedisClient GetClient(int timeout = -1)
		{
			if (this.closing)
				return null;

			PooledRedisClient client;

			if (this.pool.TryTake (out client)) {
				// we check if there's too many connections and
				// disconnect one of them on every call to GetClient
				if (this.pool.Count > this.Size)
				{
					PooledRedisClient removedClient;
					if (this.pool.TryTake (out removedClient))
					{
						removedClient.Disconnect (dispose: true);
					}
				}

				if (!client.Connected)
					client.Connect ();
			}
			else
			{
				// couldn't get a client from the pool
				// we create a new one
				client = new PooledRedisClient(this);
				client.Connect ();
			}

			// keep track of connections taken from pool
			this.markUsed (client);

			return client;
		}

		public override void AddClient(PooledRedisClient client)
		{
			this.markUnused (client);

			this.pool.Add (client);
		}

		public override void Close()
		{
			this.closing = true;

			foreach (var client in this.pool)
			{
				client.Disconnect (dispose: true);
			}

			foreach (var client in this.connectionsInUse)
			{
				client.Disconnect (dispose: true);
			}
		}
	}

	public class RedisBlockingConnectionPool : RedisBaseConnectionPool, IRedisConnectionPool
	{
		private BlockingCollection<PooledRedisClient> pool;

		public RedisBlockingConnectionPool(string host, int port, int size) : base(host, port, size)
		{
			this.pool = new BlockingCollection<PooledRedisClient>(size);

			// fill the pool with clients that are not connected
			// we will connect them once they are fetched from the pool
			for (int i = 0; i < size; i++)
			{
				this.pool.Add(new PooledRedisClient(this));
			}
		}

		public override PooledRedisClient GetClient(int timeout = -1)
		{
			PooledRedisClient client;

			if (this.pool.TryTake(out client, timeout))
			{
				if (!client.Connected)
					client.Connect();

				this.markUsed (client);

				return client;
			}
			else
			{
				throw new ConnectionPoolTimeout(
					String.Format("Could not get client from pool in {0} ms. Connections in-use/in-pool={1}/{2}.",
						timeout, this.connectionsInUse.Count, this.pool.Count));
			}
		}

		public override void AddClient(PooledRedisClient client)
		{
			this.markUnused (client);

			this.pool.Add (client);
		}

		public override void Close()
		{
			this.closing = true;

			foreach (var client in this.pool)
			{
				client.Disconnect (dispose: true);
			}

			foreach (var client in this.connectionsInUse)
			{
				client.Disconnect (dispose: true);
			}
		}
	}
}