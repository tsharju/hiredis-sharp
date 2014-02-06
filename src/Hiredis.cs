using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Hiredis
{
	public class ConnectionFailedException : System.Exception {
		public ConnectionFailedException(string msg) : base(msg) {}
	}

	public class CommandFailedException : System.Exception {
		public CommandFailedException(string msg) : base(msg) {}
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
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				// nothing to dispose here really
			}

			if (replyPtr != IntPtr.Zero)
			{
				// reply objects inside array get freed when the array is freed
				if (!this.nested)
				{
					LibHiredis.FreeReplyObject(this.replyPtr);
				}
				this.replyPtr = IntPtr.Zero;
			}
		}

		private IEnumerable<RedisReply> ArrayEnum()
		{
			for (int i=0; i < this.reply.elements; i++)
			{
				IntPtr replyPtr = Marshal.ReadIntPtr(this.reply.element, i * IntPtr.Size);
				yield return new RedisReply(replyPtr, true); // a nested reply
			}
		}
	}

	public class RedisClient : IDisposable
	{
		public string Host;
		public int Port;
		public bool Connected = false;

		internal IntPtr ContextPtr;

		public RedisClient(string host, int port, bool connect=true)
		{
			this.Host = host;
			this.Port = port;

			if (connect)
				this.Connect();
		}

		public void Connect()
		{
			this.ContextPtr = LibHiredis.RedisConnect(this.Host, this.Port);

			var context = (ContextStruct) Marshal.PtrToStructure(this.ContextPtr, typeof(ContextStruct));

			if (context.error != 0)
				throw new ConnectionFailedException(context.errstr);
			else
				this.Connected = true;
		}

		~RedisClient()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.Host = null;
			}

			if (this.ContextPtr != IntPtr.Zero)
			{
				LibHiredis.RedisFree(this.ContextPtr);
				this.ContextPtr = IntPtr.Zero;
			}
		}

		internal RedisReply CheckForError(RedisReply reply)
		{
			if (reply.Type == ReplyType.Error)
			{
				throw new CommandFailedException(reply.String);
			}
			else
			{
				return reply;
			}
		}

		public RedisPipeline GetPipeline()
		{
			return new RedisPipeline(this);
		}

		public RedisReply Command(params string[] argv)
		{
			var replyPtr = LibHiredis.RedisCommandArgv(this.ContextPtr, argv.Length, argv, null);
			return this.CheckForError(new RedisReply(replyPtr));
		}

		public RedisReply Command(string command)
		{
			var replyPtr = LibHiredis.RedisCommand(this.ContextPtr, command);
			return this.CheckForError(new RedisReply(replyPtr));
		}

		public RedisReply Command(string command, string key)
		{
			var replyPtr = LibHiredis.RedisCommand(this.ContextPtr, command, key);
			return this.CheckForError(new RedisReply(replyPtr));
		}

		public RedisReply Command(string command, string key, string value)
		{
			var replyPtr = LibHiredis.RedisCommand(this.ContextPtr, command, key, value);
			return this.CheckForError(new RedisReply(replyPtr));
		}
	}
}