using Bulbul.Web;
using GUPS.Obfuscator.Attribute;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Bulbul;

[DoNotRename]
public class EntryLifetimeScope : LifetimeScope
{
	[SerializeField]
	private GameObject mobileParent;

	[SerializeField]
	private GameObject standaloneParent;

	protected override void Configure(IContainerBuilder builder)
	{
		Object.DestroyImmediate(DevicePlatform.Steam.IsMobile() ? standaloneParent : mobileParent);
		builder.RegisterComponentInHierarchy<IUICanvasProvider>();
		builder.Register<WebApiErrorBehavior>(Lifetime.Singleton);
		builder.RegisterComponentInHierarchy<AuthUI>();
		builder.Register<LoginFlow>(Lifetime.Singleton);
		builder.Register<SystemSeService>(Lifetime.Scoped);
		builder.RegisterEntryPoint<EntryBehavior>();
		builder.RegisterComponentInHierarchy<SlideFadeAnnounceDirection>();
	}
}
