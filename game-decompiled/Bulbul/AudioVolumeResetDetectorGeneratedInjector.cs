using System.Collections.Generic;
using VContainer;

namespace Bulbul;

[Preserve]
internal class AudioVolumeResetDetectorGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		AudioVolumeResetDetector audioVolumeResetDetector = new AudioVolumeResetDetector((AudioMixerGroupContainer)resolver.ResolveOrParameter(typeof(AudioMixerGroupContainer), "audioMixer", parameters));
		Inject(audioVolumeResetDetector, resolver, parameters);
		return audioVolumeResetDetector;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
