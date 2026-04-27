using System.Collections.Generic;
using NestopiSystem.Steam;
using VContainer;

namespace Bulbul.Achievements;

[Preserve]
internal class SteamAchievementsGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		SteamAchievements steamAchievements = new SteamAchievements((SteamManager)resolver.ResolveOrParameter(typeof(SteamManager), "steamManager", parameters));
		Inject(steamAchievements, resolver, parameters);
		return steamAchievements;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
