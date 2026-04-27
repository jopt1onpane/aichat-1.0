using System.Collections.Generic;
using VContainer;

namespace Bulbul;

[Preserve]
internal class HabitDataServiceGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		HabitDataService habitDataService = new HabitDataService();
		Inject(habitDataService, resolver, parameters);
		return habitDataService;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
