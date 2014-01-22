using System;
using System.Diagnostics;

using Hiredis;

public class HiredisExample
{
	static int Main(string[] args)
		{
			using (var client = new Client("localhost", 6379))
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
					foreach (var r in reply.Array())
					{
						System.Console.WriteLine("INDEX: {0} VALUE: {1}", i, r.String);
						i++;
					}
				}
			}
			return 0;
		}
}