using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul.Mobile;

public class FacilitiesUIGroupWindowView : MonoBehaviour
{
	[Inject]
	private SystemSeService _systemSeService;

	[SerializeField]
	private FacilityCommonActivateAnimationMobile _activator;

	[SerializeField]
	private FacilitiesUIGroupTabsView _tabsView;

	[SerializeField]
	private FacilitiesUIGroupContentsView _contentsView;

	[SerializeField]
	private Button _closeButton;

	public Observable<Unit> OnClickCloseButton => _closeButton.OnClickAsObservable();

	public void Setup()
	{
		_activator.Setup();
		_tabsView.Setup();
	}

	public void Activate()
	{
		_activator.Activate().Forget();
	}

	public void Deactivate()
	{
		_activator.Deactivate().Forget();
	}

	public void SetTab(FacilitiesUIGroupContentsView.ContentType type)
	{
		_tabsView.ChangeTab(type);
		_contentsView.ChangeContentsImmediate(type);
	}
}
