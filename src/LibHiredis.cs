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
		internal UIntPtr elements;
		internal IntPtr element;
	}

	static internal class LibHiredis
	{
		[DllImport ("libhiredis")]
		internal static extern void redisFree(IntPtr context);

		[DllImport ("libhiredis")]
		internal static extern IntPtr redisConnect(string host, int port);

		[DllImport ("libhiredis")]
		internal static extern IntPtr redisCommand(IntPtr context, string command);

		[DllImport ("libhiredis")]
		internal static extern IntPtr redisCommand(IntPtr context, string command, string value, UIntPtr valueLen);

		[DllImport ("libhiredis")]
		internal static extern void freeReplyObject(IntPtr reply);
	}
}
