using System.Collections.Generic;
using VContainer;

namespace Bulbul;

[Preserve]
internal class CommonPrefabSupplierGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		CommonPrefabSupplier commonPrefabSupplier = new CommonPrefabSupplier();
		Inject(commonPrefabSupplier, resolver, parameters);
		return commonPrefabSupplier;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
