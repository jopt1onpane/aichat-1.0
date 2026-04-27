using UnityEngine.Pool;

public interface IPooledObject<T> where T : class
{
	IObjectPool<T> ObjectPool { set; }

	void Initialize();

	void Deactivate();
}
