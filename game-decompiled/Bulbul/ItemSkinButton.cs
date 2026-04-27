using System;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

public class ItemSkinButton : MonoBehaviour
{
	public enum DisplayType
	{
		Open,
		Locked,
		Purchase
	}

	[SerializeField]
	private Image _upperColorImage;

	[SerializeField]
	private Image _lowerColorImage;

	[SerializeField]
	private CanvasGroup _mainButtonCanvasGroup;

	[SerializeField]
	private CanvasGroup _shopButtonCanvasGroup;

	[SerializeField]
	private Button _mainButton;

	[SerializeField]
	private Button _shopButton;

	[SerializeField]
	private GameObject _newIcon;

	[SerializeField]
	private InteractableUI _interactableUI;

	public void SetColors(DecorationSkinMasterData decorationMasterData)
	{
		_upperColorImage.color = decorationMasterData.IconColor1;
		_lowerColorImage.color = decorationMasterData.IconColor2;
	}

	public void SetDisplayType(DisplayType displayType)
	{
		switch (displayType)
		{
		case DisplayType.Open:
			base.gameObject.SetActive(value: true);
			_mainButtonCanvasGroup.blocksRaycasts = true;
			_shopButtonCanvasGroup.blocksRaycasts = false;
			_shopButton.gameObject.SetActive(value: false);
			break;
		case DisplayType.Locked:
			base.gameObject.SetActive(value: false);
			break;
		case DisplayType.Purchase:
			base.gameObject.SetActive(value: true);
			_mainButtonCanvasGroup.blocksRaycasts = false;
			_shopButtonCanvasGroup.blocksRaycasts = true;
			_shopButton.gameObject.SetActive(value: true);
			break;
		}
	}

	public void SetUsing(bool isUsing)
	{
		if (isUsing)
		{
			_interactableUI.ActivateUseUI();
		}
		else
		{
			_interactableUI.DeactivateUseUI();
		}
	}

	public void SetNew(bool isNew)
	{
		_newIcon.SetActive(isNew);
	}

	public void SetOnClick(Action onClick)
	{
		_mainButton.onClick.RemoveAllListeners();
		_shopButton.onClick.RemoveAllListeners();
		_mainButton.onClick.AddListener(delegate
		{
			onClick();
		});
		_shopButton.onClick.AddListener(delegate
		{
			onClick();
		});
	}

	public void Reset()
	{
		_mainButton.onClick.RemoveAllListeners();
		_shopButton.onClick.RemoveAllListeners();
	}
}
