using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

public class HabitPlayPauseButton : MonoBehaviour
{
	[SerializeField]
	private Button _button;

	[SerializeField]
	private Image _normalImage;

	[SerializeField]
	private Image _activeImage;

	[SerializeField]
	private Sprite _onNormalSprite;

	[SerializeField]
	private Sprite _onActiveSprite;

	[SerializeField]
	private Sprite _offNormalSprite;

	[SerializeField]
	private Sprite _offActiveSprite;

	public bool IsOn { get; private set; }

	public Observable<Unit> OnClick => _button.OnClickAsObservable();

	public void SetIsOn(bool isOn)
	{
		IsOn = isOn;
		_normalImage.sprite = (IsOn ? _onNormalSprite : _offNormalSprite);
		_activeImage.sprite = (IsOn ? _onActiveSprite : _offActiveSprite);
	}
}
