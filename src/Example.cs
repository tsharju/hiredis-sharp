using System;
using System.Diagnostics;

using Hiredis;

public class HiredisExample
{
	static int Main(string[] args)
		{
			using (var client = new Client("localhost", 6379))
			{
				int operations = 100;

				System.Console.WriteLine(String.Format("Performing {0} operations...", operations));

				Stopwatch stopwatch = new Stopwatch();

				stopwatch.Start();

				for (int i=0; i < operations; i++) {
					using (var reply = client.PING())
					{
						Console.WriteLine(reply.String);
					}
				}

				stopwatch.Stop();

				Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);

				var opsPerSec = operations / stopwatch.Elapsed.TotalSeconds;

				Console.WriteLine("Operations per second: {0}", opsPerSec);
			}
			return 0;
		}
}