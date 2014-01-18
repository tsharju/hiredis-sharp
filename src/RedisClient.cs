using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Hiredis
{
	public class Reply : IDisposable
	{
		private IntPtr replyPtr;
		private ReplyStruct reply;

		public string String { get { return reply.str; } }
		public ReplyType Type { get { return reply.type; } }

		public Reply(IntPtr replyPtr)
		{
			this.replyPtr = replyPtr;

			if (replyPtr != IntPtr.Zero)
				this.reply = (ReplyStruct) Marshal.PtrToStructure(replyPtr, typeof(ReplyStruct));
		}

		public void Dispose()
		{
			LibHiredis.freeReplyObject(this.replyPtr);
		}
	}

	public class Client : IDisposable
	{
		public string host;
		public int port;

		private IntPtr contextPtr;

		public Client(string host, int port)
		{
			this.host = host;
			this.port = port;

			this.contextPtr = LibHiredis.redisConnect(host, port);

			var context = (ContextStruct) Marshal.PtrToStructure(this.contextPtr, typeof(ContextStruct));

			if (context.error == 0)
				System.Console.WriteLine("Connected!");
			else
				System.Console.WriteLine("ERROR: " + context.errstr);
		}

		public void Dispose()
		{
			LibHiredis.redisFree(this.contextPtr);
		}

		private Reply Command(string command, string value)
		{
			var replyPtr = LibHiredis.redisCommand(this.contextPtr, command, value, (UIntPtr) value.Length);
			return new Reply(replyPtr);
		}

		private Reply Command(string command)
		{
			var replyPtr = LibHiredis.redisCommand(this.contextPtr, command);
			return new Reply(replyPtr);
		}

		public Reply SET(string key, string value)
		{
			return Command("SET " + key + "%b", value);
		}

		public Reply GET(string key)
		{
			return Command("GET " + key);
		}

		public Reply PING()
		{
			return Command("PING");
		}

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
}