using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;

using Hiredis;

public class HiredisExample
{
	static int Main(string[] args)
	{
		/*using (var client = new RedisClient("localhost", 6379))
		{
			// Simple SET and GET example
			Console.WriteLine("=== Test SET and GET ===");

			using (var reply1 = client.Command("SET %s %s", "set:test", "test"))
			using (var reply2 = client.Command("GET %s", "set:test"))
			{
				Console.WriteLine("REPLY 1: {0}", reply1.String);
				Console.WriteLine("REPLY 2: {0}", reply2.String);
			}

			Console.WriteLine("");

			// Pipeline example
			Console.WriteLine("=== Test pipeline ===");

			// Do a few pipelined SET operations
			for (int i=0; i < 10; i++)
				client.AppendCommand("SET %s %s", String.Format("pipeline:test:{0}", i), String.Format("test:{0}", i));

			// Get replys	
			for (int i=0; i < 10; i++)
			{
				using (var reply = client.GetReply())
				{
					Console.WriteLine("REPLY {0}: {1}", i, reply.Integer);
				}
			}

			Console.WriteLine("");

			// Do a few pipelined GET operations
			for (int i=0; i < 10; i++)
			{
				client.AppendCommand("GET %s", String.Format("pipeline:test:{0}", i));
			}

			// Get replys
			for (int i=0; i < 10; i++)
			{
				using (var reply = client.GetReply())
				{
					Console.WriteLine("REPLY {0}: {1}", i, reply.String);
				}
			}

			Console.WriteLine("");
		}*/

		var connectionPool = new RedisConnectionPool("localhost", 6379);

		List<Thread> threads = new List<Thread>();

		// create a few threads to test the connection pool
		for (int i=0; i < 20; i++)
		{
			var thread = new Thread(Work);
			thread.Start(connectionPool);
			threads.Add(thread);
		}

		// wait for threads to finish
		foreach (var thread in threads)
		{
			thread.Join();
		}

		return 0;
	}

	static void Work(object data)
	{
		RedisConnectionPool connectionPool = (RedisConnectionPool) data;

		// Do a few pipelined SET operations
		for (int i=0; i < 100000; i++)
		{
			var key = String.Format("pool:test:{0}", i);

			using (var client = connectionPool.GetClient())
			{
				client.AppendCommand("SET %s %s", key, String.Format("pool:data:{0}", i));
				client.AppendCommand("EXPIRE %s %s", key, "5");
				client.GetReply();
				client.GetReply();
			}
		}
	}
}