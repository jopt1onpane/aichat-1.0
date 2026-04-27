using System.Collections.Generic;
using Bulbul.Web;
using VContainer;

namespace Bulbul;

[Preserve]
internal class WebApiPulserGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		WebApiPulser webApiPulser = new WebApiPulser((WebApiGate)resolver.ResolveOrParameter(typeof(WebApiGate), "webApiGate", parameters), (WebApiErrorBehavior)resolver.ResolveOrParameter(typeof(WebApiErrorBehavior), "webApiErrorBehavior", parameters), (SaveDataDirtyManager)resolver.ResolveOrParameter(typeof(SaveDataDirtyManager), "saveDataDirtyManager", parameters), (ScenarioReader)resolver.ResolveOrParameter(typeof(ScenarioReader), "scenarioReader", parameters), (LanguageSupplier)resolver.ResolveOrParameter(typeof(LanguageSupplier), "languageSupplier", parameters));
		Inject(webApiPulser, resolver, parameters);
		return webApiPulser;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
