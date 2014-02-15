using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;


public class RedisBenchmark
{
	static string Payload = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit, sed diam nonummy nibh euismod tincidunt ut laoreet dolore magna aliquam erat volutpat. Ut wisi enim ad minim veniam, quis nostrud exerci tation ullamcorper suscipit lobortis nisl ut aliquip ex ea commodo consequat. Duis autem vel eum iriure dolor in hendrerit in vulputate velit esse molestie consequat, vel illum dolore eu feugiat nulla facilisis at vero eros et accumsan et iusto odio dignissim qui blandit praesent luptatum zzril delenit augue duis dolore te feugait nulla facilisi. Nam liber tempor cum soluta nobis eleifend option congue nihil imperdiet doming id quod mazim placerat facer possim assum. Typi non habent claritatem insitam; est usus legentis in iis qui facit eorum claritatem. Investigationes demonstraverunt lectores legere me lius quod ii legunt saepius. Claritas est etiam processus dynamicus, qui sequitur mutationem consuetudium lectorum. Mirum est notare quam littera gothica, quam nunc putamus parum claram, anteposuerit litterarum formas humanitatis per seacula quarta decima et quinta decima. Eodem modo typi, qui nunc nobis videntur parum clari, fiant sollemnes in futurum.";

	static int Main(string[] args)
	{
		Console.WriteLine("");
		Console.WriteLine("Performing 100k SET operations (Hiredis) ...");

		var stopwatch = new Stopwatch();

		using (var client = new Hiredis.RedisClient("localhost", 6379))
		{
			stopwatch.Start();

			for (int i=0; i < 100000; i++)
			{
				using (var reply = client.Command("SET", String.Format("test:hiredis:key:{0}", i), Payload))
				{
					if (reply.String != "OK")
					{
						Console.WriteLine("Aborting!");
						break;
					}
				}
			}

			stopwatch.Stop();
		}

		Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
		Console.WriteLine("Ops/sec: {0}", 100000.0f / (stopwatch.ElapsedMilliseconds / 1000.0f));
		Console.WriteLine("Memory used: {0} kB", GC.GetTotalMemory(true) / 1024);

		stopwatch.Reset();
		GC.Collect();

		Console.WriteLine("");
		Console.WriteLine("Performing 100k SET operations (ServiceStack) ...");

		using (var client = new ServiceStack.Redis.RedisClient("localhost", 6379))
		{
			stopwatch.Start();

			for (int i=0; i < 100000; i++)
			{
				client.Set(String.Format("test:servicestack:key:{0}", i), Payload);
			}

			stopwatch.Stop();
		}

		Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
		Console.WriteLine("Ops/sec: {0}", 100000.0f / (stopwatch.ElapsedMilliseconds / 1000.0f));
		Console.WriteLine("Memory used: {0} kB", GC.GetTotalMemory(true) / 1024);

		stopwatch.Reset();
		GC.Collect();

		Console.WriteLine("");
		Console.WriteLine("Performing 100k SET operations using pipeline (Hiredis) ...");

		using (var client = new Hiredis.RedisClient("localhost", 6379))
		{
			stopwatch.Start();

			// pipeline 10k commands at a time
			for (int i=0; i < 10; i++)
			{
				using (var pipeline = client.GetPipeline())
				{
					for (int j=0; j < 10000; j++)
					{
						pipeline.AppendCommand("SET", String.Format("test:hiredis:pipeline:key:{0}:{1}", i, j), Payload);
					}
				}
			}

			stopwatch.Stop();
		}

		Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
		Console.WriteLine("Ops/sec: {0}", 100000.0f / (stopwatch.ElapsedMilliseconds / 1000.0f));
		Console.WriteLine("Memory used: {0} kB", GC.GetTotalMemory(true) / 1024);

		stopwatch.Reset();
		GC.Collect();

		Console.WriteLine("");
		Console.WriteLine("Performing 100k SET operations using pipeline (ServiceStack) ...");

		using (var client = new ServiceStack.Redis.RedisClient("localhost", 6379))
		{

			stopwatch.Start();


			// pipeline 10k commands at a time
			for (int i=0; i < 10; i++)
			{
				using (var pipeline = client.CreatePipeline())
				{
					for (int j=0; j < 10000; j++)
					{
						pipeline.QueueCommand(r => r.Set(String.Format("test:servicestack:pipeline:key:{0}:{1}", i, j), Payload));
					}

					pipeline.Flush();
				}
			}

			stopwatch.Stop();
		}

		Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
		Console.WriteLine("Ops/sec: {0}", 100000.0f / (stopwatch.ElapsedMilliseconds / 1000.0f));
		Console.WriteLine("Memory used: {0} kB", GC.GetTotalMemory(true) / 1024);
		Console.WriteLine("");

		return 0;
	}
}