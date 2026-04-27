using System.Collections.Generic;
using VContainer;

namespace Bulbul;

[Preserve]
internal class AppleAuthLogicGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		AppleAuthLogic appleAuthLogic = new AppleAuthLogic();
		Inject(appleAuthLogic, resolver, parameters);
		return appleAuthLogic;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
