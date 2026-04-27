using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bulbul;

public class SceneLoadHandle
{
	private AsyncOperation _mainOperation;

	public bool IsReady
	{
		get
		{
			if (_mainOperation != null)
			{
				return _mainOperation.progress >= 0.9f;
			}
			return false;
		}
	}

	public static SceneLoadHandle Load(string sceneName, bool autoActivate)
	{
		SceneLoadHandle sceneLoadHandle = new SceneLoadHandle();
		sceneLoadHandle.LoadAsync(sceneName, autoActivate).Forget();
		return sceneLoadHandle;
	}

	private async UniTask LoadAsync(string sceneName, bool autoActivate)
	{
		await SceneManager.LoadSceneAsync("Transition");
		await UniTask.NextFrame();
		_mainOperation = SceneManager.LoadSceneAsync(sceneName);
		_mainOperation.allowSceneActivation = autoActivate;
	}

	public void Activate()
	{
		_mainOperation.allowSceneActivation = true;
	}
}
