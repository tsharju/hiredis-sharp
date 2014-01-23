using System;
using System.Diagnostics;

using Hiredis;

public class HiredisExample
{
	static int Main(string[] args)
		{
			using (var client = new RedisClient("localhost", 6379))
			{
				using (var reply = client.Command("SET %s %s", "test", "test"))
				{
					Console.WriteLine("REPLY: {0}", reply.String);
				}
			}

			return 0;
		}
}