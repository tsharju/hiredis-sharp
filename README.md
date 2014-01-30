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
