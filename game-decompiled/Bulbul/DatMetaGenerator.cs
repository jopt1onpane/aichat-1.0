namespace Bulbul;

public class DatMetaGenerator<T> : ISaveDataMetaGenerator
{
	public static readonly DatMetaGenerator<T> Default = new DatMetaGenerator<T>();

	public string Key => FileNameWithoutExtension;

	public string FileName => FileNameWithoutExtension + Extension;

	public string FileNameWithoutExtension => typeof(T).Name;

	public string Extension => ".dat";
}
