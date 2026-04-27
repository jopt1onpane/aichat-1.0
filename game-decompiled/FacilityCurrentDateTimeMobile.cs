using Bulbul;
using UnityEngine;

public class FacilityCurrentDateTimeMobile : MonoBehaviour
{
	[SerializeField]
	private CurrentDateAndTimeUI[] _dateTimeUIs;

	public void Setup()
	{
		CurrentDateAndTimeUI[] dateTimeUIs = _dateTimeUIs;
		for (int i = 0; i < dateTimeUIs.Length; i++)
		{
			dateTimeUIs[i].Setup();
		}
	}
}
