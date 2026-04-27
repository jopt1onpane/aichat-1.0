namespace Bulbul;

public class DefaultSaveDataMetaGenerator<T> : ISaveDataMetaGenerator
{
	public static readonly DefaultSaveDataMetaGenerator<T> Default = new DefaultSaveDataMetaGenerator<T>();

	public string Key => typeof(T).Name;

	public string FileName => FileNameWithoutExtension + Extension;

	public string FileNameWithoutExtension => typeof(T).Name;

	public string Extension => ".es3";
}
