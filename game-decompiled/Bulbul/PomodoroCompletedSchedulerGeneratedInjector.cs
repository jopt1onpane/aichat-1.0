using System.Collections.Generic;
using VContainer;

namespace Bulbul;

[Preserve]
internal class PomodoroCompletedSchedulerGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		PomodoroCompletedScheduler pomodoroCompletedScheduler = new PomodoroCompletedScheduler();
		Inject(pomodoroCompletedScheduler, resolver, parameters);
		return pomodoroCompletedScheduler;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
