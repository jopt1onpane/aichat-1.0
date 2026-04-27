using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

public class LayoutPresetChangeUI : MonoBehaviour, ILayoutPresetChangeUI
{
	private enum UnsavedWarningResult
	{
		Cancel,
		Save,
		Discard
	}

	[SerializeField]
	private LayoutPresetSaveIndexButton[] _presetButtons;

	[SerializeField]
	private PresetSaveInteractableUI _saveButton;

	[SerializeField]
	private ClickOutsideDetector _saveClickOutsideDetector;

	[SerializeField]
	private GameObject _selectIndexObj;

	[SerializeField]
	private GameObject _confirmObj;

	[SerializeField]
	private Button _confirmYesButton;

	[SerializeField]
	private Button _confirmNoButton;

	[SerializeField]
	private Vector2 _confirmBgSize;

	[SerializeField]
	private GameObject _warnUnsavedObj;

	[SerializeField]
	private Button _warnUnsavedYesButton;

	[SerializeField]
	private Button _warnUnsavedNoButton;

	[SerializeField]
	private TextLocalizationBehaviour _warnUnsavedText;

	[SerializeField]
	private Vector2 _warnUnsavedBgSize;

	private readonly Subject<bool> _onChangeCurrentData = new Subject<bool>();

	private IPresetDataService _dataService;

	private CancellationTokenSource _saveCts;

	private CancellationTokenSource _warnUnsavedCts;

	public Observable<bool> OnChangeCurrentData => _onChangeCurrentData;

