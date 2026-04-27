namespace Bulbul;

public class SaveDataJsonMetaGenerator<T> : ISaveDataMetaGenerator
{
	public static readonly SaveDataJsonMetaGenerator<T> Default = new SaveDataJsonMetaGenerator<T>();

	public string Key => FileNameWithoutExtension;

	public string FileName => FileNameWithoutExtension + Extension;

	public string FileNameWithoutExtension => typeof(T).Name;

	public string Extension => ".bin";
}
