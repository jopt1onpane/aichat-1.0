using Bulbul;
using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.UI;

public class WantTalkUI : MonoBehaviour, IWantTalkUI
{
	private const string UIIdleTrigger = "Idle";

	private const string UIPlayNormalTrigger = "PlayNormal";

	private const string UIPlaySpecialTrigger = "PlaySpecial";

	private const string UIIdleAnimationName = "Idle";

	[SerializeField]
	[Header("TalkGuideAnimator")]
	private Animator _talkGuideAnimator;

	[SerializeField]
	[Header("フェード用CanvasGroup")]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	[Header("話したがる時のUI: Image")]
	private Image _wantTalkImage;

	[SerializeField]
	[Header("話したがる時のUI: 通常Sprite")]
	private Sprite _wantTalkNormalSprite;

	[SerializeField]
	[Header("話したがる時のUI: スペシャルSprite")]
	private Sprite _wantTalkSpecialSprite;

	private RectTransform _rectTransform;

	private Tween _moveTween;

	private Tween _fadeTween;

	protected float _fromPosY;

	protected float _toPosY;

	private bool _isActive;

	public virtual void Setup()
	{
		_rectTransform = base.transform as RectTransform;
		_fromPosY = _rectTransform.anchoredPosition.y + -8f;
		_toPosY = _rectTransform.anchoredPosition.y;
		Deactivate();
		ObservableSubscribeExtensions.Subscribe(SaveDataManager.Instance.CollaborationSaveData.CurrentType, delegate
		{
			if (!_talkGuideAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && base.gameObject.activeSelf)
			{
				PlayUpdateAnim();
			}
		}).AddTo(this);
	}

	public bool IsActive()
	{
		return _isActive;
	}

	public void Activate()
	{
		_isActive = true;
		base.gameObject.SetActive(value: true);
		_moveTween?.Kill();
		_fadeTween?.Kill();
		if (SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value == SpecialService.CollaborationType.None)
		{
			_wantTalkImage.sprite = _wantTalkNormalSprite;
		}
		else
		{
			_wantTalkImage.sprite = _wantTalkSpecialSprite;
		}
		_moveTween = _rectTransform.DOAnchorPosY(_toPosY, 0.2f);
		_fadeTween = _canvasGroup.DOFade(1f, 0.2f).OnComplete(delegate
		{
			PlayUpdateAnim();
		});
	}

	private void PlayUpdateAnim()
	{
		if (SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value == SpecialService.CollaborationType.None)
		{
			_talkGuideAnimator.SetTrigger("PlayNormal");
		}
		else
		{
			_talkGuideAnimator.SetTrigger("PlaySpecial");
		}
	}

	public void Deactivate()
	{
		_isActive = false;
		_moveTween?.Kill();
		_fadeTween?.Kill();
		_moveTween = _rectTransform.DOAnchorPosY(_fromPosY, 0.2f);
		_fadeTween = _canvasGroup.DOFade(0f, 0.2f).OnComplete(delegate
		{
			base.gameObject.SetActive(value: false);
			_talkGuideAnimator.SetTrigger("Idle");
		});
	}
}
