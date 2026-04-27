using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using NestopiSystem;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul.Mobile;

public class EnvironmentTimeSelectorUI : MonoBehaviour
{
	[Inject]
	private SystemSeService _systemSeService;

	[SerializeField]
	private float _normalClosedWidth;

	[SerializeField]
	private float _autoModeClosedWidth;

	[SerializeField]
	private Vector2 _normalCurrentIconShrinkAncPos;

	[SerializeField]
	private Vector2 _autoModeCurrentIconShrinkAncPos;

	[SerializeField]
	private EnvironmentTimeItemView _itemOriginal;

	[SerializeField]
	private Button _showHideRootButton;

	[SerializeField]
	private Button _showButton;

	[SerializeField]
	private Button _hideButton;

	[SerializeField]
	private Image _arrowImage;

	[SerializeField]
	private Image _currentIcon;

	[Header("オート設定(歯車)")]
	[SerializeField]
	private GameObject _autoSettingObj;

	[SerializeField]
	private InteractableUI[] _autoSettingInteractableUIs;

	[SerializeField]
	private CanvasGroup _autoSettingCanvasGroup;

	[SerializeField]
	private Button[] _autoSettingButtons;

	[Header("オートボタン")]
	[SerializeField]
	private EnvironmentTimeAutoItemView _itemAuto;

	[Header("現在設定系 _currentIcon周り")]
	[SerializeField]
	[Header("currentIcon or Textになる")]
	private TextMeshProUGUI _currentAutoText;

	[SerializeField]
	private GameObject _currentAutoSettingObj;

	[SerializeField]
	private RectTransform _bgRectTransform;

	[SerializeField]
	private RectTransform _buttonParent;

	[SerializeField]
	private CanvasGroup _buttonParentCanvasGroup;

	[SerializeField]
	private float _expandDuration = 0.2f;

	[SerializeField]
	private float _fadeInDuration = 0.1f;

	[SerializeField]
	private float _fadeInDelay = 0.15f;

	private Dictionary<EnvironmentType, EnvironmentTimeItemView> _itemDic;

	private readonly Subject<EnvironmentType> _onClickItem = new Subject<EnvironmentType>();

	private readonly Subject<Unit> _onClickAutoItem = new Subject<Unit>();

	private readonly Subject<Unit> _onClickAutoSetting = new Subject<Unit>();

	private bool _isExpanded;

	private EnvironmentType _currentEnvironmentType;

	private bool _isLayoutDirty;

	private float _shrinkSizeX;

	private float _expandSizeX;

	private Vector2 _currentIconShrinkAnchorPos;

	private Tween _showTween;

	private Tween _showAutoObjTweem;

	private float _curClosedWidth;

	private RectTransform _autoSettingRectTrans;

	public Observable<EnvironmentType> OnClickItem => _onClickItem;

	public Observable<Unit> OnClickAutoSetting => _onClickAutoSetting;

	public Observable<Unit> OnClickAutoItem => _onClickAutoItem;

	public bool IsExpanded => _isExpanded;

	public bool EnabledAuto { get; set; }

	public bool ActiveAuto { get; set; }

