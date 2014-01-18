using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Redis
{
	internal enum ReplyType
	{
		String = 1,
		Array = 2,
		Integer = 3,
		Nil = 4,
		Status = 5,
		Error = 6
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct Context
	{
	    internal int error;
	    [MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
	    internal string errstr;
	    internal int fd;
	    internal int flags;
	    internal IntPtr obuf;
	    internal IntPtr reader;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct Reply
	{
		internal ReplyType type;
		internal Int64 integer;
		internal int len;
		internal IntPtr str;
		internal UIntPtr elements;
		internal IntPtr element;
	}

	public class Client
	{
		public string host;
		public int port;

		private IntPtr contextPtr;

		public Client(string host, int port)
		{
			this.host = host;
			this.port = port;

			this.contextPtr = redisConnect(host, port);

			var context = (Context) Marshal.PtrToStructure(this.contextPtr, typeof(Context));

			if (context.error == 0)
				System.Console.WriteLine("Connected!");
			else
				System.Console.WriteLine("ERROR: " + context.errstr);
		}

		[DllImport ("libhiredis")]
		private static extern IntPtr redisConnect(string host, int port);

		[DllImport ("libhiredis")]
		private static extern IntPtr redisCommand(IntPtr context, string command);

		[DllImport ("libhiredis")]
		private static extern IntPtr redisCommand(IntPtr context, string command, string value, UIntPtr valueLen);

		private string GetReplyString(Reply reply)
		{
			byte[] bytes = new byte[reply.len];
			for (int i=0; i < reply.len; i++) {
				bytes[i] = Marshal.ReadByte(reply.str, i);
			}
			return System.Text.Encoding.Default.GetString(bytes);
		}

		public string SET(string key, string value)
		{
			var replyPtr = redisCommand(this.contextPtr, String.Format("SET {0} %b", key), value, (UIntPtr) value.Length);

			if (replyPtr != IntPtr.Zero)
			{
				var reply = (Reply) Marshal.PtrToStructure(replyPtr, typeof(Reply));
				return GetReplyString(reply);
			}

			return "ERROR";
		}

		public string GET(string key)
		{
			var replyPtr = redisCommand(this.contextPtr, "GET " + key);
			
			if (replyPtr != IntPtr.Zero)
			{
				var reply = (Reply) Marshal.PtrToStructure(replyPtr, typeof(Reply));

				if (reply.type == ReplyType.String)
				{
					return GetReplyString(reply);
				}
			}

			return "ERROR";
		}

		public string PING()
		{
			var replyPtr = redisCommand(this.contextPtr, "PING");

			if (replyPtr != IntPtr.Zero)
			{
				return "PONG";
			} else
			{
				return "ERROR";
			}
		}

		static int Main(string[] args)
		{
			var client = new Client("localhost", 6379);

			int operations = 100000;

			System.Console.WriteLine(String.Format("Performing {0} operations...", operations));

			Stopwatch stopwatch = new Stopwatch();

			stopwatch.Start();

			for (int i=0; i < operations; i++) {
				//client.SET("KEY:" + i, "VALUE " + i);
				client.PING();
			}

			stopwatch.Stop();

			Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);

			var opsPerSec = operations / stopwatch.Elapsed.TotalSeconds;

			Console.WriteLine("Operations per second: {0}", opsPerSec);

			return 0;
		}
	}
}