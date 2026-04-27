namespace Bulbul;

public class HashDatMetaGenerator<T> : ISaveDataMetaGenerator
{
	public static readonly HashDatMetaGenerator<T> Default = new HashDatMetaGenerator<T>();

	public string Key => FileNameWithoutExtension;

	public string FileName => FileNameWithoutExtension + Extension;

	public string FileNameWithoutExtension => typeof(T).Name.GetHashCode().ToString();

	public string Extension => ".dat";
}
