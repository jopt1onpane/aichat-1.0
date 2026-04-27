using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class PurchaseButtonWithPoint : MonoBehaviour
{
	[SerializeField]
	private Image _shopButtonBgImage;

	[SerializeField]
	private Sprite _shopButtonBgInactiveSprite;

	[SerializeField]
	private TMP_Text _priceText;

	[SerializeField]
	private Button _button;

	private Sprite _shopButtonBgActiveSprite;

	private Color _priceTextDefaultColor;

	public Observable<Unit> OnClick => _button.OnClickAsObservable();

	public void Initialize()
	{
		_shopButtonBgActiveSprite = _shopButtonBgImage.sprite;
		_priceTextDefaultColor = _priceText.color;
	}

	public void Setup(int price, bool hasEnoughPoints)
	{
		_priceText.text = price.ToString();
		_priceText.color = (hasEnoughPoints ? _priceTextDefaultColor : MyDefine.CommonRed);
		_shopButtonBgImage.sprite = (hasEnoughPoints ? _shopButtonBgActiveSprite : _shopButtonBgInactiveSprite);
	}

	public void SetButtonInteractable(bool interactable)
	{
		_button.interactable = interactable;
	}
}
