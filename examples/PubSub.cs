using System;

using Hiredis;

class PubSubExample
{
	static int Main(string[] args)
	{
		using (var client = new RedisClient("localhost", 6379))
		{
			var subscription = client.GetSubscription();

			subscription.OnSubscribe = channel =>
			{
				Console.WriteLine("SUBSCRIBED: {0}", channel);
			};

			subscription.OnUnsubscribe = channel =>
			{
				Console.WriteLine("UNSUBSCRIBED: {0}", channel);
			};

			subscription.OnMessage = (channel, payload) =>
			{
				Console.WriteLine("CHANNEL: {0} MESSAGE: {1}", channel, payload);

				subscription.Unsubscribe("pubsub:test");
			};

			subscription.Subscribe("pubsub:test");
		}

		return 0;
	}
}