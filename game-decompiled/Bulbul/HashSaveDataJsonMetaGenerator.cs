namespace Bulbul;

public class HashSaveDataJsonMetaGenerator<T> : ISaveDataMetaGenerator
{
	public static readonly HashSaveDataJsonMetaGenerator<T> Default = new HashSaveDataJsonMetaGenerator<T>();

	public string Key => FileNameWithoutExtension;

	public string FileName => FileNameWithoutExtension + Extension;

	public string FileNameWithoutExtension => typeof(T).Name.GetHashCode().ToString();

	public string Extension => ".bin";
}
