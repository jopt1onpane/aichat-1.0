using System.IO;
using System.Linq;

namespace Bulbul;

public readonly struct FileSnapShotDiff
{
	public readonly FileInfo[] AddedOrModified;

	public readonly long AddedOrModifiedSize;

	public readonly string[] DeletedFileNames;

	public FileSnapShotDiff(FilesSnapshot oldSnapshot, FilesSnapshot newSnapshot, string directoryPath)
	{
		FileMeta[] source = newSnapshot.GetAddedOrModified(oldSnapshot).ToArray();
		AddedOrModifiedSize = source.Sum((FileMeta x) => x.Size);
		AddedOrModified = (from x in source
			select new FileInfo(Path.Combine(directoryPath, x.FileName)) into x
			where x.Exists
			select x).ToArray();
		DeletedFileNames = (from x in newSnapshot.GetDeleted(oldSnapshot)
			select x.FileName).ToArray();
	}

	public static FileSnapShotDiff Create(FilesSnapshot oldSnapshot, FilesSnapshot newSnapshot, string directoryPath)
	{
		return new FileSnapShotDiff(oldSnapshot, newSnapshot, directoryPath);
	}
}
