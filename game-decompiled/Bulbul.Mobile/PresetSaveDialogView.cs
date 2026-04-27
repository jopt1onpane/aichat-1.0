using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul.Mobile;

public class PresetSaveDialogView : MonoBehaviour
{
	[Serializable]
	public class SaveSlotObj
	{
		public InteractableUI SlotButton;

		public GameObject ArrowObj;

		public CanvasGroup SaveEffectCanvasGroup;
	}

	private enum Mode
	{
		Switch,
		Save
	}

	[Inject]
	private SystemSeService _systemSeService;

	private static readonly string _overwriteNoticeTextID = "ui_overwrite_check";

	private static readonly string _saveNoticeTextID = "ui_save_check";

	private static readonly string _saveSlotSelectionNoticeTextID = "ui_save_select";

	private static readonly string _diffNoticeTextID = "ui_changed_check";

	[SerializeField]
	private CommonWindowShowHideAnimation _activator;

	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private RectTransform _baseRectTransform;

	[SerializeField]
	private CanvasGroup _baseCanvasGroup;

	[SerializeField]
	private CanvasGroup _saveModeOnlyCanvasGroup;

	[SerializeField]
	private float _switchModeBaseHeight;

	[SerializeField]
	private float _saveModeBaseHeight;

	[SerializeField]
	private InteractableUI _saveButton;

	[SerializeField]
	private SaveSlotObj[] _saveSlotObjs;

	[SerializeField]
	private TextLocalizationBehaviour _noticeText;

	[SerializeField]
	private TextLocalizationBehaviour _saveNoticeText;

	[SerializeField]
	private Button _yesButton;

	[SerializeField]
	private Button _noButton;

	private Mode _currentMode;

	private int _unSaveIdx = -1;

	private int _openedSlotIdx = -1;

	private bool _isExecSaved;

	private bool _isEnterOverwrite;

	private Sequence _sequence;

	private UniTaskCompletionSource<int> _selectedCompletionSource;

	private UniTaskCompletionSource<bool> _decisionCompletionSource;

	private CancellationTokenSource _animationCancellationTokenSource;

	private Subject<(int, bool)> _onSavedSlot = new Subject<(int, bool)>();

	private Subject<int> _onChangedSlot = new Subject<int>();

	private Subject<bool> _onCloseDialog = new Subject<bool>();

	private bool _isPlayingSavedAnimation;

	public ReactiveProperty<bool> IsActive { get; private set; } = new ReactiveProperty<bool>(value: false);

	public Func<int> CurrentSelectedPresetIdxGetter { get; set; }

	public Func<int, bool> HasDifferenceFromPreset { get; set; }

	public Observable<(int, bool)> OnSavedSlot => _onSavedSlot;

	public Observable<int> OnChangedSlot => _onChangedSlot;

	public Observable<bool> OnCloseDialog => _onCloseDialog;

	private void ReleaseCompletionSource()
	{
		_selectedCompletionSource?.TrySetCanceled();
		_decisionCompletionSource?.TrySetCanceled();
		_selectedCompletionSource = null;
		_decisionCompletionSource = null;
	}

