using System.Collections.Generic;
using VContainer;

namespace Bulbul;

[Preserve]
internal class FacilityLockEventServiceGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		FacilityLockEventService facilityLockEventService = new FacilityLockEventService();
		Inject(facilityLockEventService, resolver, parameters);
		return facilityLockEventService;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
