using System;
using System.Collections.Generic;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class LocalMusicSetting
{
	public List<LocalAudioData> LocalAudioDatas = new List<LocalAudioData>();

	public LocalMusicSetting()
	{
	}

	public LocalMusicSetting(List<LocalAudioData> localAudioDatas)
	{
		LocalAudioDatas = localAudioDatas;
	}
}
