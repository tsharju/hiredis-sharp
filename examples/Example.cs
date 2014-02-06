using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;

using Hiredis;

public class HiredisExample
{
	static int Main(string[] args)
	{
		using (var client = new RedisClient("localhost", 6379))
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
			using (var pipeline1 = client.GetPipeline())
			{
				for (int i=0; i < 10; i++)
					pipeline1.AppendCommand("SET %s %s", String.Format("pipeline:test:{0}", i), String.Format("test:{0}", i));
			}

			// How to get replys for pipelined commands
			var pipeline2 = client.GetPipeline();
			
			pipeline2.AppendCommand("SET %s %s", "pipeline:enum:test:0", "test:0");
			pipeline2.AppendCommand("SET %s %s", "pipeline:enum:test:1", "test:1");
			pipeline2.AppendCommand("GET %s", "pipeline:enum:test:0");
			pipeline2.AppendCommand("GET %s", "pipeline:enum:test:1");

			foreach (var reply in pipeline2.FlushEnum())
			{
				Console.WriteLine("REPLY: {0}", reply.String);
			}

			Console.WriteLine("");

			// Set example
			using (var reply1 = client.Command("SADD %s %s", "smembers:test", "item1"))
			using (var reply2 = client.Command("SADD %s %s", "smembers:test", "item2"))
			{
				Console.WriteLine("REPLY 1: {0}", reply1.String);
				Console.WriteLine("REPLY 2: {0}", reply2.String);
			}
			using (var reply = client.Command("SMEMBERS %s", "smembers:test"))
			{
				foreach (var member in reply.Array)
				{
					Console.WriteLine("MEMBER: {0}", member.String);
				}
			}
		}

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
		for (int i=0; i < 1000; i++)
		{
			var key = String.Format("pool:test:{0}", i);

			using (var client = connectionPool.GetClient())
			{
				using (var pipeline = client.GetPipeline())
				{
					pipeline.AppendCommand("SET %s %s", key, String.Format("pool:data:{0}", i));
					pipeline.AppendCommand("EXPIRE %s %s", key, "5");
				}
			}
		}
	}
}