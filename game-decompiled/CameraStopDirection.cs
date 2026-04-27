using Bulbul;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class CameraStopDirection : MonoBehaviour
{
	[Inject]
	private IDirectionServiceUIProvider directionServiceUIProvider;

	[SerializeField]
	private Camera _mainCamera;

	[SerializeField]
	private Camera _stopTextureRenderCamera;

	private Canvas _uiCanvas;

	private RawImage _screenStopRawImage;

	public void Setup()
	{
		_uiCanvas = directionServiceUIProvider.UICanvas;
		_screenStopRawImage = directionServiceUIProvider.ScreenStopRawImage;
		_stopTextureRenderCamera.gameObject.SetActive(value: false);
		_screenStopRawImage.gameObject.SetActive(value: false);
	}

	public async UniTask Play()
	{
		_stopTextureRenderCamera.gameObject.SetActive(value: true);
		_screenStopRawImage.gameObject.SetActive(value: true);
		await UniTask.NextFrame();
		_stopTextureRenderCamera.Render();
		_mainCamera.enabled = false;
		_uiCanvas.enabled = false;
	}

	public void Stop()
	{
		_stopTextureRenderCamera.gameObject.SetActive(value: false);
		_screenStopRawImage.gameObject.SetActive(value: false);
		_uiCanvas.enabled = true;
		_mainCamera.enabled = true;
	}

	public void EndTidying()
	{
		Stop();
	}
}
