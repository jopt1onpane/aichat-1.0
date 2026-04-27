namespace Bulbul;

public class HashSaveDataMetaGenerator<T> : ISaveDataMetaGenerator
{
	public static readonly HashSaveDataMetaGenerator<T> Default = new HashSaveDataMetaGenerator<T>();

	public string Key => FileNameWithoutExtension;

	public string FileName => FileNameWithoutExtension + Extension;

	public string FileNameWithoutExtension => typeof(T).Name.GetHashCode().ToString();

	public string Extension => ".es3";
}
