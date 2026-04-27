using System;
using System.IO;
using System.Text.Json.Serialization;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[DoNotObfuscateClass]
public class FileMeta
{
	public readonly string FileName;

	public readonly long Size;

	public readonly long LastWriteTime;

	[JsonConstructor]
	public FileMeta(string fileName, long size, long lastWriteTime)
	{
		FileName = fileName;
		Size = size;
		LastWriteTime = lastWriteTime;
	}

	public FileMeta(string fileName, long size, DateTime lastWriteTime)
		: this(fileName, size, lastWriteTime.Ticks)
	{
	}

	public FileMeta(FileInfo info)
		: this(info.Name, info.Length, info.LastWriteTime.Ticks)
	{
	}
}
