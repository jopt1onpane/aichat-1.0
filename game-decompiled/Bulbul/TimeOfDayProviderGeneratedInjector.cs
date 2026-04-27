using System.Collections.Generic;
using VContainer;

namespace Bulbul;

[Preserve]
internal class TimeOfDayProviderGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		TimeOfDayProvider timeOfDayProvider = new TimeOfDayProvider();
		Inject(timeOfDayProvider, resolver, parameters);
		return timeOfDayProvider;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
