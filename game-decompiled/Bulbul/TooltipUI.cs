using DG.Tweening;
using NestopiSystem.DIContainers;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class TooltipUI : MonoBehaviour
{
	private enum AnimationState
	{
		Unknown,
		Deactivated,
		Activated,
		Activating,
		Deactivating
	}

	[Inject]
	private LanguageSupplier languageSupplier;

	[Inject]
	private LocalizationMasterWrapper localizationMaster;

	[SerializeField]
	private Vector2 _pointerOffset;

	[SerializeField]
	private Vector2 _screenMargin;

	[SerializeField]
	private TextLocalizationBehaviour _textLocalization;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private float _fadeInDuration;

	[SerializeField]
	private float _fadeOutDuration;

	private Tween _tween;

	private bool _isLayoutDirty;

	private AnimationState _animState;

	public bool IsActive
	{
		get
		{
			AnimationState animState = _animState;
			return animState == AnimationState.Activated || animState == AnimationState.Activating;
		}
	}

	public bool IsInactive
	{
		get
		{
			AnimationState animState = _animState;
			return animState == AnimationState.Deactivated || animState == AnimationState.Deactivating;
		}
	}

	public void Setup()
	{
		if (languageSupplier == null)
		{
			languageSupplier = ProjectLifetimeScope.Resolve<LanguageSupplier>();
		}
		if (localizationMaster == null)
		{
			localizationMaster = ProjectLifetimeScope.Resolve<LocalizationMasterWrapper>();
		}
		_canvasGroup.alpha = 0f;
		_animState = AnimationState.Deactivated;
		base.gameObject.SetActive(value: false);
		_isLayoutDirty = true;
	}

	public bool SetData(TooltipData data)
	{
		bool result = false;
		_isLayoutDirty = true;
		string result2;
		if (data.LocalizeConverter == null)
		{
			result = _textLocalization.Set(data.ContentLocalizeID);
		}
		else if (localizationMaster.TryGet(data.ContentLocalizeID, out result2))
		{
			data.LocalizeConverter.Bind(_textLocalization.Text, result2);
			result = true;
		}
		if (IsActive)
		{
			UpdateLayoutIfDirty();
		}
		return result;
	}

	public void SetPosition(Vector2 pointerPosition)
	{
		RectTransform rectTransform = base.transform as RectTransform;
		Vector2 vector = pointerPosition + (Vector2)rectTransform.TransformVector(_pointerOffset);
		Rect safeArea = Screen.safeArea;
		Vector2 size = rectTransform.rect.size;
		Vector3 lossyScale = rectTransform.lossyScale;
		size.x *= lossyScale.x;
		size.y *= lossyScale.y;
		vector.x = Mathf.Clamp(vector.x, safeArea.xMin + _screenMargin.x, safeArea.xMax - size.x * (1f - rectTransform.pivot.x) - _screenMargin.x);
		vector.y = Mathf.Clamp(vector.y, safeArea.yMin + _screenMargin.y, safeArea.yMax - size.y * (1f - rectTransform.pivot.y) - _screenMargin.y);
		rectTransform.position = vector;
	}

	public void Activate(bool withAnimation = true)
	{
		if (_animState == AnimationState.Activated)
		{
			return;
		}
		if (!withAnimation)
		{
			KillTween();
			base.gameObject.SetActive(value: true);
			UpdateLayoutIfDirty();
			_canvasGroup.alpha = 1f;
			_animState = AnimationState.Activated;
		}
		else if (_animState != AnimationState.Activating)
		{
			KillTween();
			_animState = AnimationState.Activating;
			base.gameObject.SetActive(value: true);
			UpdateLayoutIfDirty();
			_tween = _canvasGroup.DOFade(1f, _fadeInDuration).OnComplete(delegate
			{
				_animState = AnimationState.Activated;
			});
		}
	}

	public void Deactivate(bool withAnimation = true)
	{
		if (_animState == AnimationState.Deactivated)
		{
			return;
		}
		if (!withAnimation)
		{
			KillTween();
			base.gameObject.SetActive(value: false);
			_animState = AnimationState.Deactivated;
		}
		else if (_animState != AnimationState.Deactivating)
		{
			KillTween();
			_animState = AnimationState.Deactivating;
			_tween = _canvasGroup.DOFade(0f, _fadeOutDuration).OnComplete(delegate
			{
				base.gameObject.SetActive(value: false);
				_animState = AnimationState.Deactivated;
			});
		}
	}

	private void KillTween()
	{
		_tween?.Kill();
		_tween = null;
		_animState = AnimationState.Unknown;
	}

	private void UpdateLayoutIfDirty()
	{
		if (_isLayoutDirty)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
			_isLayoutDirty = false;
		}
	}
}
