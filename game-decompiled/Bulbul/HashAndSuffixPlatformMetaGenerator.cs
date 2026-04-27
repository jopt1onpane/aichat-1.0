using FastEnumUtility;

namespace Bulbul;

public class HashAndSuffixPlatformMetaGenerator<T> : ISaveDataMetaGenerator
{
	public static readonly HashAndSuffixPlatformMetaGenerator<T> Default = new HashAndSuffixPlatformMetaGenerator<T>();

	public string Key => FileNameWithoutExtension;

	public string FileName => FileNameWithoutExtension + Extension;

	public string FileNameWithoutExtension => typeof(T).Name.GetHashCode() + "_" + DevicePlatform.Steam.ToName();

	public string Extension => ".es3";
}
