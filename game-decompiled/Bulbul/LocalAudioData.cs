using System;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class LocalAudioData
{
	public string FilePath;

	public string UUID;

	public LocalAudioData(string filePath, string uuid)
	{
		FilePath = filePath;
		UUID = uuid;
	}
}
