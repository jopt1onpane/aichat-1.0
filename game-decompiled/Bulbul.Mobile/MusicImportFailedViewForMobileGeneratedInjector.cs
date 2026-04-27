using System;
using System.Collections.Generic;
using VContainer;

namespace Bulbul.Mobile;

[Preserve]
internal class MusicImportFailedViewForMobileGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		throw new NotSupportedException("UnityEngine.Component:MusicImportFailedViewForMobile cannot be `new`");
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		MusicImportFailedViewForMobile obj = (MusicImportFailedViewForMobile)instance;
		DirectionService directionService = (DirectionService)resolver.ResolveOrParameter(typeof(DirectionService), "directionService", parameters);
		obj.Construct(directionService);
	}
}
