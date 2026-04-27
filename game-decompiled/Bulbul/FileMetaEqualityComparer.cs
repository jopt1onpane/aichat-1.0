using System;
using System.Collections.Generic;

namespace Bulbul;

public class FileMetaEqualityComparer : IEqualityComparer<FileMeta>
{
	public static readonly FileMetaEqualityComparer Default = new FileMetaEqualityComparer();

	public bool Equals(FileMeta x, FileMeta y)
	{
		if (x == y)
		{
			return true;
		}
		if (x == null)
		{
			return false;
		}
		if (y == null)
		{
			return false;
		}
		if (string.Equals(x.FileName, y.FileName, StringComparison.OrdinalIgnoreCase) && x.Size == y.Size)
		{
			return x.LastWriteTime.Equals(y.LastWriteTime);
		}
		return false;
	}

	public int GetHashCode(FileMeta obj)
	{
		if (obj == null)
		{
			return 0;
		}
		return HashCode.Combine((obj.FileName != null) ? StringComparer.OrdinalIgnoreCase.GetHashCode(obj.FileName) : 0, obj.Size, obj.LastWriteTime);
	}
}