	public void Initialize(IEnumerable<EnvironmentTimeItemModel> itemModels)
	{
		_autoSettingRectTrans = _autoSettingObj.transform as RectTransform;
		_itemOriginal.gameObject.SetActive(value: false);
		_itemAuto.OnClickAuto.Subscribe(delegate(Unit _)
		{
			_onClickAutoItem.OnNext(_);
		}).AddTo(this);
		_autoSettingButtons.Select((Button _) => _.OnClickAsObservable()).Merge().Subscribe(delegate(Unit _)
		{
			_onClickAutoSetting.OnNext(_);
		})
			.AddTo(this);
		SetActiveAutoButton(active: false);
		SetActiveAutoSettingButton(active: false);
		SetActiveCurrentAutoSettingObj(active: false);
		_itemDic = new Dictionary<EnvironmentType, EnvironmentTimeItemView>();
		foreach (EnvironmentTimeItemModel itemModel in itemModels)
		{
			EnvironmentTimeItemView item = Object.Instantiate(_itemOriginal, _itemOriginal.transform.parent);
			item.gameObject.SetActive(value: true);
			item.Initialize(itemModel);
			ObservableSubscribeExtensions.Subscribe(item.OnClick, delegate
			{
				_onClickItem.OnNext(item.EnvironmentType);
			}).AddTo(this);
			_itemDic.Add(itemModel.EnvironmentType, item);
		}
		_currentEnvironmentType = itemModels.First((EnvironmentTimeItemModel x) => x.IsUse).EnvironmentType;
		_currentIcon.sprite = _itemDic[_currentEnvironmentType].ActiveSprite;
		_currentIconShrinkAnchorPos = _normalCurrentIconShrinkAncPos;
		_shrinkSizeX = _bgRectTransform.sizeDelta.x;
		_curClosedWidth = _normalClosedWidth;
		_isLayoutDirty = true;
		ShrinkImmediate();
		ObservableSubscribeExtensions.Subscribe(_showButton.OnClickAsObservable(), delegate
		{
			if (_isExpanded)
			{
				_systemSeService.PlayPulldownClose();
				Shrink();
			}
			else
			{
				_systemSeService.PlayPulldownOpen();
				Expand();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_hideButton.OnClickAsObservable(), delegate
		{
			if (_isExpanded)
			{
				_systemSeService.PlayPulldownClose();
				Shrink();
			}
			else
			{
				_systemSeService.PlayPulldownOpen();
				Expand();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_showHideRootButton.OnClickAsObservable(), delegate
		{
			if (_isExpanded)
			{
				_systemSeService.PlayPulldownClose();
				Shrink();
			}
			else
			{
				_systemSeService.PlayPulldownOpen();
				Expand();
			}
		}).AddTo(this);
	}

	public void ReapplyModel(EnvironmentType environmentType)
	{
		if (_itemDic.TryGetValue(environmentType, out var value))
		{
			value.ReapplyModel(ActiveAuto);
			if (value.IsUse)
			{
				_currentEnvironmentType = environmentType;
				_currentIcon.sprite = value.ActiveSprite;
			}
		}
		UpdateCurrentIconDisplayMode();
	}

	private void UpdateCurrentIconDisplayMode()
	{
		_currentIcon.enabled = !ActiveAuto;
		_currentAutoText.enabled = ActiveAuto;
	}

	private void Expand(bool immediate = false)
	{
		_showAutoObjTweem?.Complete();
		_showTween?.Kill();
		if (_isLayoutDirty)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(_buttonParent);
			_expandSizeX = _buttonParent.sizeDelta.x;
			_isLayoutDirty = false;
		}
		_bgRectTransform.sizeDelta = _bgRectTransform.sizeDelta.WithX(_curClosedWidth);
		_buttonParentCanvasGroup.alpha = 0f;
		_currentIcon.rectTransform.anchoredPosition = _currentIconShrinkAnchorPos;
		Vector3 currentIconExpandLocalPos = GetCurrentIconExpandLocalPos();
		_showTween = DOTween.Sequence().Append(_bgRectTransform.DOSizeDelta(_bgRectTransform.sizeDelta.WithX(_expandSizeX), _expandDuration)).Join(_currentIcon.rectTransform.DOLocalMove(currentIconExpandLocalPos, _expandDuration))
			.Join(_buttonParentCanvasGroup.DOFade(1f, _fadeInDuration).SetDelay(_fadeInDelay))
			.OnComplete(delegate
			{
				SetActiveCurrentAutoSettingObj(active: false);
				_buttonParentCanvasGroup.blocksRaycasts = true;
				_showButton.gameObject.SetActive(value: false);
				_hideButton.gameObject.SetActive(value: true);
				_currentIcon.gameObject.SetActive(value: false);
			});
		if (immediate)
		{
			_showTween.Complete();
		}
		_isExpanded = true;
	}

	private void Shrink(bool immediate = false)
	{
		_showAutoObjTweem?.Complete();
		_showTween?.Kill();
		_buttonParentCanvasGroup.blocksRaycasts = false;
		_currentIcon.gameObject.SetActive(value: true);
		_currentIcon.rectTransform.localPosition = GetCurrentIconExpandLocalPos();
		SetActiveCurrentAutoSettingObj(ActiveAuto);
		_showTween = DOTween.Sequence().Append(_buttonParentCanvasGroup.DOFade(0f, _fadeInDuration)).Join(_bgRectTransform.DOSizeDelta(_bgRectTransform.sizeDelta.WithX(_curClosedWidth), _expandDuration))
			.Join(_currentIcon.rectTransform.DOAnchorPos(_currentIconShrinkAnchorPos, _expandDuration))
			.OnComplete(delegate
			{
				_showButton.gameObject.SetActive(value: true);
				_hideButton.gameObject.SetActive(value: false);
			});
		if (immediate)
		{
			_showTween.Complete();
		}
		_isExpanded = false;
	}

	private Vector3 GetCurrentIconExpandLocalPos()
	{
		if (ActiveAuto)
		{
			return base.transform.InverseTransformPoint(_itemAuto.transform.position);
		}
		return base.transform.InverseTransformPoint(_itemDic[_currentEnvironmentType].transform.position);
	}

	public void SetActiveAutoButton(bool active)
	{
		if (_itemAuto.gameObject.activeSelf != active)
		{
			_itemAuto.gameObject.SetActive(active);
			_isLayoutDirty = true;
		}
	}

	public void SetActiveAutoSettingButton(bool active)
	{
		if (_autoSettingObj.activeSelf != active)
		{
			_autoSettingObj.SetActive(active);
			_isLayoutDirty = true;
		}
	}

	public void SetActiveCurrentAutoSettingObj(bool active)
	{
		if (_currentAutoSettingObj.activeSelf != active)
		{
			_currentAutoSettingObj.SetActive(active);
		}
	}

	public void SetUseAuto(bool isUse)
	{
		_itemAuto.SetUse(isUse);
	}

	public void PlayAutoObjExpandShrinkAnim(bool active)
	{
		if (active)
		{
			ExpandAutoObj();
		}
		else
		{
			ShrinkAutoObj();
		}
	}

	private void ExpandAutoObj(bool immediate = false)
	{
		_showAutoObjTweem?.Kill();
		_buttonParentCanvasGroup.blocksRaycasts = false;
		SetActiveAutoSettingButton(active: true);
		_autoSettingRectTrans.sizeDelta = _autoSettingRectTrans.sizeDelta.WithX(100f);
		LayoutRebuilder.ForceRebuildLayoutImmediate(_buttonParent);
		_expandSizeX = _buttonParent.sizeDelta.x;
		_autoSettingRectTrans.sizeDelta = _autoSettingRectTrans.sizeDelta.WithX(0f);
		_autoSettingCanvasGroup.alpha = 0f;
		_showAutoObjTweem = DOTween.Sequence().Join(_autoSettingCanvasGroup.DOFade(1f, _fadeInDuration)).Join(_autoSettingRectTrans.DOSizeDelta(_autoSettingRectTrans.sizeDelta.WithX(100f), _expandDuration))
			.Join(_bgRectTransform.DOSizeDelta(_bgRectTransform.sizeDelta.WithX(_expandSizeX), _expandDuration))
			.Join(_currentIcon.rectTransform.DOAnchorPos(_currentIconShrinkAnchorPos, _expandDuration))
			.OnComplete(delegate
			{
				_currentIconShrinkAnchorPos = _autoModeCurrentIconShrinkAncPos;
				_curClosedWidth = _autoModeClosedWidth;
				_buttonParentCanvasGroup.blocksRaycasts = true;
			});
		if (immediate)
		{
			_showAutoObjTweem.Complete();
		}
	}

	private void ShrinkAutoObj(bool immediate = false)
	{
		_showAutoObjTweem?.Kill();
		_buttonParentCanvasGroup.blocksRaycasts = false;
		SetActiveAutoSettingButton(active: true);
		_autoSettingRectTrans.sizeDelta = _autoSettingRectTrans.sizeDelta.WithX(0f);
		LayoutRebuilder.ForceRebuildLayoutImmediate(_buttonParent);
		_expandSizeX = _buttonParent.sizeDelta.x;
		_autoSettingRectTrans.sizeDelta = _autoSettingRectTrans.sizeDelta.WithX(100f);
		_autoSettingCanvasGroup.alpha = 1f;
		_showAutoObjTweem = DOTween.Sequence().Join(_autoSettingCanvasGroup.DOFade(0f, _fadeInDuration)).Join(_autoSettingRectTrans.DOSizeDelta(_autoSettingRectTrans.sizeDelta.WithX(0f), _expandDuration))
			.Join(_bgRectTransform.DOSizeDelta(_bgRectTransform.sizeDelta.WithX(_expandSizeX), _expandDuration))
			.Join(_currentIcon.rectTransform.DOAnchorPos(_currentIconShrinkAnchorPos, _expandDuration))
			.OnComplete(delegate
			{
				_currentIconShrinkAnchorPos = _normalCurrentIconShrinkAncPos;
				_curClosedWidth = _normalClosedWidth;
				SetActiveAutoSettingButton(active: false);
				_buttonParentCanvasGroup.blocksRaycasts = true;
			});
		if (immediate)
		{
			_showAutoObjTweem.Complete();
		}
	}

	public void ExpandImmediate()
	{
		if (ActiveAuto)
		{
			ExpandAutoObj(immediate: true);
		}
		else
		{
			ShrinkAutoObj(immediate: true);
		}
		Expand(immediate: true);
	}

	public void ShrinkImmediate()
	{
		if (ActiveAuto)
		{
			ExpandAutoObj(immediate: true);
		}
		else
		{
			ShrinkAutoObj(immediate: true);
		}
		Shrink(immediate: true);
	}

	public void SetUseAutoSetting(bool use)
	{
		InteractableUI[] autoSettingInteractableUIs = _autoSettingInteractableUIs;
		for (int i = 0; i < autoSettingInteractableUIs.Length; i++)
		{
			autoSettingInteractableUIs[i].SetUseUI(use);
		}
	}
}
