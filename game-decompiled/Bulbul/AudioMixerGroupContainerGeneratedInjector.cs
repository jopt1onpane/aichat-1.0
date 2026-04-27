using System.Collections.Generic;
using UnityEngine.Audio;
using VContainer;

namespace Bulbul;

[Preserve]
internal class AudioMixerGroupContainerGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		AudioMixerGroupContainer audioMixerGroupContainer = new AudioMixerGroupContainer((MasterDataLoader)resolver.ResolveOrParameter(typeof(MasterDataLoader), "masterDataLoader", parameters), (AudioMixer)resolver.ResolveOrParameter(typeof(AudioMixer), "audioMixer", parameters), (AudioMixerGroups)resolver.ResolveOrParameter(typeof(AudioMixerGroups), "groups", parameters));
		Inject(audioMixerGroupContainer, resolver, parameters);
		return audioMixerGroupContainer;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
