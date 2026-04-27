using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class EnvironmentTabView : MonoBehaviour
{
	[SerializeField]
	private InteractableUI _interactableUI;

	public Observable<Unit> OnClick => _interactableUI.GetComponent<Button>().OnClickAsObservable();

	public void SetSelected(bool isSelected)
	{
		_interactableUI.SetUseUI(isSelected);
	}
}
