using System;
using System.Collections.Generic;
using VContainer;

namespace Bulbul;

[Preserve]
internal class TooltipServiceGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		throw new NotSupportedException("UnityEngine.Component:TooltipService cannot be `new`");
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
