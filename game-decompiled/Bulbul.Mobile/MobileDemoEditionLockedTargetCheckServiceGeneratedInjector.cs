using System.Collections.Generic;
using VContainer;

namespace Bulbul.Mobile;

[Preserve]
internal class MobileDemoEditionLockedTargetCheckServiceGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		MobileDemoEditionLockedTargetCheckService mobileDemoEditionLockedTargetCheckService = new MobileDemoEditionLockedTargetCheckService((MasterDataLoader)resolver.ResolveOrParameter(typeof(MasterDataLoader), "masterDataLoader", parameters));
		Inject(mobileDemoEditionLockedTargetCheckService, resolver, parameters);
		return mobileDemoEditionLockedTargetCheckService;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
