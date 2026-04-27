using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

[RequireComponent(typeof(Button))]
public class SentenceSelectionButton : MonoBehaviour
{
	[SerializeField]
	private Button _button;

	[SerializeField]
	private TextMeshProUGUI _selectionText;

	public Button Button => _button;

	public TextMeshProUGUI SelectionText => _selectionText;

	private void Start()
	{
	}

	public void DisableButton()
	{
		base.gameObject.SetActive(value: false);
		_selectionText.SetText("");
	}

	public void RemoveAllListeners()
	{
		_button.onClick.RemoveAllListeners();
	}

	public void ColorToGrayOut()
	{
		_selectionText.color = new Color32(77, 72, 72, 81);
	}

	public void ColorToNormal()
	{
		_selectionText.color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
	}
}
