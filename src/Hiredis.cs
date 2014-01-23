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
		private bool nested;

		public string String { get { return reply.str; } }
		public Int64 Integer { get { return reply.integer; } }
		public IEnumerable<RedisReply> Array { get { return this.ArrayEnum(); }}

		public ReplyType Type { get { return reply.type; } }

		public RedisReply(IntPtr replyPtr, bool nested=false)
		{
			this.replyPtr = replyPtr;
			this.nested = nested;

			if (replyPtr != IntPtr.Zero) {
				this.reply = (ReplyStruct) Marshal.PtrToStructure(replyPtr, typeof(ReplyStruct));
			}
		}

		~RedisReply()
		{
			if (replyPtr != IntPtr.Zero)
				Dispose();
		}

		public void Dispose()
		{
			if (!this.nested) {
				// nested reply objects get freed when their parent gets freed
				LibHiredis.FreeReplyObject(this.replyPtr);
			}
			this.replyPtr = IntPtr.Zero;
		}

		private IEnumerable<RedisReply> ArrayEnum()
		{
			for (int i=0; i < this.reply.elements; i++)
			{
				IntPtr replyPtr = Marshal.ReadIntPtr(this.reply.element, i * IntPtr.Size);
				yield return new RedisReply(replyPtr, true);
			}
		}
	}

	public class RedisClient : IDisposable
	{
		public string host;
		public int port;

		protected IntPtr contextPtr;

		public RedisClient(string host, int port)
		{
			this.host = host;
			this.port = port;

			this.contextPtr = LibHiredis.RedisConnect(host, port);

			var context = (ContextStruct) Marshal.PtrToStructure(this.contextPtr, typeof(ContextStruct));

			if (context.error != 0)
				throw new ConnectionFailedException(context.errstr);
		}

		~RedisClient()
		{
			if (this.contextPtr != IntPtr.Zero)
				Dispose();
		}

		public void Dispose()
		{
			LibHiredis.RedisFree(this.contextPtr);
			this.contextPtr = IntPtr.Zero;
		}

		public RedisReply Command(string command)
		{
			var replyPtr = LibHiredis.RedisCommand(this.contextPtr, command);
			return new RedisReply(replyPtr);
		}

		public RedisReply Command(string command, string key)
		{
			var replyPtr = LibHiredis.RedisCommand(this.contextPtr, command, key);
			return new RedisReply(replyPtr);
		}

		public RedisReply Command(string command, string key, string value)
		{
			var replyPtr = LibHiredis.RedisCommand(this.contextPtr, command, key, value);
			return new RedisReply(replyPtr);
		}

		public void AppendCommand(string command)
		{
			LibHiredis.RedisAppendCommand(this.contextPtr, command);
		}

		public void AppendCommand(string command, string key)
		{
			LibHiredis.RedisAppendCommand(this.contextPtr, command, key);
		}

		public void AppendCommand(string command, string key, string value)
		{
			LibHiredis.RedisAppendCommand(this.contextPtr, command, key, value);
		}

		public RedisReply GetReply()
		{
			IntPtr replyPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ReplyStruct)));
			Marshal.StructureToPtr(new ReplyStruct(), replyPtr, false);

			var result = LibHiredis.RedisGetReply(this.contextPtr, replyPtr);

			if (result == 0)
				return new RedisReply(replyPtr);
			else
				throw new Exception(); // something went wrong
		}
	}
}