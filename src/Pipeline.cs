using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Hiredis
{
	public interface IRedisPipeline : IDisposable
	{
		void AppendCommand(params string[] argv);
		void AppendCommand(string command);
		void AppendCommand(string command, string key);
		void AppendCommand(string command, string key, string value);

		void Flush();
		
		IEnumerable<IRedisReply> FlushEnum();
	}

	public class RedisPipeline : IRedisPipeline
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
				var reply = this.Client.GetReply();
				reply.Dispose();
			}
			this.OpCount = 0;
		}

		public IEnumerable<IRedisReply> FlushEnum()
		{
			for (int i=0; i < this.OpCount; i++)
			{
				yield return this.Client.GetReply();
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
	}
}
