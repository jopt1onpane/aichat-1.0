using System.Collections.Generic;
using VContainer;

namespace Bulbul;

[Preserve]
internal class EnvironmentDataServiceGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		EnvironmentDataService environmentDataService = new EnvironmentDataService();
		Inject(environmentDataService, resolver, parameters);
		return environmentDataService;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
