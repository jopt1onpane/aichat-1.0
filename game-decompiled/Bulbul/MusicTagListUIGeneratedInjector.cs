using System;
using System.Collections.Generic;
using VContainer;

namespace Bulbul;

[Preserve]
internal class MusicTagListUIGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		throw new NotSupportedException("UnityEngine.Component:MusicTagListUI cannot be `new`");
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		MusicTagListUI obj = (MusicTagListUI)instance;
		MusicService musicService = (MusicService)resolver.ResolveOrParameter(typeof(MusicService), "musicService", parameters);
		obj.Construct(musicService);
	}
}
