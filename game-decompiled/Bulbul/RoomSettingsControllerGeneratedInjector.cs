using System.Collections.Generic;
using VContainer;

namespace Bulbul;

[Preserve]
internal class RoomSettingsControllerGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		RoomSettingsController roomSettingsController = new RoomSettingsController();
		Inject(roomSettingsController, resolver, parameters);
		return roomSettingsController;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
