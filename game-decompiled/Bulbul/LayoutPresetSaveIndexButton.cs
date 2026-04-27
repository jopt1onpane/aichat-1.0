using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

public class LayoutPresetSaveIndexButton : MonoBehaviour
{
	[SerializeField]
	private Button _button;

	[SerializeField]
	private InteractableUI _interactableUI;

	[SerializeField]
	private GameObject _selectionArrow;

	[SerializeField]
	private TweenImgeAlphaBlinking saveCompletedEffectTween;

	public Button Button => _button;

	public Observable<Unit> OnClickAsObservable()
	{
		return _button.OnClickAsObservable();
	}

	public void Setup()
	{
		_interactableUI.Setup();
	}

	public void SetDisplay(bool showArrow, bool isSelected, bool canClick)
	{
		_selectionArrow.SetActive(showArrow);
		if (isSelected)
		{
			_interactableUI.ActivateUseUI();
		}
		else
		{
			_interactableUI.DeactivateUseUI();
		}
		SetInteractable(canClick);
	}

	public void PlaySaveCompletedEffect()
	{
		saveCompletedEffectTween.Play(MyTweenPlayType.Normal);
	}

	private void SetInteractable(bool enable)
	{
		_button.interactable = enable;
		_interactableUI.enabled = enable;
	}
}
