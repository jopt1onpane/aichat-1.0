using System.Collections.Generic;
using VContainer;

namespace Bulbul.Web;

[Preserve]
internal class WebApiGateGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		WebApiGate webApiGate = new WebApiGate();
		Inject(webApiGate, resolver, parameters);
		return webApiGate;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
