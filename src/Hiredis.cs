using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Hiredis
{
	public class ConnectionFailedException : System.Exception {
		public ConnectionFailedException(string msg) : base(msg) {}
	}

	public class RedisReply : IDisposable
	{
		private IntPtr replyPtr;
		private ReplyStruct reply;

		public string String { get { return reply.str; } }
		public Int64 Integer { get { return reply.integer; } }

		public ReplyType Type { get { return reply.type; } }

		public RedisReply(IntPtr replyPtr)
		{
			this.replyPtr = replyPtr;

			if (replyPtr != IntPtr.Zero) {
				this.reply = (ReplyStruct) Marshal.PtrToStructure(replyPtr, typeof(ReplyStruct));
			}
		}

		public IEnumerable<RedisReply> Array()
		{
			for (int i=0; i < this.reply.elements; i++)
			{
				IntPtr replyPtr = Marshal.ReadIntPtr(this.reply.element, i * IntPtr.Size);
				yield return new RedisReply(replyPtr);
			}
		}

		public void Dispose()
		{
			LibHiredis.FreeReplyObject(this.replyPtr);
		}
	}

	public class RedisClient : IDisposable
	{
		public string host;
		public int port;

		private IntPtr contextPtr;

		public RedisClient(string host, int port)
		{
			this.host = host;
			this.port = port;

			this.contextPtr = LibHiredis.RedisConnect(host, port);

			var context = (ContextStruct) Marshal.PtrToStructure(this.contextPtr, typeof(ContextStruct));

			if (context.error != 0)
				throw new ConnectionFailedException(context.errstr);
		}

		public void Dispose()
		{
			LibHiredis.RedisFree(this.contextPtr);
		}

		private RedisReply RedisCommand(string command)
		{
			var replyPtr = LibHiredis.RedisCommand(this.contextPtr, command);
			return new RedisReply(replyPtr);
		}

		private RedisReply RedisCommand(string command, string key)
		{
			var replyPtr = LibHiredis.RedisCommand(this.contextPtr, command, key);
			return new RedisReply(replyPtr);
		}

		private RedisReply RedisCommand(string command, string key, string value)
		{
			var replyPtr = LibHiredis.RedisCommand(this.contextPtr, command, key, value);
			return new RedisReply(replyPtr);
		}

		public RedisReply SET(string key, string value)
		{
			return RedisCommand("SET %s %s", key, value);
		}

		public RedisReply GET(string key)
		{
			return RedisCommand("GET %s", key);
		}

		public RedisReply DEL(string key)
		{
			return RedisCommand("DEL %s", key);
		}

		public RedisReply SADD(string key, string value)
		{
			return RedisCommand("SADD %s %s", key, value);
		}

		public RedisReply SMEMBERS(string key)
		{

			return RedisCommand("SMEMBERS %s", key);
		}

		public RedisReply SCARD(string key)
		{
			return RedisCommand("SCARD %s", key);
		}

		public RedisReply PING()
		{
			return RedisCommand("PING");
		}
	}
}