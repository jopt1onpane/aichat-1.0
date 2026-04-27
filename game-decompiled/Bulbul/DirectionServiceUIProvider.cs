using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

public class DirectionServiceUIProvider : MonoBehaviour, IDirectionServiceUIProvider
{
	[SerializeField]
	private Animator _screenSharing;

	[SerializeField]
	private Canvas _uiCanvas;

	[SerializeField]
	private RawImage _screenStopRawImage;

	[SerializeField]
	private GameObject _connectionLost;

	public Animator ScreenSharing => _screenSharing;

	public Canvas UICanvas => _uiCanvas;

	public RawImage ScreenStopRawImage => _screenStopRawImage;

	public GameObject ConnectionLost => _connectionLost;
}