	public void Setup(IPresetDataService dataService)
	{
		_dataService = dataService;
		int currentPresetIndex = _dataService.GetCurrentPresetIndex();
		for (int i = 0; i < _presetButtons.Length; i++)
		{
			LayoutPresetSaveIndexButton obj = _presetButtons[i];
			obj.Setup();
			obj.SetDisplay(showArrow: false, i == currentPresetIndex, canClick: true);
			int ownIndex = i;
			ObservableSubscribeExtensions.Subscribe(obj.Button.OnClickAsObservable(), delegate
			{
				OnClickIndexButtonAsync(ownIndex).Forget();
			}).AddTo(this);
		}
		ObservableSubscribeExtensions.Subscribe(_saveButton.OnClickAsObservable(), delegate
		{
			OnClickSaveButtonAsync().Forget();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_saveClickOutsideDetector.OnClickOutside, delegate
		{
			HideSaveDialog();
			HideUnsavedWarningDialog();
		}).AddTo(this);
		_saveButton.DeactivateUseUI();
		_saveClickOutsideDetector.enabled = false;
		_selectIndexObj.SetActive(value: false);
		_confirmObj.SetActive(value: false);
		_warnUnsavedObj.SetActive(value: false);
	}

	private async UniTask OnClickIndexButtonAsync(int index)
	{
		if (_saveCts != null || _warnUnsavedCts != null)
		{
			return;
		}
		int prevIndex = _dataService.GetCurrentPresetIndex();
		bool flag = _dataService.HasDifferenceFromPreset(prevIndex);
		if (index == prevIndex && !flag)
		{
			return;
		}
		if (flag)
		{
			switch (await ShowUnsavedWarningDialogAsync(prevIndex, index))
			{
			case UnsavedWarningResult.Cancel:
				return;
			case UnsavedWarningResult.Save:
				_dataService.SaveCurrentToPreset(prevIndex, alsoSetSelectedIndex: false);
				_presetButtons[prevIndex].PlaySaveCompletedEffect();
				break;
			}
		}
		_dataService.LoadPreset(index);
		ResetPresetButtonDisplayToNormal();
		_onChangeCurrentData.OnNext(value: true);
	}

	private async UniTask OnClickSaveButtonAsync()
	{
		int num = await ShowSaveDialogAsync();
		if (num >= 0)
		{
			_dataService.SaveCurrentToPreset(num, alsoSetSelectedIndex: true);
			_presetButtons[num].PlaySaveCompletedEffect();
			ResetPresetButtonDisplayToNormal();
		}
	}

	public async UniTask<int> ShowSaveDialogAsync()
	{
		_saveCts = new CancellationTokenSource();
		CancellationToken ct = _saveCts.Token;
		_saveButton.ActivateUseUI(_confirmBgSize);
		_saveClickOutsideDetector.enabled = true;
		try
		{
			int presetIndex;
			(int, Unit, Unit) obj;
			do
			{
				_selectIndexObj.SetActive(value: true);
				_confirmObj.SetActive(value: false);
				for (int i = 0; i < _presetButtons.Length; i++)
				{
					_presetButtons[i].SetDisplay(showArrow: true, isSelected: false, canClick: true);
				}
				presetIndex = (await UniTask.WhenAny(_presetButtons.Select((LayoutPresetSaveIndexButton button) => button.OnClickAsObservable().ToUniTask(useFirstValue: true, ct)))).Item1;
				ct.ThrowIfCancellationRequested();
				_selectIndexObj.SetActive(value: false);
				_confirmObj.SetActive(value: true);
				for (int num = 0; num < _presetButtons.Length; num++)
				{
					_presetButtons[num].SetDisplay(num == presetIndex, num == presetIndex, canClick: false);
				}
				obj = await UniTask.WhenAny(_confirmYesButton.OnClickAsObservable().ToUniTask(useFirstValue: true, ct), _confirmNoButton.OnClickAsObservable().ToUniTask(useFirstValue: true, ct));
				ct.ThrowIfCancellationRequested();
			}
			while (obj.Item1 != 0);
			return presetIndex;
		}
		catch (OperationCanceledException)
		{
			return -1;
		}
		finally
		{
			HideSaveDialog();
		}
	}

	private void HideSaveDialog()
	{
		if (_saveCts != null)
		{
			_saveCts.Cancel();
			_saveCts = null;
			_saveButton.DeactivateUseUI();
			_saveClickOutsideDetector.enabled = false;
			_selectIndexObj.SetActive(value: false);
			_confirmObj.SetActive(value: false);
			ResetPresetButtonDisplayToNormal();
		}
	}

	private async UniTask<UnsavedWarningResult> ShowUnsavedWarningDialogAsync(int unsavedIndex, int moveToIndex)
	{
		_warnUnsavedCts = new CancellationTokenSource();
		CancellationToken ct = _warnUnsavedCts.Token;
		try
		{
			_saveButton.ActivateUseUI(_warnUnsavedBgSize);
			_warnUnsavedText.Set("ui_changed_check", (string str) => string.Format(str, unsavedIndex + 1));
			_warnUnsavedObj.SetActive(value: true);
			_saveClickOutsideDetector.enabled = true;
			for (int num = 0; num < _presetButtons.Length; num++)
			{
				_presetButtons[num].SetDisplay(num == unsavedIndex, num == unsavedIndex || num == moveToIndex, canClick: false);
			}
			(int, Unit, Unit) obj = await UniTask.WhenAny(_warnUnsavedYesButton.OnClickAsObservable().ToUniTask(useFirstValue: true, ct), _warnUnsavedNoButton.OnClickAsObservable().ToUniTask(useFirstValue: true, ct));
			ct.ThrowIfCancellationRequested();
			return (obj.Item1 == 0) ? UnsavedWarningResult.Save : UnsavedWarningResult.Discard;
		}
		catch (OperationCanceledException)
		{
			return UnsavedWarningResult.Cancel;
		}
		finally
		{
			HideUnsavedWarningDialog();
		}
	}

	private void HideUnsavedWarningDialog()
	{
		if (_warnUnsavedCts != null)
		{
			_warnUnsavedCts.Cancel();
			_warnUnsavedCts = null;
			_saveButton.DeactivateUseUI();
			_warnUnsavedObj.SetActive(value: false);
			_saveClickOutsideDetector.enabled = false;
			ResetPresetButtonDisplayToNormal();
		}
	}

	private void ResetPresetButtonDisplayToNormal()
	{
		int currentPresetIndex = _dataService.GetCurrentPresetIndex();
		for (int i = 0; i < _presetButtons.Length; i++)
		{
			_presetButtons[i].SetDisplay(showArrow: false, i == currentPresetIndex, canClick: true);
		}
	}
}
