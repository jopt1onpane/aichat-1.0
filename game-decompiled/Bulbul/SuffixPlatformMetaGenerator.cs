using FastEnumUtility;

namespace Bulbul;

public class SuffixPlatformMetaGenerator<T> : ISaveDataMetaGenerator
{
	public static readonly SuffixPlatformMetaGenerator<T> Default = new SuffixPlatformMetaGenerator<T>();

	public string Key => typeof(T).Name;

	public string FileName => FileNameWithoutExtension + Extension;

	public string FileNameWithoutExtension => typeof(T).Name + "_" + DevicePlatform.Steam.ToName();

	public string Extension => ".es3";
}
