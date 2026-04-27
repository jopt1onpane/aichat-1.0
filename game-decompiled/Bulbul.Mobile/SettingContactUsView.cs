using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class SettingContactUsView : MonoBehaviour
{
	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private FacilitySettingActivateAnimationMobile _viewAnimation;

	[SerializeField]
	private Button _faqButton;

	[SerializeField]
	private Button _contactUsButton;

	public Observable<Unit> OnClickClose => _closeButton.OnClickAsObservable();

	public Observable<Unit> OnClickFAQ => _faqButton.OnClickAsObservable();

	public Observable<Unit> OnClickContactUs => _contactUsButton.OnClickAsObservable();

	public void Setup()
	{
	}

	public void Activate()
	{
		if (_viewAnimation == null)
		{
			base.gameObject.SetActive(value: true);
		}
		else
		{
			_viewAnimation.Activate().Forget();
		}
	}

	public void Deactivate()
	{
		if (_viewAnimation == null)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			_viewAnimation.Deactivate().Forget();
		}
	}
}
