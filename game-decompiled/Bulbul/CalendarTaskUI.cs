using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

public class CalendarTaskUI : MonoBehaviour
{
	[SerializeField]
	public TextMeshProUGUI TaskText;

	[SerializeField]
	public TextMeshProUGUI FocusTimeText;

	[SerializeField]
	public Button DeleteButton;

	public string UniqueKey;

	public void Setup()
	{
		UniqueKey = "";
		TaskText.SetText("");
		FocusTimeText.SetText("");
		DeleteButton.onClick.RemoveAllListeners();
	}
}
