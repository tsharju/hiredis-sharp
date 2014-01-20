using System;
using System.Text;
using System.Runtime.InteropServices;

namespace Hiredis
{
	public class ConnectionFailedException : System.Exception {
		public ConnectionFailedException(string msg) : base(msg) {}
	}

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

			if (context.error != 0)
				throw new ConnectionFailedException(context.errstr);
		}

		public void Dispose()
		{
			LibHiredis.redisFree(this.contextPtr);
		}

		private Reply Command(string command)
		{
			var replyPtr = LibHiredis.redisCommand(this.contextPtr, command);
			return new Reply(replyPtr);
		}

		private Reply Command(string command, string key)
		{
			var replyPtr = LibHiredis.redisCommand(this.contextPtr, command, key);
			return new Reply(replyPtr);
		}

		private Reply Command(string command, string key, string value)
		{
			var replyPtr = LibHiredis.redisCommand(this.contextPtr, command, key, value);
			return new Reply(replyPtr);
		}

		public Reply SET(string key, string value)
		{
			return Command("SET %s %s", key, value);
		}

		public Reply GET(string key)
		{
			return Command("GET %s", key);
		}

		public Reply DEL(string key)
		{
			return Command("DEL %s", key);
		}

		public Reply PING()
		{
			return Command("PING");
		}
	}
}