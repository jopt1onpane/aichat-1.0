using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Bulbul;

public class PlayerPointView : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _pointText;

	private Tween _pointTween;

	public void SetPoint(int point, bool withAnimation)
	{
		_pointTween?.Kill();
		if (withAnimation)
		{
			if (!int.TryParse(_pointText.text, out var result))
			{
				result = point;
			}
			_pointTween = DOVirtual.Int(result, point, 0.5f, delegate(int val)
			{
				_pointText.SetText("{0}", val);
			});
		}
		else
		{
			_pointText.text = point.ToString();
		}
	}
}
