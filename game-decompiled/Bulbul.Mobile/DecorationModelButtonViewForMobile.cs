using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class DecorationModelButtonViewForMobile : MonoBehaviour
{
	[SerializeField]
	private InteractableUI _interactableUI;

	[SerializeField]
	private Button _button;

	[SerializeField]
	private Image _baseIcon;

	[SerializeField]
	private Image[] _activeIcons;

	public Observable<Unit> OnClickButton => _button.OnClickAsObservable();

	public InteractableUI InteractableUI => _interactableUI;

	public void SetIcon(Sprite baseIcon, Sprite activeIcon)
	{
		_baseIcon.sprite = baseIcon;
		Image[] activeIcons = _activeIcons;
		for (int i = 0; i < activeIcons.Length; i++)
		{
			activeIcons[i].sprite = activeIcon;
		}
	}

	public void SetSelected(bool isSelected)
	{
		InteractableUI.SetUseUI(isSelected);
	}

	public void Activate()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Deactivate()
	{
		base.gameObject.SetActive(value: false);
	}
}
