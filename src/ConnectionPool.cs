using System;
using System.Collections.Concurrent;

namespace Hiredis
{
	public class PooledRedisClient : RedisClient, IDisposable
	{
		private RedisConnectionPool Pool;

		public PooledRedisClient(RedisConnectionPool pool) : base(pool.Host, pool.Port, false)
		{
			this.Pool = pool;
		}

		public new void Dispose()
		{
			// we return the client to the pool
			this.Pool.AddClient(this);
		}
	}

	public class RedisConnectionPool
	{
		public readonly string Host;
		public readonly int Port;

		public const int GET_CLIENT_TIMEOUT = 500; // milliseconds

		private BlockingCollection<PooledRedisClient> Pool;

		public RedisConnectionPool(string host, int port, int maxSize=5)
		{
			this.Host = host;
			this.Port = port;
			this.Pool = new BlockingCollection<PooledRedisClient>(maxSize);

			// fill the pool with clients that are not connected
			// we will connect them once they are fetched from the pool
			for (int i = 0; i < maxSize; i++)
			{
				this.Pool.Add(new PooledRedisClient(this));
			}
		}

		public PooledRedisClient GetClient()
		{
			PooledRedisClient client;

			if (this.Pool.TryTake(out client, GET_CLIENT_TIMEOUT))
			{
				if (!client.Connected)
					client.Connect();

				return client;
			}
			else
			{
				throw new Exception(); // waited for a client too long
			}
		}

		public void AddClient(PooledRedisClient client)
		{
			this.Pool.Add(client);
		}
	}
}