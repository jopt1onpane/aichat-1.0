using System.Collections.Generic;
using VContainer;

namespace Bulbul;

[Preserve]
internal class DefaultPomodoroCompletedSchedulerGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		DefaultPomodoroCompletedScheduler defaultPomodoroCompletedScheduler = new DefaultPomodoroCompletedScheduler();
		Inject(defaultPomodoroCompletedScheduler, resolver, parameters);
		return defaultPomodoroCompletedScheduler;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
