using System.Collections.Generic;
using VContainer;

[Preserve]
internal class DateServiceGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		DateService dateService = new DateService();
		Inject(dateService, resolver, parameters);
		return dateService;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
