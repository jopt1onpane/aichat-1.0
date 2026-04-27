using System;
using System.Collections.Generic;

namespace Bulbul;

public class FileMetaNameOnlyEqualityComparer : IEqualityComparer<FileMeta>
{
	public static readonly FileMetaNameOnlyEqualityComparer Default = new FileMetaNameOnlyEqualityComparer();

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
		return string.Equals(x.FileName, y.FileName, StringComparison.OrdinalIgnoreCase);
	}

	public int GetHashCode(FileMeta obj)
	{
		if (obj == null)
		{
			return 0;
		}
		if (obj.FileName == null)
		{
			return 0;
		}
		return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.FileName);
	}
}
