using System.Collections.Generic;
using VContainer;

namespace Bulbul;

[Preserve]
internal class RoomCalendarControllerGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		RoomCalendarController roomCalendarController = new RoomCalendarController();
		Inject(roomCalendarController, resolver, parameters);
		return roomCalendarController;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
