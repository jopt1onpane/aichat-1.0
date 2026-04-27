using System.Collections.Generic;
using Bulbul.Web;
using NestopiSystem.Steam;
using VContainer;

namespace Bulbul;

[Preserve]
internal class EntryBehaviorGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		EntryBehavior entryBehavior = new EntryBehavior((MasterDataLoader)resolver.ResolveOrParameter(typeof(MasterDataLoader), "masterDataLoader", parameters), (SteamManager)resolver.ResolveOrParameter(typeof(SteamManager), "steamManager", parameters), (MusicService)resolver.ResolveOrParameter(typeof(MusicService), "musicManager", parameters), (UnlockMusic)resolver.ResolveOrParameter(typeof(UnlockMusic), "unlockMusic", parameters), (LoadDirectionService)resolver.ResolveOrParameter(typeof(LoadDirectionService), "loadDirectionService", parameters), (LoginFlow)resolver.ResolveOrParameter(typeof(LoginFlow), "loginFlow", parameters), (SaveDataDirtyManager)resolver.ResolveOrParameter(typeof(SaveDataDirtyManager), "saveDataDirtyManager", parameters), (WebApiGate)resolver.ResolveOrParameter(typeof(WebApiGate), "webApiGate", parameters), (WebApiErrorBehavior)resolver.ResolveOrParameter(typeof(WebApiErrorBehavior), "apiErrorBehavior", parameters), (IUICanvasProvider)resolver.ResolveOrParameter(typeof(IUICanvasProvider), "uiCanvasProvider", parameters), (AudioMixerGroupContainer)resolver.ResolveOrParameter(typeof(AudioMixerGroupContainer), "audioMixerGroupContainer", parameters));
		Inject(entryBehavior, resolver, parameters);
		return entryBehavior;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
