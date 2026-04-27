using UnityEngine.Audio;

namespace Bulbul;

public readonly struct AudioMixerGroups(AudioMixerGroup masterGroup, AudioMixerGroup seGroup, AudioMixerGroup musicGroup, AudioMixerGroup voiceGroup, AudioMixerGroup ambientBGMGroup, AudioMixerGroup ambientSeGroup)
{
	public readonly AudioMixerGroup MasterGroup = masterGroup;

	public readonly AudioMixerGroup SEGroup = seGroup;

	public readonly AudioMixerGroup MusicGroup = musicGroup;

	public readonly AudioMixerGroup VoiceGroup = voiceGroup;

	public readonly AudioMixerGroup AmbientBGMGroup = ambientBGMGroup;

	public readonly AudioMixerGroup AmbientSEGroup = ambientSeGroup;
}
