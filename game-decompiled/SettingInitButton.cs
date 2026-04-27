using Bulbul;
using DG.Tweening;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingInitButton : MonoBehaviour
{
	[SerializeField]
	private Button _button;

	[SerializeField]
	private InteractableUI _interactableUI;

	[SerializeField]
	private HoldButtonAnimation _holdAnim;

	[SerializeField]
	private TMP_Text _text;

	public Observable<Unit> OnClickButton => _button.OnClickAsObservable();

	public void Setup()
	{
		_interactableUI.Setup();
		_holdAnim.Setup();
	}

	public void Activate()
	{
		_button.interactable = true;
		_interactableUI.enabled = true;
		_holdAnim.enabled = true;
		_text.DOColor(new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), 0.2f);
	}

	public void Deactivate()
	{
		_button.interactable = false;
		_interactableUI.enabled = false;
		_interactableUI.DeactivateAllUI();
		_holdAnim.enabled = false;
		_text.DOColor(new Color32(128, 128, 128, 128), 0.2f);
	}
}
