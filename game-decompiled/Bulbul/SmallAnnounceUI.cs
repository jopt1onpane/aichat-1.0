using DG.Tweening;
using NestopiSystem.DIContainers;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class SmallAnnounceUI : MonoBehaviour
{
	private enum AnimationState
	{
		Unknown,
		Deactivated,
		Activated,
		Activating,
		Deactivating
	}

	private const float DisplayDuration = 1.5f;

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
	private RectTransform _rectTransform;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private float _fadeInDuration;

	[SerializeField]
	private float _fadeOutDuration;

	private Tween _tween;

	private Sequence sequence;

	private bool _isLayoutDirty;

	private float _moveAmountY;

	private Vector2 _initPos;

	private float _fromPosY;

	private float _toPosY;

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

	private void Start()
	{
		Setup();
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
		base.gameObject.SetActive(value: true);
		_isLayoutDirty = true;
	}

	public void SetPosition(Vector2 pointerPosition)
	{
		Vector2 anchoredPosition = _rectTransform.anchoredPosition;
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
		_initPos = rectTransform.anchoredPosition;
		if (_animState != AnimationState.Deactivated && _animState != AnimationState.Unknown)
		{
			_rectTransform.anchoredPosition = anchoredPosition;
		}
	}

	public void Activate(float moveAmountY, string localizeID)
	{
		_moveAmountY = moveAmountY;
		_textLocalization.Set(localizeID);
		_fromPosY = _initPos.y - _moveAmountY;
		_toPosY = _initPos.y;
		base.gameObject.SetActive(value: true);
		KillTween();
		_canvasGroup.alpha = 0f;
		_animState = AnimationState.Activating;
		UpdateLayoutIfDirty();
		_rectTransform.anchoredPosition = new Vector2(_initPos.x, _fromPosY);
		sequence = DOTween.Sequence().SetLink(base.gameObject);
		sequence.Append(_canvasGroup.DOFade(1f, _fadeInDuration)).Join(_rectTransform.DOAnchorPosY(_toPosY, _fadeInDuration)).AppendCallback(delegate
		{
			_animState = AnimationState.Activated;
		})
			.AppendInterval(1.5f)
			.AppendCallback(delegate
			{
				Deactivate();
			});
	}

	public void Deactivate()
	{
		if (_animState != AnimationState.Deactivated && _animState != AnimationState.Deactivating)
		{
			KillTween();
			_animState = AnimationState.Deactivating;
			sequence = DOTween.Sequence().SetLink(base.gameObject);
			sequence.Append(_rectTransform.DOAnchorPosY(_fromPosY, _fadeOutDuration)).Join(_canvasGroup.DOFade(0f, _fadeOutDuration)).AppendCallback(delegate
			{
				base.gameObject.SetActive(value: false);
				_animState = AnimationState.Deactivated;
			});
		}
	}

	private void KillTween()
	{
		_tween?.Kill();
		sequence?.Complete();
		_tween = null;
		sequence = null;
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
