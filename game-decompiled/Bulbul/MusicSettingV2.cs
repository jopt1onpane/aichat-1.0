using System;
using System.Collections.Generic;
using GUPS.Obfuscator.Attribute;
using R3;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class MusicSettingV2
{
	public bool IsGameStartPlayMusic = true;

	public bool IsShufflePlayMusic;

	[ES3NonSerializable]
	public ReactiveProperty<AudioTag> CurrentAudioTag = new ReactiveProperty<AudioTag>();

	[ES3Serializable]
	private AudioTag _audioTagForSave;

	public List<string> FavoriteAudioUUIDs = new List<string>();

	public List<string> PlaylistOrder = new List<string>();

	public List<string> ExcludedFromPlaylistUUIDs = new List<string>();

	public MusicSettingV2()
	{
		IsGameStartPlayMusic = true;
		IsShufflePlayMusic = false;
		CurrentAudioTag.Value = AudioTag.All;
		_audioTagForSave = AudioTag.All;
	}

	public void LoadSetup()
	{
		CurrentAudioTag.Value = _audioTagForSave;
	}

	public void SaveReady()
	{
		_audioTagForSave = CurrentAudioTag.Value;
	}
}
