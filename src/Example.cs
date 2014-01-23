using System;
using System.Diagnostics;

using Hiredis;

public class HiredisExample
{
	static int Main(string[] args)
		{
			using (var client = new RedisClient("localhost", 6379))
			{
				using (var reply = client.SADD("set:test", "test"))
				{
					Console.WriteLine(reply.String);
				}

				using (var reply = client.SCARD("set:test"))
				{
					Console.WriteLine(reply.Integer);
				}

				using (var reply = client.SMEMBERS("set:test"))
				{
					int i = 0;
					foreach (var r in reply.Array)
					{
						Console.WriteLine("INDEX: {0} VALUE: {1}", i, r.String);
						i++;
					}
				}

				string test;

				using (var reply1 = client.APPEND("key:append", "Hello"))
				using (var reply2 = client.APPEND("key:append", " World!"))
				using (var reply3 = client.GET("key:append"))
				{
					Console.WriteLine("REPLY 1: {0}", reply1.Integer);
					Console.WriteLine("REPLY 2: {0}", reply2.Integer);
					Console.WriteLine("REPLY 3: {0}", reply3.String);
					test = reply3.String;
				}

				Console.WriteLine(test);

				using (var reply1 = client.SET("key:exists", "1"))
				using (var reply2 = client.EXISTS("key:exists"))
				using (var reply3 = client.EXISTS("key:noexists"))
				{
					Console.WriteLine("EXISTS: {0}", reply2.Integer);
					Console.WriteLine("EXISTS: {0}", reply3.Integer);
				}
			}

			using (var pipeline = new RedisPipelineClient("localhost", 6379))
			{
				for (int i=0; i < 10000; i++)
				{
					pipeline.SET(String.Format("pipeline:key{0}", i), "blaablaa");
				}

				for (int i=0; i < 10000; i++)
				{
					var reply = pipeline.RedisGetReply();
					Console.WriteLine("{0}:{1}", i, reply.Integer);
				}
			}

			return 0;
		}
}