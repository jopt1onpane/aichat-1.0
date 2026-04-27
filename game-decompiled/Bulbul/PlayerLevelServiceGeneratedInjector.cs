using System.Collections.Generic;
using VContainer;

namespace Bulbul;

[Preserve]
internal class PlayerLevelServiceGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		PlayerLevelService playerLevelService = new PlayerLevelService();
		Inject(playerLevelService, resolver, parameters);
		return playerLevelService;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
