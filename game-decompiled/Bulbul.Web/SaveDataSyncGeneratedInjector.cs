using System.Collections.Generic;
using VContainer;

namespace Bulbul.Web;

[Preserve]
internal class SaveDataSyncGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		SaveDataSync saveDataSync = new SaveDataSync((SaveDataIO)resolver.ResolveOrParameter(typeof(SaveDataIO), "saveDataIO", parameters));
		Inject(saveDataSync, resolver, parameters);
		return saveDataSync;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
