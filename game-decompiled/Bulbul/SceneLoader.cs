using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Bulbul;

public static class SceneLoader
{
	public static SceneLoadHandle LoadSceneAsync(string sceneName, bool autoActivate)
	{
		return SceneLoadHandle.Load(sceneName, autoActivate);
	}

	public static async UniTask LoadSceneAdditiveAsync(string sceneName)
	{
		await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
	}
}
