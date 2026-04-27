using System.Collections.Generic;
using VContainer;

namespace Bulbul;

[Preserve]
internal class ScenarioGroupMasterWrapperGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		ScenarioGroupMasterWrapper scenarioGroupMasterWrapper = new ScenarioGroupMasterWrapper((MasterDataLoader)resolver.ResolveOrParameter(typeof(MasterDataLoader), "masterDataLoader", parameters));
		Inject(scenarioGroupMasterWrapper, resolver, parameters);
		return scenarioGroupMasterWrapper;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
