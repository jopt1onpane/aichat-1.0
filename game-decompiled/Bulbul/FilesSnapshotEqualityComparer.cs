using System;
using System.Collections.Generic;
using System.Linq;

namespace Bulbul;

public class FilesSnapshotEqualityComparer : IEqualityComparer<FilesSnapshot>
{
	public static readonly FilesSnapshotEqualityComparer Default = new FilesSnapshotEqualityComparer();

	public bool Equals(FilesSnapshot x, FilesSnapshot y)
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
		if (x.GetType() != y.GetType())
		{
			return false;
		}
		if (x.TotalSize == y.TotalSize)
		{
			return x.files.SequenceEqual(y.files, FileMetaEqualityComparer.Default);
		}
		return false;
	}

	public int GetHashCode(FilesSnapshot obj)
	{
		if (obj == null)
		{
			return 0;
		}
		HashCode hashCode = default(HashCode);
		hashCode.Add(obj.TotalSize);
		if (obj.files != null)
		{
			foreach (FileMeta file in obj.files)
			{
				hashCode.Add(file, FileMetaEqualityComparer.Default);
			}
		}
		return hashCode.ToHashCode();
	}
}
