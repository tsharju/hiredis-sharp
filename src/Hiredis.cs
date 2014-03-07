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

	public interface IRedisReply : IDisposable
	{
		long Integer { get; }
		string String { get; }
		IEnumerable<Hiredis.RedisReply> Array { get; }

		ReplyType Type { get; }
	}

	public interface IRedisClient : IDisposable
	{
		IRedisReply Command(params string[] argv);
		IRedisReply Command(string command);
		IRedisReply Command(string command, string key);
		IRedisReply Command(string command, string key, string value);

		void Connect();

		IRedisPipeline GetPipeline();

		IRedisSubscription GetSubscription();
	}

	public class RedisReply : IRedisReply
	{
		internal IntPtr replyPtr;

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

	public class RedisClient : IRedisClient
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

		private IRedisReply CheckForError(RedisReply reply)
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

		internal IRedisReply GetReply(IntPtr replyPtr=default(IntPtr))
		{
			if (replyPtr == IntPtr.Zero)
			{
				replyPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ReplyStruct)));
				Marshal.StructureToPtr(new ReplyStruct(), replyPtr, false);
			}

			var result = LibHiredis.RedisGetReply(this.ContextPtr, ref replyPtr);

			if (result == 0)
				return this.CheckForError(new RedisReply(replyPtr));
			else
				throw new Exception(); // something went wrong
		}

		public IRedisPipeline GetPipeline()
		{
			return new RedisPipeline(this);
		}

		public IRedisSubscription GetSubscription()
		{
			return new RedisSubscription(this);
		}

		public IRedisReply Command(string command)
		{
			var replyPtr = LibHiredis.RedisCommand(this.ContextPtr, command);
			return this.CheckForError(new RedisReply(replyPtr));
		}

		public IRedisReply Command(string command, string key)
		{
			var cmd = String.Format("{0} %s", command);
			var replyPtr = LibHiredis.RedisCommand(this.ContextPtr, cmd, key);
			return this.CheckForError(new RedisReply(replyPtr));
		}

		public IRedisReply Command(string command, string key, string value)
		{
			var cmd = String.Format("{0} %s %s", command);
			var replyPtr = LibHiredis.RedisCommand(this.ContextPtr, cmd, key, value);
			return this.CheckForError(new RedisReply(replyPtr));
		}

		public IRedisReply Command(params string[] argv)
		{
			var replyPtr = LibHiredis.RedisCommandArgv(this.ContextPtr, argv.Length, argv, null);
			return this.CheckForError(new RedisReply(replyPtr));
		}
	}
}
