using System.Collections.Generic;
using VContainer;

namespace Bulbul;

[Preserve]
internal class PomodoroTimerServiceGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		PomodoroTimerService pomodoroTimerService = new PomodoroTimerService();
		Inject(pomodoroTimerService, resolver, parameters);
		return pomodoroTimerService;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
