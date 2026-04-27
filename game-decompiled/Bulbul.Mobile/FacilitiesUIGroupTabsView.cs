using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul.Mobile;

public class FacilitiesUIGroupTabsView : MonoBehaviour
{
	[Inject]
	private SystemSeService _systemSeService;

	[SerializeField]
	private InteractiveTabIconUI[] _tabs;

	[SerializeField]
	private FacilitiesUIGroupContentsView _groupContentsView;

	private InteractiveTabIconUI _currentTab;

	public void Setup()
	{
		int num = 0;
		_groupContentsView.Setup();
		InteractiveTabIconUI[] tabs = _tabs;
		foreach (InteractiveTabIconUI tab in tabs)
		{
			tab.Setup();
			FacilitiesUIGroupContentsView.ContentType type = (FacilitiesUIGroupContentsView.ContentType)num;
			ObservableExtensions.Subscribe(tab.GetComponent<Button>().OnClickAsObservable(), delegate
			{
				if (!_groupContentsView.IsChanging)
				{
					_systemSeService?.PlaySelect();
					if (ChangeTab(tab))
					{
						_groupContentsView.ChangeContents(type);
					}
				}
			}).AddTo(this);
			num++;
		}
	}

	private bool ChangeTab(InteractiveTabIconUI tab)
	{
		if (_currentTab != null && _currentTab == tab)
		{
			return false;
		}
		tab.ActivateUseUI();
		if (_currentTab != null)
		{
			_currentTab.DeactivateUseUI();
		}
		_currentTab = tab;
		return true;
	}

	public void ChangeTab(FacilitiesUIGroupContentsView.ContentType contentType)
	{
		InteractiveTabIconUI[] tabs = _tabs;
		for (int i = 0; i < tabs.Length; i++)
		{
			tabs[i].DeactivateUseUI();
		}
		_tabs[(int)contentType].ActivateUseUI();
		_currentTab = _tabs[(int)contentType];
	}
}
