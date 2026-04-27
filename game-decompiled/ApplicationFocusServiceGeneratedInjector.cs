using System.Collections.Generic;
using VContainer;

[Preserve]
internal class ApplicationFocusServiceGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		ApplicationFocusService applicationFocusService = new ApplicationFocusService();
		Inject(applicationFocusService, resolver, parameters);
		return applicationFocusService;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
