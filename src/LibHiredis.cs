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
		[MarshalAs(UnmanagedType.LPTStr)]
		internal string str;
		internal Int64 elements;
		internal IntPtr element;
	}

	static internal class LibHiredis
	{
		[DllImport ("libhiredis", EntryPoint="redisFree")]
		internal static extern void RedisFree(IntPtr context);

		[DllImport ("libhiredis", EntryPoint="redisConnect")]
		internal static extern IntPtr RedisConnect(string host, int port);

		[DllImport ("libhiredis", EntryPoint="redisCommand", CallingConvention=CallingConvention.Cdecl)]
		internal static extern IntPtr RedisCommand(IntPtr context, string command);

		[DllImport ("libhiredis", EntryPoint="redisCommand", CallingConvention=CallingConvention.Cdecl)]
		internal static extern IntPtr RedisCommand(IntPtr context, string command, string key);

		[DllImport ("libhiredis", EntryPoint="redisCommand", CallingConvention=CallingConvention.Cdecl)]
		internal static extern IntPtr RedisCommand(IntPtr context, string command, string key, string value);

		[DllImport ("libhiredis", EntryPoint="freeReplyObject")]
		internal static extern void FreeReplyObject(IntPtr reply);
	}
}
