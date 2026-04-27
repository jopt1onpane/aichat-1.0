using System.Globalization;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace NestopiSystem.DIContainers;

public class ProjectLifetimeScope : LifetimeScope
{
	[SerializeField]
	private CursorService cursorService;

	private static ProjectLifetimeScope inst;

	public static GameObject GameObject => inst.gameObject;

	private static ProjectLifetimeScope Inst => GetOrCreate();

	protected override void Configure(IContainerBuilder builder)
	{
		CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
		MonoRegister[] components = GetComponents<MonoRegister>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].Register(builder);
		}
		builder.Register<MusicService>(Lifetime.Singleton);
		builder.RegisterComponentInHierarchy<LoadDirectionService>();
		builder.RegisterInstance(cursorService);
	}

	public static T Resolve<T>()
	{
		return Inst.Container.Resolve<T>();
	}

	public static bool TryResolve<T>(out T resolved)
	{
		return Inst.Container.TryResolve<T>(out resolved);
	}

	public static void Inject(object o)
	{
		Inst.Container.Inject(o);
	}

	public static ProjectLifetimeScope GetOrCreate()
	{
		return inst;
	}

	public static void SetInstance(ProjectLifetimeScope instance)
	{
		if (inst != null && inst != instance)
		{
			Debug.LogWarning("ProjectLifetimeScopeが複数存在します");
			Object.Destroy(instance.gameObject);
		}
		else
		{
			inst = instance;
		}
	}

	protected override void Awake()
	{
		SetInstance(this);
		if (!(this == null))
		{
			base.Awake();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (this == inst)
		{
			inst = null;
		}
	}

	public static T ResolveInstantiate<T>(T prefab) where T : Component
	{
		return Inst.Container.Instantiate(prefab, prefab.transform.position, prefab.transform.rotation);
	}

	public static T ResolveInstantiate<T>(T prefab, Transform parent, bool worldPositionStays = false) where T : Component
	{
		return Inst.Container.Instantiate(prefab, parent, worldPositionStays);
	}

	public static T ResolveInstantiate<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
	{
		return Inst.Container.Instantiate(prefab, position, rotation);
	}

	public static T ResolveInstantiate<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Component
	{
		return Inst.Container.Instantiate(prefab, position, rotation, parent);
	}

	public static GameObject ResolveInstantiate(GameObject prefab)
	{
		return inst.Container.Instantiate(prefab);
	}

	public static GameObject ResolveInstantiate(GameObject prefab, Transform parent, bool worldPositionStays = false)
	{
		return inst.Container.Instantiate(prefab, parent, worldPositionStays);
	}

	public static GameObject ResolveInstantiate(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		return inst.Container.Instantiate(prefab, position, rotation);
	}

	public static GameObject ResolveInstantiate(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
	{
		return inst.Container.Instantiate(prefab, position, rotation, parent);
	}
}
