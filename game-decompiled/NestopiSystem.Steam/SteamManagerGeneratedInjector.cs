using System.Collections.Generic;
using VContainer;

namespace NestopiSystem.Steam;

[Preserve]
internal class SteamManagerGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		SteamManager steamManager = new SteamManager();
		Inject(steamManager, resolver, parameters);
		return steamManager;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
