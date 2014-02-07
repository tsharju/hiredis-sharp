using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Hiredis
{
	public class RedisPipeline : IDisposable
	{
		private RedisClient Client;
		private int OpCount;

		public RedisPipeline(RedisClient client)
		{
			this.Client = client;

			this.OpCount = 0; // initialize the operation count for the pipeline
		}

		public void Dispose()
		{
			this.Flush();
		}

		public void Flush()
		{
			for (int i=0; i < this.OpCount; i++)
			{
				this.GetReply();
			}
			this.OpCount = 0;
		}

		public IEnumerable<RedisReply> FlushEnum()
		{
			for (int i=0; i < this.OpCount; i++)
			{
				yield return this.GetReply();
			}
		}

		public void AppendCommand(string command)
		{
			LibHiredis.RedisAppendCommand(this.Client.ContextPtr, command);
			this.OpCount++;
		}

		public void AppendCommand(string command, string key)
		{
			var cmd = String.Format("{0} %s", command);
			LibHiredis.RedisAppendCommand(this.Client.ContextPtr, cmd, key);
			this.OpCount++;
		}

		public void AppendCommand(string command, string key, string value)
		{
			var cmd = String.Format("{0} %s %s", command);
			LibHiredis.RedisAppendCommand(this.Client.ContextPtr, cmd, key, value);
			this.OpCount++;
		}

		public void AppendCommand(params string[] argv)
		{
			LibHiredis.RedisAppendCommandArgv(this.Client.ContextPtr, argv.Length, argv, null);
			this.OpCount++;
		}

		private RedisReply GetReply()
		{
			IntPtr replyPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ReplyStruct)));
			Marshal.StructureToPtr(new ReplyStruct(), replyPtr, false);

			var result = LibHiredis.RedisGetReply(this.Client.ContextPtr, ref replyPtr);

			if (result == 0)
				return this.Client.CheckForError(new RedisReply(replyPtr));
			else
				throw new Exception(); // something went wrong
		}
	}
}