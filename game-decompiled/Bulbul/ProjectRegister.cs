using Bulbul.Achievements;
using Bulbul.Web;
using NestopiSystem;
using NestopiSystem.DIContainers;
using NestopiSystem.Steam;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Bulbul;

public class ProjectRegister : MonoRegister
{
	[SerializeField]
	private FontSupplier fontSupplier;

	[SerializeField]
	private CommonPrefabSupplier commonPrefabSupplier;

	public override void Register(IContainerBuilder builder)
	{
		builder.RegisterEntryPoint<SteamManager>().AsSelf();
		builder.RegisterEntryPoint<AudioVolumeResetDetector>();
		builder.Register<MasterDataLoader>(Lifetime.Singleton);
		builder.Register<LocalizationMasterWrapper>(Lifetime.Singleton);
		builder.Register<ScenarioGroupMasterWrapper>(Lifetime.Singleton);
		builder.Register<SystemSeService>(Lifetime.Scoped);
		builder.RegisterComponentInHierarchy<LoadingScreen>();
		builder.Register<LanguageSupplier>(Lifetime.Singleton);
		builder.Register<IDefaultLanguageSupplier, SteamDefaultLanguageSupplier>(Lifetime.Singleton);
		builder.Register<IAchievementList, SteamAchievements>(Lifetime.Singleton);
		builder.RegisterInstance(fontSupplier);
		builder.RegisterInstance(commonPrefabSupplier);
		builder.Register<SaveDataIO>(Lifetime.Singleton);
		builder.Register<SaveDataMigrator>(Lifetime.Singleton);
		builder.Register<SaveDataDirtyManager>(Lifetime.Singleton);
		builder.Register<SaveDataSync>(Lifetime.Singleton);
		builder.Register<UnlockMusic>(Lifetime.Singleton);
		builder.Register<WebApiGate>(Lifetime.Singleton);
		builder.Register<AppAuth>(Lifetime.Singleton);
		builder.Register<GoogleAuthLogic>(Lifetime.Singleton);
		builder.Register<AppleAuthLogic>(Lifetime.Singleton);
		builder.RegisterComponentOnNewGameObject<NativeProxy>(Lifetime.Singleton, "NativeProxy");
		builder.RegisterBuildCallback(delegate(IObjectResolver c)
		{
			Object.DontDestroyOnLoad(c.Resolve<NativeProxy>().gameObject);
		});
		builder.Register<TimeOfDayProvider>(Lifetime.Singleton);
		builder.Register<PurchasingCtrl>(Lifetime.Singleton);
		builder.RegisterBuildCallback(delegate(IObjectResolver c)
		{
			c.Resolve<PurchasingCtrl>();
		});
	}
}
