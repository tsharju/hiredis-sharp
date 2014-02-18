using System;

namespace Hiredis
{
	public class SubscriptionFailedException : System.Exception {
		public SubscriptionFailedException() : base() {}
	}

	public class RedisSubscription
	{
		private RedisClient client;
		private bool hasSubscriptions = false;

		public Action<string> OnSubscribe { get; set; }
		public Action<string> OnUnsubscribe { get; set; }
		public Action<string, string> OnMessage { get; set; }

		public RedisSubscription(RedisClient client)
		{
			this.client = client;
		}

		public void Subscribe(params string[] channels)
		{
			DoCommand("SUBSCRIBE", channels);
		}

		public void PSubscribe(params string[] patterns)
		{
			DoCommand("PSUBSCRIBE", patterns);
		}

		public void Unsubscribe(params string[] channels)
		{
			DoCommand("UNSUBSCRIBE", channels);
		}

		public void PUnsubscribe(params string[] patterns)
		{
			DoCommand("PUNSUBSCRIBE", patterns);
		}

		private void HandleReply(RedisReply reply)
		{
			var enumerator = reply.Array.GetEnumerator();

			enumerator.MoveNext();
			var messageType = enumerator.Current.String;

			string channel;
			long numChannels;

			switch (messageType)
			{
				case "subscribe":

					enumerator.MoveNext();
					channel = enumerator.Current.String;
					enumerator.Current.Dispose();

					enumerator.MoveNext();
					numChannels = enumerator.Current.Integer;
					enumerator.Current.Dispose();

					if (numChannels > 0)
					{
						this.hasSubscriptions = true;
					}

					if (this.OnSubscribe != null)
					{
						this.OnSubscribe(channel);
					}

					break;

				case "unsubscribe":

					enumerator.MoveNext();
					channel = enumerator.Current.String;
					enumerator.Current.Dispose();

					enumerator.MoveNext();
					numChannels = enumerator.Current.Integer;
					enumerator.Current.Dispose();

					if (numChannels == 0)
					{
						this.hasSubscriptions = false; // subscribe loop will exit
					}

					if (this.OnUnsubscribe != null)
					{
						this.OnUnsubscribe(channel);
					}

					break;

				case "message":

					enumerator.MoveNext();
					channel = enumerator.Current.String;
					enumerator.Current.Dispose();
					
					enumerator.MoveNext();
					var payload = enumerator.Current.String;
					enumerator.Current.Dispose();

					if (this.OnMessage != null)
					{
						this.OnMessage(channel, payload);
					}
					
					break;
			}
		}

		private void DoCommand(string command, string[] channels)
		{
			string[] args = new string[channels.Length + 1];
			args[0] = command;
			Array.Copy(channels, 0, args, 1, channels.Length);

			using (var reply = this.client.Command(args))
			{
				this.HandleReply(reply);
			}

			while (this.hasSubscriptions)
			{
				using (var reply = this.client.GetReply())
				{
					this.HandleReply(reply);
				}
			}
		}
	}
}