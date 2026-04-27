using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

[RequireComponent(typeof(ButtonEventObservable))]
public class MusicTagListButton : MonoBehaviour
{
	[SerializeField]
	private Image checkBoxImage;

	[SerializeField]
	private Sprite activeCheckBoxSprite;

	[SerializeField]
	private Sprite inactiveCheckBoxSprite;

	[SerializeField]
	private Sprite mouseoverInactiveCheckBoxSprite;

	[SerializeField]
	private Image backImage;

	[SerializeField]
	private Sprite backSprite;

	[SerializeField]
	private Sprite mouseoverBackSprite;

	private ButtonEventObservable _subject;

	private bool isMouseOver;

	[field: SerializeField]
	public AudioTag Tag { get; private set; }

	private ButtonEventObservable subject
	{
		get
		{
			if (!(_subject != null))
			{
				return _subject = GetComponent<ButtonEventObservable>();
			}
			return _subject;
		}
	}

	public Observable<MusicTagListButton> OnClick => subject.OnClick.Select(this, (Unit _, MusicTagListButton @this) => @this);

	private void Start()
	{
		ObservableSubscribeExtensions.Subscribe(subject.OnPointerEnter, delegate
		{
			isMouseOver = true;
			backImage.sprite = mouseoverBackSprite;
			SetCheck(checkBoxImage.sprite == activeCheckBoxSprite);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(subject.OnPointerExit, delegate
		{
			isMouseOver = false;
			backImage.sprite = backSprite;
			SetCheck(checkBoxImage.sprite == activeCheckBoxSprite);
		}).AddTo(this);
	}

	public void SetCheck(bool isActive)
	{
		if (isActive)
		{
			checkBoxImage.sprite = activeCheckBoxSprite;
		}
		else
		{
			checkBoxImage.sprite = (isMouseOver ? mouseoverInactiveCheckBoxSprite : inactiveCheckBoxSprite);
		}
	}
}
