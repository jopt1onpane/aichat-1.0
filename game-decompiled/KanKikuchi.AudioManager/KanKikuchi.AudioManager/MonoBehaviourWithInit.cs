using Cysharp.Threading.Tasks;
using UnityEngine;

namespace KanKikuchi.AudioManager;

public class MonoBehaviourWithInit : MonoBehaviour
{
	private bool _initCalled;

	public bool IsInitialized { get; private set; }

	public void InitIfNeeded()
	{
		if (!_initCalled)
		{
			InitAsync().ContinueWith(() => IsInitialized = true).Forget();
			_initCalled = true;
		}
	}

	protected virtual UniTask InitAsync()
	{
		return UniTask.CompletedTask;
	}

	protected virtual void Awake()
	{
	}
}
