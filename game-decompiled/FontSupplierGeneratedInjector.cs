using System;
using System.Collections.Generic;
using VContainer;

[Preserve]
internal class FontSupplierGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		throw new NotSupportedException("UnityEngine.Component:FontSupplier cannot be `new`");
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
