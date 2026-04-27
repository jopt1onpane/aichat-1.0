using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class SettingCreditView : MonoBehaviour
{
	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private FacilityAnimationBase viewAnimation;

	[SerializeField]
	private ScrollRect creditScroll;

	private Subject<Unit> onClickClose = new Subject<Unit>();

	public Observable<Unit> OnClickClose => onClickClose;

	private void OnDestroy()
	{
		onClickClose?.Dispose();
	}

	public void Activate()
	{
		if (viewAnimation == null)
		{
			base.gameObject.SetActive(value: true);
			return;
		}
		viewAnimation.Activate().Forget();
		LayoutRebuilder.ForceRebuildLayoutImmediate(creditScroll.content);
		creditScroll.verticalNormalizedPosition = 1f;
	}

	public void Deactivate()
	{
		if (viewAnimation == null)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			viewAnimation.Deactivate().Forget();
		}
	}

	public void Setup()
	{
		ObservableSubscribeExtensions.Subscribe(closeButton.OnClickAsObservable(), delegate
		{
			onClickClose.OnNext(Unit.Default);
		}).AddTo(this);
		viewAnimation.Setup();
	}
}
