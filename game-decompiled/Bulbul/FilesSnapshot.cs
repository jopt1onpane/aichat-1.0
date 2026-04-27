using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[DoNotObfuscateClass]
public class FilesSnapshot
{
	public List<FileMeta> files;

	public long TotalSize { get; private set; }

	public static string DefaultTargetDirectory => new ES3Settings(BulbulConstant.SaveDirectoryPath).FullPath;

	[JsonConstructor]
	public FilesSnapshot(List<FileMeta> files, long totalSize)
	{
		this.files = files;
		TotalSize = totalSize;
	}

	public FilesSnapshot(string directoryPath, string searchPattern)
	{
		UpdateSnapshot(directoryPath, searchPattern);
	}

	public static FilesSnapshot CreateGameDataDefault()
	{
		return new FilesSnapshot(DefaultTargetDirectory, "*.es3");
	}

	public void UpdateSnapshot(string directoryPath, string searchPattern)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
		if (!directoryInfo.Exists)
		{
			files = new List<FileMeta>();
			TotalSize = 0L;
			return;
		}
		files = (from f in directoryInfo.GetFiles(searchPattern)
			select new FileMeta(f)).ToList();
		TotalSize = files.Sum((FileMeta f) => f.Size);
	}

	public IEnumerable<FileMeta> GetAddedOrModified(FilesSnapshot oldSnapshot)
	{
		return files.Except(oldSnapshot.files, FileMetaEqualityComparer.Default);
	}

	public IEnumerable<FileMeta> GetDeleted(FilesSnapshot oldSnapshot)
	{
		return oldSnapshot.files.Except(files, FileMetaNameOnlyEqualityComparer.Default);
	}
}
