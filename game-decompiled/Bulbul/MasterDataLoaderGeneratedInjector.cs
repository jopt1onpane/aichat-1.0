using System.Collections.Generic;
using VContainer;

namespace Bulbul;

[Preserve]
internal class MasterDataLoaderGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		MasterDataLoader masterDataLoader = new MasterDataLoader();
		Inject(masterDataLoader, resolver, parameters);
		return masterDataLoader;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
