using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class PoorConnectionView : MonoBehaviour
{
	[Inject]
	private WebApiPulser webApiPulser;

	[SerializeField]
	private RectTransform parent;

	[SerializeField]
	private PoorConnectionViewAnimation viewAnimation;

	private bool lastUploadSucessFlag = true;

	public bool IsActive => parent.gameObject.activeSelf;

	private void Start()
	{
		webApiPulser.LastSaveDataUploadSuccess.Subscribe(this, delegate(bool success, PoorConnectionView @this)
		{
			bool flag = @this.lastUploadSucessFlag != success;
			@this.lastUploadSucessFlag = success;
			@this.parent.gameObject.SetActive(!success);
			if (!success && flag && !success && @this.viewAnimation != null)
			{
				@this.viewAnimation.PlayActivationAnimation().Forget();
			}
		}).AddTo(this);
	}
}
