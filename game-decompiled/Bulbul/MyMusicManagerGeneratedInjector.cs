using System.Collections.Generic;
using VContainer;

namespace Bulbul;

[Preserve]
internal class MyMusicManagerGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		MyMusicManager myMusicManager = new MyMusicManager();
		Inject(myMusicManager, resolver, parameters);
		return myMusicManager;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
