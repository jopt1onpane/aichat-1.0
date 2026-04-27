namespace Bulbul;

public interface ISaveDataMetaGenerator
{
	string Key { get; }

	string FileName { get; }

	string FileNameWithoutExtension { get; }

	string Extension { get; }
}
