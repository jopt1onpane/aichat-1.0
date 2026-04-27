using UnityEngine;

namespace Bulbul;

[DisallowMultipleComponent]
public class TooltipTarget : MonoBehaviour
{
	[SerializeField]
	private TooltipData _data;

	public TooltipData Data => _data;

	private void OnEnable()
	{
		TooltipTargets.Add(this);
	}

	private void OnDisable()
	{
		TooltipTargets.Remove(this);
	}

	public void SetContentLocalizeID(string contentLocalizeID)
	{
		_data.ContentLocalizeID = contentLocalizeID;
	}

	public void SetLocalizeConverter(ILocalizeConverter converter)
	{
		_data.LocalizeConverter = converter;
	}
}
