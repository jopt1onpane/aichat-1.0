using System;
using System.Collections.Generic;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[Obsolete]
[DoNotObfuscateClass]
public class MusicSetting
{
	public bool IsGameStartPlayMusic = true;

	public List<LocalAudioData> LocalAudioDatas = new List<LocalAudioData>();

	public List<string> FavoriteAudioUUIDs = new List<string>();

	public List<string> PlaylistOrder = new List<string>();

	public List<string> ExcludedFromPlaylistUUIDs = new List<string>();

	public MusicSettingV2 ToV2()
	{
		return new MusicSettingV2
		{
			IsGameStartPlayMusic = IsGameStartPlayMusic,
			FavoriteAudioUUIDs = FavoriteAudioUUIDs,
			PlaylistOrder = PlaylistOrder,
			ExcludedFromPlaylistUUIDs = ExcludedFromPlaylistUUIDs
		};
	}
}
