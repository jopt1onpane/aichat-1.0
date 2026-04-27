using UnityEngine;
using UnityEngine.Pool;

public class PoolManager<T> : MonoBehaviour where T : MonoBehaviour, IPooledObject<T>
{
	[SerializeField]
	private T _pooledPrefab;

	protected ObjectPool<T> _objectPool;

	[SerializeField]
	private bool _collectionCheck = true;

	[SerializeField]
	private int _defaultCapacity = 32;

	[SerializeField]
	private int _maxSize = 100;

	protected virtual void NewObjectPool()
	{
		_objectPool = new ObjectPool<T>(Create, OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject, _collectionCheck, _defaultCapacity, _maxSize);
	}

	protected virtual T Create()
	{
		T val = Object.Instantiate(_pooledPrefab, base.transform.position, Quaternion.identity, base.transform);
		val.ObjectPool = _objectPool;
		return val;
	}

	protected virtual void OnGetFromPool(T pooledObject)
	{
		pooledObject.gameObject.SetActive(value: true);
	}

	protected virtual void OnReleaseToPool(T pooledObject)
	{
		pooledObject.gameObject.SetActive(value: false);
	}

	protected virtual void OnDestroyPooledObject(T pooledObject)
	{
		if (!(pooledObject == null))
		{
			Object.Destroy(pooledObject.gameObject);
		}
	}
}
