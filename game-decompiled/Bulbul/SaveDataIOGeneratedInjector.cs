using System.Collections.Generic;
using Bulbul.Web;
using VContainer;

namespace Bulbul;

[Preserve]
internal class SaveDataIOGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		SaveDataIO saveDataIO = new SaveDataIO((WebApiGate)resolver.ResolveOrParameter(typeof(WebApiGate), "webApiGate", parameters));
		Inject(saveDataIO, resolver, parameters);
		return saveDataIO;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
