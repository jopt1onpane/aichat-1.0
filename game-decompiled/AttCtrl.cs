using Cysharp.Threading.Tasks;
using UnityEngine;

public class AttCtrl : MonoBehaviour
{
	private async void Start()
	{
		await RequestAttAsync();
	}

	private async UniTask RequestAttAsync()
	{
	}
}
