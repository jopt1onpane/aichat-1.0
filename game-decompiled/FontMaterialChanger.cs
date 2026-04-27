using System;
using Bulbul;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

public class FontMaterialChanger : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	[Inject]
	private FontSupplier _fontSupplier;

	[Inject]
	private LanguageSupplier _languageSupplier;

	[SerializeField]
	private TextMeshProUGUI _text;

	[SerializeField]
	[Header("通常時のマテリアル")]
	private FontMaterialType _normalMaterial;

	[SerializeField]
	[Header("マウスオーバー時のマテリアル")]
	private FontMaterialType _mouseOverMaterial;

	[SerializeField]
	[Header("使用中のマテリアル")]
	private FontMaterialType _onUseMaterial;

	private bool _isUsing;

	private bool _isMouseOver;

	private bool _isFinishSetup;

	private IDisposable _disposable;

	public void Awake()
	{
		if (!_isFinishSetup)
		{
			Setup();
		}
		if (DevicePlatform.Steam.IsMobile())
		{
			return;
		}
		_disposable = ObservableSubscribeExtensions.Subscribe(Observable.Interval(TimeSpan.FromSeconds(0.20000000298023224)), delegate
		{
			if (InputController.Instance.CurrentFrameEventSystemRaycastHitObject?.GetComponent<FontMaterialChanger>() == this)
			{
				if (!_isMouseOver)
				{
					PointerEnter();
				}
			}
			else if (_isMouseOver)
			{
				PointerExit();
			}
		});
	}

	public void Setup()
	{
		if (!_isFinishSetup)
		{
			SetupCore();
		}
	}

	protected virtual void SetupCore()
	{
		if ((object)_fontSupplier == null)
		{
			_fontSupplier = RoomLifetimeScope.Resolve<FontSupplier>();
		}
		if (_languageSupplier == null)
		{
			_languageSupplier = RoomLifetimeScope.Resolve<LanguageSupplier>();
		}
		_isFinishSetup = true;
	}

	public void ActivateUse()
	{
		_isUsing = true;
		_text.fontMaterial = GetMaterial(_onUseMaterial);
	}

	public void DeactivateUse()
	{
		_isUsing = false;
		_text.fontMaterial = GetMaterial(_normalMaterial);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!DevicePlatform.Steam.IsMobile())
		{
			PointerEnter();
		}
	}

	private void PointerEnter()
	{
		if (!(this == null) && base.enabled)
		{
			_isMouseOver = true;
			if (!_isUsing)
			{
				ActivateMouseOver();
			}
		}
	}

	private void ActivateMouseOver()
	{
		_text.fontMaterial = GetMaterial(_mouseOverMaterial);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		PointerExit();
	}

	private void PointerExit()
	{
		_isMouseOver = false;
		if (!_isUsing)
		{
			DeactivateMouseOver();
		}
	}

	private void DeactivateMouseOver()
	{
		_text.fontMaterial = GetMaterial(_normalMaterial);
	}

	private Material GetMaterial(FontMaterialType materialType)
	{
		return materialType switch
		{
			FontMaterialType.Normal => _fontSupplier.GetFontNormalMaterial(_languageSupplier.Language.CurrentValue), 
			FontMaterialType.Gradation => _fontSupplier.GetFontGradationMaterial(_languageSupplier.Language.CurrentValue), 
			_ => null, 
		};
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (DevicePlatform.Steam.IsMobile())
		{
			PointerEnter();
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (DevicePlatform.Steam.IsMobile())
		{
			PointerExit();
		}
	}
}
