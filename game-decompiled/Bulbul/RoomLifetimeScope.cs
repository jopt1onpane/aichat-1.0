using GUPS.Obfuscator.Attribute;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Bulbul;

[DoNotRename]
public class RoomLifetimeScope : LifetimeScope
{
	[SerializeField]
	private GameObject _pcPlatform;

	[SerializeField]
	private GameObject _mobilePlatform;

	private static RoomLifetimeScope inst;

	private static RoomLifetimeScope Inst => GetOrCreate();

	protected override void Configure(IContainerBuilder builder)
	{
		GameObject[] injectObjects = _pcPlatform.GetComponent<IAutoInjectObjectsProvider>().InjectObjects;
		if (_mobilePlatform != null)
		{
			Debug.LogError("MobilePlatformオブジェクトがビルドに含まれています");
			Object.DestroyImmediate(_mobilePlatform);
			_mobilePlatform = null;
		}
		autoInjectGameObjects.AddRange(injectObjects);
		GetComponent<RoomRegister>().Register(builder);
	}

	public static T Resolve<T>()
	{
		return Inst.Container.Resolve<T>();
	}

	public static RoomLifetimeScope GetOrCreate()
	{
		if (inst == null)
		{
			if (Resources.Load<VContainerSettings>("VContainerSettings").GetOrCreateRootLifetimeScopeInstance() is RoomLifetimeScope roomLifetimeScope)
			{
				return inst = roomLifetimeScope;
			}
			inst = Object.FindObjectOfType<RoomLifetimeScope>();
			inst = ((inst == null) ? Object.Instantiate(Resources.Load<RoomLifetimeScope>("RoomLifetimeScope")) : inst);
			Object.DontDestroyOnLoad(inst.gameObject);
		}
		return inst;
	}

	protected override void Awake()
	{
		if (inst == null)
		{
			inst = this;
			base.Awake();
		}
		else
		{
			Object.Destroy(base.gameObject);
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
