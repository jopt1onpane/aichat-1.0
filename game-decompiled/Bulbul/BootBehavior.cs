using Cysharp.Threading.Tasks;
using KanKikuchi.AudioManager;
using NestopiSystem.DIContainers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer.Unity;

namespace Bulbul;

public class BootBehavior : MonoBehaviour
{
	[SerializeField]
	private LoadDirectionService _loadDirectionService;

	private void Start()
	{
		LoadAsync().Forget();
	}

	private async UniTask LoadAsync()
	{
		Object.DontDestroyOnLoad(_loadDirectionService.gameObject);
		_loadDirectionService.StartDirection(DevicePlatform.Steam.IsPC());
		await Addressables.InitializeAsync();
		await SceneLoader.LoadSceneAdditiveAsync("AssetLoad");
		AssetLoadHolder assetLoadHolder = Object.FindAnyObjectByType<AssetLoadHolder>();
		assetLoadHolder.AudioManagerSetting.Initialize();
		await UniTask.WhenAll(AmbientBGMManager.CreateAndInitAsync(), AmbientSEManager.CreateAndInitAsync(), MusicManager.CreateAndInitAsync(), ScenarioAmbientBGMManager.CreateAndInitAsync(), ScenarioMusicManager.CreateAndInitAsync(), VoiceManager.CreateAndInitAsync(), SEManager.CreateAndInitAsync());
		VContainerSettings vContainerSettings = assetLoadHolder.VContainerSettings;
		if (vContainerSettings.RootLifetimeScope != null)
		{
			Debug.LogError("VContainerSettings.RootLifetimeScopeは動的にセットするため、nullにしてください");
		}
		vContainerSettings.RootLifetimeScope = assetLoadHolder.ProjectLifetimeScope;
		ProjectLifetimeScope.SetInstance((ProjectLifetimeScope)vContainerSettings.GetOrCreateRootLifetimeScopeInstance());
		await UniTask.NextFrame();
		ProjectLifetimeScope.Resolve<MasterDataLoader>().Load().Forget();
		Debug.LogDeveloperCheck("Load Entry Scene Start");
		await SceneLoader.LoadSceneAdditiveAsync("Entry");
	}
}
