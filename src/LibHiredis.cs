using System;
using System.Runtime.InteropServices;

namespace Hiredis
{
	public enum ReplyType
	{
		String = 1,
		Array = 2,
		Integer = 3,
		Nil = 4,
		Status = 5,
		Error = 6
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct ContextStruct
	{
	    internal int error;
	    [MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
	    internal string errstr;
	    internal int fd;
	    internal int flags;
	    internal IntPtr obuf;
	    internal IntPtr reader;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct ReplyStruct
	{
		internal ReplyType type;
		internal Int64 integer;
		internal int len;
#if __MonoCS__
		[MarshalAs(UnmanagedType.LPTStr)]
#else
		[MarshalAs(UnmanagedType.LPStr)]
#endif
		internal string str;
		internal Int64 elements;
		internal IntPtr element;
	}

	static internal class LibHiredis
	{
#if __MonoCS__
		[DllImport ("libhiredis", EntryPoint="redisFree", CallingConvention=CallingConvention.Cdecl)]
#else
		[DllImport("hiredis_win_x64.dll", EntryPoint="redisFree", CallingConvention=CallingConvention.Cdecl)]
#endif
		internal static extern void RedisFree(IntPtr context);

#if __MonoCS__
		[DllImport ("libhiredis", EntryPoint="redisConnect", CallingConvention=CallingConvention.Cdecl)]
#else
		[DllImport("hiredis_win_x64.dll", EntryPoint="redisConnect", CallingConvention=CallingConvention.Cdecl)]
#endif
		internal static extern IntPtr RedisConnect(string host, int port);

#if __MonoCS__
		[DllImport ("libhiredis", EntryPoint="redisCommand", CallingConvention=CallingConvention.Cdecl)]
#else
		[DllImport("hiredis_win_x64.dll", EntryPoint="redisCommand", CallingConvention=CallingConvention.Cdecl)]
#endif
		internal static extern IntPtr RedisCommand(IntPtr context, string command);

#if __MonoCS__
		[DllImport ("libhiredis", EntryPoint="redisCommand", CallingConvention=CallingConvention.Cdecl)]
#else
		[DllImport("hiredis_win_x64.dll", EntryPoint="redisCommand", CallingConvention=CallingConvention.Cdecl)]
#endif
		internal static extern IntPtr RedisCommand(IntPtr context, string command, string key);

#if __MonoCS__
		[DllImport ("libhiredis", EntryPoint="redisCommand", CallingConvention=CallingConvention.Cdecl)]
#else
		[DllImport("hiredis_win_x64.dll", EntryPoint="redisCommand", CallingConvention=CallingConvention.Cdecl)]
#endif
		internal static extern IntPtr RedisCommand(IntPtr context, string command, string key, string value);

#if __MonoCS__
		[DllImport ("libhiredis", EntryPoint="redisAppendCommand", CallingConvention=CallingConvention.Cdecl)]
#else
		[DllImport("hiredis_win_x64.dll", EntryPoint="redisAppendCommand", CallingConvention=CallingConvention.Cdecl)]
#endif
		internal static extern void RedisAppendCommand(IntPtr context, string command);

#if __MonoCS__
		[DllImport ("libhiredis", EntryPoint="redisAppendCommand", CallingConvention=CallingConvention.Cdecl)]
#else
		[DllImport("hiredis_win_x64.dll", EntryPoint="redisAppendCommand", CallingConvention=CallingConvention.Cdecl)]
#endif
		internal static extern void RedisAppendCommand(IntPtr context, string command, string key);

#if __MonoCS__
		[DllImport ("libhiredis", EntryPoint="redisAppendCommand", CallingConvention=CallingConvention.Cdecl)]
#else
		[DllImport("hiredis_win_x64.dll", EntryPoint="redisAppendCommand", CallingConvention=CallingConvention.Cdecl)]
#endif
		internal static extern int RedisAppendCommand(IntPtr context, string command, string key, string value);

#if __MonoCS__
		[DllImport ("libhiredis", EntryPoint="redisGetReply", CallingConvention=CallingConvention.Cdecl)]
#else
		[DllImport("hiredis_win_x64.dll", EntryPoint="redisGetReply", CallingConvention=CallingConvention.Cdecl)]
#endif
		internal static extern int RedisGetReply(IntPtr context, ref IntPtr reply);

#if __MonoCS__
		[DllImport ("libhiredis", EntryPoint="redisCommandArgv", CallingConvention=CallingConvention.Cdecl)]
#else
		[DllImport("hiredis_win_x64.dll", EntryPoint="redisCommandArgv", CallingConvention=CallingConvention.Cdecl)]
#endif
			internal static extern IntPtr RedisCommandArgv(IntPtr context, int argc, [In] string[] argv, [In] Int64[] argvlen);

#if __MonoCS__
		[DllImport ("libhiredis", EntryPoint="redisAppendCommandArgv", CallingConvention=CallingConvention.Cdecl)]
#else
		[DllImport("hiredis_win_x64.dll", EntryPoint="redisAppendCommandArgv", CallingConvention=CallingConvention.Cdecl)]
#endif
			internal static extern int RedisAppendCommandArgv(IntPtr context, int argc, [In] string[] argv, [In] Int64[] argvlen);

#if __MonoCS__
		[DllImport ("libhiredis", EntryPoint="freeReplyObject", CallingConvention=CallingConvention.Cdecl)]
#else
		[DllImport("hiredis_win_x64.dll", EntryPoint="freeReplyObject", CallingConvention=CallingConvention.Cdecl)]
#endif
		internal static extern void FreeReplyObject(IntPtr reply);
	}
}
