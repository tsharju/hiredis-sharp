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

			using (var reply1 = client.Command("SET", "set:test", "test"))
			using (var reply2 = client.Command("GET", "set:test"))
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
					pipeline1.AppendCommand("SET", String.Format("pipeline:test:{0}", i), String.Format("test:{0}", i));
			}

			// How to get replys for pipelined commands
			var pipeline2 = client.GetPipeline();
			
			pipeline2.AppendCommand("SET", "pipeline:enum:test:0", "test:0");
			pipeline2.AppendCommand("SET", "pipeline:enum:test:1", "test:1");
			pipeline2.AppendCommand("GET", "pipeline:enum:test:0");
			pipeline2.AppendCommand("GET", "pipeline:enum:test:1");

			foreach (var reply in pipeline2.FlushEnum())
			{
				Console.WriteLine("REPLY: {0}", reply.String);
			}

			Console.WriteLine("");

			// Set example
			using (var reply1 = client.Command("SADD", "smembers:test", "item1"))
			using (var reply2 = client.Command("SADD", "smembers:test", "item2"))
			using (var reply3 = client.Command("EXPIRE", "smembers:test", "60"))
			{
				Console.WriteLine("REPLY 1: {0}", reply1.Type);
				Console.WriteLine("REPLY 2: {0}", reply2.Type);
				Console.WriteLine("REPLY 2: {0}", reply3.Type);
			}
			using (var reply = client.Command("SMEMBERS", "smembers:test"))
			{
				foreach (var member in reply.Array)
				{
					Console.WriteLine("MEMBER: {0}", member.Type);
				}
			}

			using (var reply = client.Command(new string[] {"MSET", "test1", "test1", "test2", "test2"}))
			{
				Console.WriteLine("REPLY: {0}", reply.Type);
			}

			using (var reply = client.Command("MGET", "test1", "test2", "test3"))
			{
				foreach (var member in reply.Array)
				{
					Console.WriteLine("REPLY: {0}", member.Type);
				}
			}

			using (var reply1 = client.Command(new string[] {"SET", "setrange:test", "Hello World"}))
			using (var reply2 = client.Command("SETRANGE", "setrange:test", "6", "Redis"))
			{
				Console.WriteLine("REPLY: {0}", reply1.String);
				Console.WriteLine("REPLY: {0}", reply2.Integer);
			}
		}

		/*var connectionPool = new RedisConnectionPool("localhost", 6379);

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
		}*/

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
					pipeline.AppendCommand("SET", key, String.Format("pool:data:{0}", i));
					pipeline.AppendCommand("EXPIRE", key, "5");
				}
			}
		}
	}
}