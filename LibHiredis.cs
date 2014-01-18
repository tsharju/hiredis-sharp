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
	public struct ContextStruct
	{
	    public int error;
	    [MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
	    public string errstr;
	    public int fd;
	    public int flags;
	    public IntPtr obuf;
	    public IntPtr reader;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct ReplyStruct
	{
		public ReplyType type;
		public Int64 integer;
		public int len;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string str;
		public UIntPtr elements;
		public IntPtr element;
	}

	static public class LibHiredis
	{
		[DllImport ("libhiredis")]
		public static extern void redisFree(IntPtr context);

		[DllImport ("libhiredis")]
		public static extern IntPtr redisConnect(string host, int port);

		[DllImport ("libhiredis")]
		public static extern IntPtr redisCommand(IntPtr context, string command);

		[DllImport ("libhiredis")]
		public static extern IntPtr redisCommand(IntPtr context, string command, string value, UIntPtr valueLen);

		[DllImport ("libhiredis")]
		public static extern void freeReplyObject(IntPtr reply);
	}
}
