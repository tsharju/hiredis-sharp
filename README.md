Hiredis-sharp
=============

C# Redis driver that uses `libhiredis`.

Building
========

Like the name already hints, building `hiredis-sharp` requires `libhiredis`. So far there have not been any attemts on building this library for Windows platform. It is mainly intended for use with the Mono Framework running on Linux or OS X.

Make sure you have `libhiredis` installed and in the project directory type:

```
make
```

That's it. The build will succeed whether you had `libhiredis` properly installed or not. To test whether the library works on your system you can build a simple example and try to run it. To do that, type:

```
make example
```

Now you should have an EXE file called `Example.exe` in the `/build` directory. Make sure you have Redis server running in localhost and run this command:

```
mono build/Example.exe
```

If you get something like this, your `libhiredis` is not properly installed.

```
Unhandled Exception:
System.DllNotFoundException: libhiredis
  at (wrapper managed-to-native) Hiredis.LibHiredis:RedisConnect (string,int)
  at Hiredis.RedisClient.Connect () [0x00000] in <filename unknown>:0 
  at Hiredis.RedisConnectionPool.GetClient () [0x00000] in <filename unknown>:0 
  at HiredisExample.Work (System.Object data) [0x00000] in <filename unknown>:0 
  at System.Threading.Thread.StartInternal () [0x00000] in <filename unknown>:0 
```

To debug the problem you can run the previous command again and set the Mono loglevel to `debug`. Like this:

```
MONO_LOG_LEVEL=debug mono build/Example.exe
```

> A common problem on OS X is that the Mono binary distribution for OS X that you can download from Mono website,
> is 32bit and if you install `libhiredis` for example using Homebrew, it is built 64bit. In this case you will
> get the `System.DllNotFoundException` even you have `libhiredis` installed. To fix this you can build mono from
> sources and make sure it is 64bit.

API
===

The API is pretty much the same as using `libhiredis` from C. However, there are some stuff that make things easier, like the `Hiredis.RedisPipeline` or the `Hiredis.RedisConnectionPool` class. Anyways the API is really simple.

Connecting
----------

All you need to do is create a new `Hiredis.RedisClient` object.

```c#
using Hiredis;

RedisClient client = new RedisClient("localhost", 6379);
```

`Hiredis.RedisClient` implements `IDisposable` so it is very much recommended to use it inside a `using` block so that the resourses get released properly. Like this...

```c#
using Hiredis;

using (var client = new RedisClient("localhost", 6379))
{
  // do your thing
}
```

> Another option is to call `RedisClient.Dispose()` after you're done with it.

Using Connection Pool
---------------------

`Hiredis.RedisConnectionPool` implements a thread safe connection pooling. Use it like this...

```c#
using Hiredis;

RedisConnectionPool connectionPool = new RedisConnectionPool("localhost", 6379);

using (var client = connectionPool.GetClient())
{
  // do your thing
}
```

Redis Commands
--------------

Issuing commands to Redis is really simple. You just call either `RedisClient.Command` or `RedisPipeline.AppendCommand` methods and get back a `RedisReply`. For full list of commands see [Redis command reference](http://redis.io/commands). Here are few examples.

```c#
using System;
using Hiredis;

using (var client = new RedisClient("localhost", 6379))
{
  using (var reply1 = client.Command("SET", "key", "value"))
  using (var reply2 = client.Command("GET", "key"))
  {
    Console.WriteLine("Reply 1: {0}", reply1.Type);
    Console.WriteLine("Reply 2: {0}", reply2.String);
  }
}
```
