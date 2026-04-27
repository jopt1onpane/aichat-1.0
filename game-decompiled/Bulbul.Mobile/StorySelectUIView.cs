using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class StorySelectUIView : MonoBehaviour, IStorySelectUI
{
	[SerializeField]
	private FacilityCommonActivateAnimationMobile _activator;

	[SerializeField]
	private StoryListView _storyListView;

	[SerializeField]
	private Button _closeButton;

	private bool _isPurchasedStandardEdition;

	Observable<Unit> IStorySelectUI.OnClickTagChangeToMainButton => _storyListView.OnClickTagChangeToMainButton;

	Observable<Unit> IStorySelectUI.OnClickTagChangeToSpecialButton => _storyListView.OnClickTagChangeToSpecialButton;

	Observable<Unit> IStorySelectUI.OnClickCloseButton => _closeButton.OnClickAsObservable();

	void IStorySelectUI.Activate()
	{
	}

	void IStorySelectUI.Deactivate()
	{
		_activator.Deactivate();
	}

	void IStorySelectUI.Setup(FacilityStory facilityStory)
	{
	}
}
