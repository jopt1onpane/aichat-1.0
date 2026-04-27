using NestopiSystem.DIContainers;
using UnityEngine;
using UnityEngine.Audio;
using VContainer;

namespace Bulbul;

public class AudioMixerGroupRegister : MonoRegister
{
	[SerializeField]
	private AudioMixer audioMixer;

	[SerializeField]
	private AudioMixerGroup masterGroup;

	[SerializeField]
	private AudioMixerGroup bgmGroup;

	[SerializeField]
	private AudioMixerGroup seGroup;

	[SerializeField]
	private AudioMixerGroup musicGroup;

	[SerializeField]
	private AudioMixerGroup voiceGroup;

	[SerializeField]
	private AudioMixerGroup ambientBGMGroup;

	[SerializeField]
	private AudioMixerGroup ambientSEGroup;

	public override void Register(IContainerBuilder builder)
	{
		builder.RegisterInstance(audioMixer);
		AudioMixerGroups value = new AudioMixerGroups(masterGroup, seGroup, musicGroup, voiceGroup, ambientBGMGroup, ambientSEGroup);
		builder.Register<AudioMixerGroupContainer>(Lifetime.Singleton).WithParameter(value);
	}
}