	public void Setup()
	{
		ObservableSubscribeExtensions.Subscribe(_closeButton.OnClickAsObservable(), delegate
		{
			if (!_isPlayingSavedAnimation)
			{
				Deactivate();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_saveButton.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			if (_currentMode != Mode.Save)
			{
				_systemSeService.PlaySelect();
				StartSaveAsync().Forget();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_yesButton.OnClickAsObservable(), delegate
		{
			OnClickYesButton();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_noButton.OnClickAsObservable(), delegate
		{
			OnClickNoButton();
		}).AddTo(this);
		int num = 0;
		SaveSlotObj[] saveSlotObjs = _saveSlotObjs;
		foreach (SaveSlotObj obj in saveSlotObjs)
		{
			int i = num;
			ObservableSubscribeExtensions.Subscribe(obj.SlotButton.GetComponent<Button>().OnClickAsObservable(), delegate
			{
				OnClickSaveSlot(i);
			}).AddTo(this);
			num++;
		}
	}

	private void OnClickYesButton()
	{
		_systemSeService.PlayClick();
		_decisionCompletionSource?.TrySetResult(result: true);
	}

	private void OnClickNoButton()
	{
		_systemSeService.PlayCancel();
		_decisionCompletionSource?.TrySetResult(result: false);
	}

	private void OnClickSaveSlot(int idx)
	{
		_systemSeService.PlaySelect();
		_selectedCompletionSource?.TrySetResult(idx);
	}

	private void SelectSaveSlot(int idx)
	{
		SaveSlotObj[] saveSlotObjs = _saveSlotObjs;
		for (int i = 0; i < saveSlotObjs.Length; i++)
		{
			saveSlotObjs[i].SlotButton.DeactivateUseUI();
		}
		_saveSlotObjs[idx].SlotButton.ActivateUseUI();
	}

	private void PlaySaveModeTween()
	{
		_sequence?.Kill();
		_sequence = DOTween.Sequence();
		_baseCanvasGroup.blocksRaycasts = false;
		_saveModeOnlyCanvasGroup.blocksRaycasts = false;
		Vector2 sizeDelta = _baseRectTransform.sizeDelta;
		sizeDelta.y = _saveModeBaseHeight;
		_sequence.Join(_baseRectTransform.DOSizeDelta(sizeDelta, 0.2f));
		_sequence.Join(_saveModeOnlyCanvasGroup.DOFade(1f, 0.2f).SetEase(Ease.InQuint));
		_sequence.OnComplete(delegate
		{
			_baseCanvasGroup.blocksRaycasts = true;
			_saveModeOnlyCanvasGroup.blocksRaycasts = true;
		});
	}

	private void PlaySwitchModeTween()
	{
		_sequence?.Kill();
		_sequence = DOTween.Sequence();
		_baseCanvasGroup.blocksRaycasts = false;
		_saveModeOnlyCanvasGroup.blocksRaycasts = false;
		Vector2 sizeDelta = _baseRectTransform.sizeDelta;
		sizeDelta.y = _switchModeBaseHeight;
		_sequence.Join(_baseRectTransform.DOSizeDelta(sizeDelta, 0.2f));
		_sequence.Join(_saveModeOnlyCanvasGroup.DOFade(0f, 0.1f).SetEase(Ease.OutExpo));
		_sequence.OnComplete(delegate
		{
			_baseCanvasGroup.blocksRaycasts = true;
		});
	}

	private void SetActiveArrowImageAll(bool active)
	{
		SaveSlotObj[] saveSlotObjs = _saveSlotObjs;
		for (int i = 0; i < saveSlotObjs.Length; i++)
		{
			saveSlotObjs[i].ArrowObj.SetActive(active);
		}
	}

	private void ResetSelectButtonState()
	{
		SaveSlotObj[] saveSlotObjs = _saveSlotObjs;
		foreach (SaveSlotObj obj in saveSlotObjs)
		{
			obj.SlotButton.DeactivateUseUI();
			obj.ArrowObj.SetActive(value: false);
			obj.SaveEffectCanvasGroup.alpha = 0f;
			obj.SaveEffectCanvasGroup.gameObject.SetActive(value: false);
		}
	}

	private async UniTask StartSwitchAsync()
	{
		try
		{
			_currentMode = Mode.Switch;
			_selectedCompletionSource = new UniTaskCompletionSource<int>();
			_unSaveIdx = CurrentSelectedPresetIdxGetter();
			SelectSaveSlot(_unSaveIdx);
			int num = await _selectedCompletionSource.Task;
			if (HasDifferenceFromPreset(_unSaveIdx))
			{
				StartSaveOverwrite(num).Forget();
				return;
			}
			_onChangedSlot.OnNext(num);
			SelectSaveSlot(num);
			StartSwitchAsync().Forget();
		}
		catch (OperationCanceledException ex)
		{
			throw ex;
		}
	}

	private async UniTask StartSaveOverwrite(int movedIdx)
	{
		_ = 1;
		try
		{
			_isEnterOverwrite = true;
			ReleaseCompletionSource();
			ResetSelectButtonState();
			_currentMode = Mode.Save;
			PlaySaveModeTween();
			_saveButton.ActivateUseUI();
			_saveNoticeText.Text.enabled = true;
			_noticeText.Text.enabled = true;
			_saveNoticeText.Set(_saveNoticeTextID);
			_noticeText.Set(_diffNoticeTextID, (string str) => string.Format(str, _unSaveIdx + 1));
			_yesButton.gameObject.SetActive(value: true);
			_noButton.gameObject.SetActive(value: true);
			_saveSlotObjs[_unSaveIdx].ArrowObj.SetActive(value: true);
			_saveSlotObjs[_unSaveIdx].SlotButton.ActivateUseUI();
			_saveSlotObjs[movedIdx].SlotButton.ActivateUseUI();
			_decisionCompletionSource = new UniTaskCompletionSource<bool>();
			if (await _decisionCompletionSource.Task)
			{
				_onSavedSlot.OnNext((_unSaveIdx, false));
				_isExecSaved = true;
				await PlaySaveEffect(_unSaveIdx);
			}
			_onChangedSlot.OnNext(movedIdx);
			_saveButton.DeactivateUseUI();
			_currentMode = Mode.Switch;
			PlaySwitchModeTween();
			StartSwitchAsync().Forget();
		}
		catch (OperationCanceledException ex)
		{
			throw ex;
		}
	}

	private async UniTask StartSaveAsync()
	{
		_ = 2;
		try
		{
			ReleaseCompletionSource();
			_currentMode = Mode.Save;
			PlaySaveModeTween();
			ResetSelectButtonState();
			SetActiveArrowImageAll(active: true);
			_saveButton.ActivateUseUI();
			_noticeText.Set(_saveSlotSelectionNoticeTextID);
			_noticeText.Text.enabled = true;
			_saveNoticeText.Text.enabled = false;
			_yesButton.gameObject.SetActive(value: false);
			_noButton.gameObject.SetActive(value: false);
			_selectedCompletionSource = new UniTaskCompletionSource<int>();
			int idx = await _selectedCompletionSource.Task;
			SelectSaveSlot(idx);
			SetActiveArrowImageAll(active: false);
			_noticeText.Text.enabled = false;
			_saveSlotObjs[idx].ArrowObj.SetActive(value: true);
			_saveNoticeText.Text.enabled = true;
			_saveNoticeText.Set(_overwriteNoticeTextID);
			_yesButton.gameObject.SetActive(value: true);
			_noButton.gameObject.SetActive(value: true);
			_selectedCompletionSource = null;
			_decisionCompletionSource = new UniTaskCompletionSource<bool>();
			bool num = await _decisionCompletionSource.Task;
			_decisionCompletionSource = null;
			if (num)
			{
				_onSavedSlot.OnNext((idx, true));
				_isExecSaved = true;
				await PlaySaveEffect(idx);
			}
			_currentMode = Mode.Switch;
			_saveButton.DeactivateUseUI();
			PlaySwitchModeTween();
			StartSwitchAsync().Forget();
		}
		catch (OperationCanceledException ex)
		{
			throw ex;
		}
	}

	private async UniTask PlaySaveEffect(int idx)
	{
		CancelAnimation();
		_isPlayingSavedAnimation = true;
		_animationCancellationTokenSource = new CancellationTokenSource();
		_saveSlotObjs[idx].SaveEffectCanvasGroup.gameObject.SetActive(value: true);
		_saveSlotObjs[idx].SaveEffectCanvasGroup.alpha = 0f;
		await _saveSlotObjs[idx].SaveEffectCanvasGroup.DOFade(1f, 0.2f).SetLoops(2, LoopType.Yoyo).AwaitForComplete(TweenCancelBehaviour.Kill, _animationCancellationTokenSource.Token);
		_saveSlotObjs[idx].SaveEffectCanvasGroup.gameObject.SetActive(value: false);
		_isPlayingSavedAnimation = false;
	}

	private void CancelAnimation()
	{
		if (_animationCancellationTokenSource != null)
		{
			_animationCancellationTokenSource.Cancel();
			_animationCancellationTokenSource.Dispose();
			_animationCancellationTokenSource = null;
		}
		_isPlayingSavedAnimation = false;
	}

	public void Activate()
	{
		_currentMode = Mode.Switch;
		_isPlayingSavedAnimation = false;
		IsActive.Value = true;
		_saveButton.DeactivateUseUI();
		_openedSlotIdx = CurrentSelectedPresetIdxGetter();
		_isExecSaved = false;
		_isEnterOverwrite = false;
		_activator.Show();
		PlaySwitchModeTween();
		_sequence.Complete();
		ResetSelectButtonState();
		StartSwitchAsync().Forget();
	}

	public void Deactivate()
	{
		int num = CurrentSelectedPresetIdxGetter();
		_onCloseDialog.OnNext(_isExecSaved || _openedSlotIdx != num || _isEnterOverwrite);
		IsActive.Value = false;
		CancelAnimation();
		ReleaseCompletionSource();
		_activator.Hide();
	}

	private void OnDestroy()
	{
		ReleaseCompletionSource();
		CancelAnimation();
		IsActive.Dispose();
	}
}
