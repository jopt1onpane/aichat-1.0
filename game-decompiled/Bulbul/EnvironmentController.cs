using System;
using System.Linq;
using Bulbul.MasterData;
using FastEnumUtility;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class EnvironmentController : EnvironmentControllerBase
{
	private EnvironmentControllerType _controllerType;

	[Inject]
	private EnvironmentListService _environmentListService;

	[Inject]
	private UnlockItemService _unlockItemService;

	[Inject]
	private FireworksSoundService _fireworksSoundService;

	[Inject]
	private AchievementService _achievementService;

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private DirectionService _directionService;

	[Inject]
	private ScenarioGroupMasterWrapper _scenarioGroupMasterWrapper;

	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private EnvironmentDataService _environmentDataService;

	[SerializeField]
	private EnvironmentType environmentType;

	[SerializeField]
	private WindowBehavior _windowBehavior;

	[SerializeField]
	private AmbientSoundBehavior _ambientSoundBehavior;

	[SerializeField]
	private InteractableUI _mainIconUI;

	[SerializeField]
	private InteractableUI _windowIconUI;

	[SerializeField]
	private RectTransform _newItemIcon;

	[SerializeField]
	private Button _mainIconButton;

	[SerializeField]
	private Button _shopButton;

	[SerializeField]
	private CanvasGroup _mainButtonCanvasGroup;

	[SerializeField]
	private CanvasGroup _shopButtonCanvasGroup;

	[SerializeField]
	private EnvironmentIconDBPC _iconDB;

	[SerializeField]
	private Image _lockImage;

	[SerializeField]
	private GameObject[] _lockDeactiveobjects;

	[SerializeField]
	private Image[] _backImages;

	[SerializeField]
	private TextLocalizationBehaviour _environmentNameText;

	[SerializeField]
	private int _purchasePopoverParentHeight = 90;

	private readonly ReactiveProperty<bool> _isNewIconActive = new ReactiveProperty<bool>();

	private DisposableBag scenarioDisposable;

	private PurchasePopover _purchasePopover;

	public override ReadOnlyReactiveProperty<bool> IsNewIconActive => _isNewIconActive;

	public WindowViewType WindowViewType => _windowBehavior.WindowViewType;

	public AmbientSoundType AmbientSoundType => _ambientSoundBehavior.AmbientSoundType;

	public EnvironmentType EnvironmentType => environmentType;

	public void Setup(PurchasePopover purchasePopover)
	{
		_purchasePopover = purchasePopover;
		_controllerType = this.environmentType.GetEnvironmentControllerType();
		switch (_controllerType)
		{
		}
		if (_mainIconUI != null)
		{
			_mainIconUI.Setup();
		}
		if (_windowIconUI != null)
		{
			_windowIconUI.Setup();
		}
		if (_windowBehavior != null)
		{
			ObservableSubscribeExtensions.Subscribe(_windowBehavior.OnActivateWindow, delegate
			{
				OnActivateWindow();
				switch (this.environmentType)
				{
				case EnvironmentType.ThunderRain:
					_ambientSoundBehavior.Replay();
					break;
				case EnvironmentType.Fireworks:
					_fireworksSoundService.Replay();
					break;
				}
			}).AddTo(this);
			ObservableSubscribeExtensions.Subscribe(_windowBehavior.OnDeactivateWindow, delegate
			{
				OnDeactivateWindow();
				if (this.environmentType == EnvironmentType.Fireworks && _environmentDataService.IsMuteOrVolumeZero(AmbientSoundType.Fireworks_First))
				{
					_fireworksSoundService.Stop();
				}
			}).AddTo(this);
			if (this.environmentType.TryConvertToWindowViewType(out var windowViewType))
			{
				_windowBehavior.Setup(windowViewType);
			}
		}
		if (_ambientSoundBehavior != null)
		{
			_ambientSoundBehavior.OnActivateSound.Subscribe(delegate(AmbientSoundType ambientType)
			{
				OnActivateSound();
				if (ambientType == AmbientSoundType.Fireworks_First && !_fireworksSoundService.IsPlaying())
				{
					_fireworksSoundService.Replay();
				}
			}).AddTo(this);
			_ambientSoundBehavior.OnDeactivateSound.Subscribe(delegate(AmbientSoundType ambientType)
			{
				OnDeactivateSound();
				if (ambientType == AmbientSoundType.Fireworks_First && !_windowBehavior.IsActiveWindow())
				{
					_fireworksSoundService.Stop();
				}
			}).AddTo(this);
			if (this.environmentType.TryConvertToAmbientSoundType(out var ambientSoundType))
			{
				_ambientSoundBehavior.Setup(ambientSoundType);
			}
		}
		_unlockItemService.Environment.GetLockState(this.environmentType).IsLocked.Subscribe(OnChangeLocked).AddTo(this);
		EnvironmentType environmentType = this.environmentType;
		if (environmentType != EnvironmentType.Day && environmentType != EnvironmentType.Sunset && environmentType != EnvironmentType.Night && environmentType != EnvironmentType.Cloudy)
		{
			ObservableSubscribeExtensions.Subscribe(_mainIconButton.OnClickAsObservable().Merge(_shopButton.OnClickAsObservable()), delegate
			{
				OnClickButtonMainIcon();
			}).AddTo(this);
		}
	}

	public override void ApplyWindowBySaveData()
	{
		if (_windowBehavior != null)
		{
			_windowBehavior.ApplyWindowBySaveData();
		}
	}

	private void OnActivateSound()
	{
		_mainIconUI.ActivateUseUI();
		NewIconDeactivateFirstPlay();
	}

	private void OnDeactivateSound()
	{
		switch (_controllerType)
		{
		case EnvironmentControllerType.ViewAndSound:
			if (!_environmentDataService.IsWindowActive(_windowBehavior.WindowViewType))
			{
				_mainIconUI.DeactivateUseUI();
			}
			break;
		case EnvironmentControllerType.SoundOnly:
			_mainIconUI.DeactivateUseUI();
			break;
		}
	}

	public void OnActivateWindow()
	{
		switch (_controllerType)
		{
		case EnvironmentControllerType.ViewAndSound:
			_mainIconUI.ActivateUseUI();
			_windowIconUI.ActivateUseUI();
			break;
		case EnvironmentControllerType.ViewOnly:
			_mainIconUI.ActivateUseUI();
			break;
		}
		NewIconDeactivateFirstPlay();
	}

	public void OnDeactivateWindow()
	{
		switch (_controllerType)
		{
		case EnvironmentControllerType.ViewAndSound:
			_windowIconUI.DeactivateUseUI();
			if (_environmentDataService.IsMute(_ambientSoundBehavior.AmbientSoundType))
			{
				_mainIconUI.DeactivateUseUI();
			}
			break;
		case EnvironmentControllerType.ViewOnly:
			_mainIconUI.DeactivateUseUI();
			break;
		}
	}

	public void ChangeMute()
	{
		_ambientSoundBehavior.ChangeMute();
	}

	public void MuteActivate()
	{
		_ambientSoundBehavior.MuteActivate();
	}

	public void MuteDeactivate()
	{
		_ambientSoundBehavior.MuteDeactivate();
	}

	public void ChangeVolume(float volume)
	{
		_ambientSoundBehavior.ChangeVolume(volume);
	}

	public void ChangeWindowView(ChangeType changeType)
	{
		_windowBehavior.IsActiveWindow();
		_windowBehavior.ChangeWindowView(changeType);
		_environmentListService.SetLastChangeWindowDateTime();
	}

	public void OnClickButtonMainIcon()
	{
		if (_unlockItemService.Environment.GetLockState(environmentType).IsLocked.CurrentValue)
		{
			if (_unlockItemService.Environment.IsPurchasableType(environmentType, out var price))
			{
				_masterDataLoader.EnvironmentMaster.GetEnvironment(environmentType);
				bool canPurchase = _unlockItemService.Environment.CanPurchase(environmentType);
				_purchasePopover.Show(_iconDB.GetShopThumbnail(environmentType), price, canPurchase, Purchase, base.transform, _purchasePopoverParentHeight, new Vector3(0f, -72f));
			}
			return;
		}
		switch (_controllerType)
		{
		case EnvironmentControllerType.ViewAndSound:
			if (_environmentDataService.IsWindowActive(_windowBehavior.WindowViewType) || !_environmentDataService.IsMute(_ambientSoundBehavior.AmbientSoundType))
			{
				ChangeWindowView(ChangeType.Deactivate);
				MuteActivate();
			}
			else
			{
				ChangeWindowView(ChangeType.Activate);
				MuteDeactivate();
			}
			_achievementService.OnChangeWindowView();
			break;
		case EnvironmentControllerType.ViewOnly:
			ChangeWindowView(ChangeType.Switch);
			_achievementService.OnChangeWindowView();
			break;
		case EnvironmentControllerType.SoundOnly:
			ChangeMute();
			break;
		}
		NewIconDeactivateFirstPlay();
	}

	private bool Purchase()
	{
		bool num = _unlockItemService.Environment.Purchase(environmentType);
		if (num)
		{
			_purchasePopover.HideWithPurchaseAnim();
			_systemSeService.PlayBuyItem();
			OnClickButtonMainIcon();
			if (CheckAndAnnounceUnlockedExtraScenario())
			{
				_directionService.SlideFadeAnnounce.Play(SlideFadeAnnounceDirection.AnnounceType.UnlockExtraScenario);
			}
		}
		return num;
	}

	private bool CheckAndAnnounceUnlockedExtraScenario()
	{
		if (_directionService.GamePlayingDefect.IsUseDefectForEpisodeDirection())
		{
			return false;
		}
		if (SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value != SpecialService.CollaborationType.None)
		{
			return false;
		}
		string purchasedEnvironmentName = environmentType.ToName();
		if ((from x in _scenarioGroupMasterWrapper.GetPlayableExtraScenario(includeSeen: false)
			where x.ContentType == SmallTalkSelector.ContentType.PurchaseEnvironment.ToName() && x.Arg1 == purchasedEnvironmentName
			select x).Any())
		{
			return true;
		}
		return false;
	}

	public void OnClickButtonChangeWindowView()
	{
		if (!_unlockItemService.Environment.GetLockState(environmentType).IsLocked.CurrentValue)
		{
			ChangeWindowView(ChangeType.Switch);
			NewIconDeactivateFirstPlay();
			_achievementService.OnChangeWindowView();
		}
	}

	private void NewIconDeactivateFirstPlay()
	{
		DeactivateNewIcon();
		_environmentDataService.SetPlayed(environmentType);
	}

	private void OnDestroy()
	{
		scenarioDisposable.Dispose();
	}

	public bool IsActiveNewIcon()
	{
		return _newItemIcon.gameObject.activeSelf;
	}

	public void ActivateNewIcon()
	{
		_newItemIcon.gameObject.SetActive(value: true);
		_isNewIconActive.Value = true;
	}

	public override void DeactivateNewIcon()
	{
		_newItemIcon.gameObject.SetActive(value: false);
		_isNewIconActive.Value = false;
	}

	private bool IsNew()
	{
		if (!_unlockItemService.Environment.GetLockState(environmentType).IsNotLockCondition)
		{
			return !_environmentDataService.HavePlayed(environmentType);
		}
		return false;
	}

	private void OnChangeLocked(bool isLocked)
	{
		if (IsExpired(isLocked))
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		UpdateLock(isLocked);
		if (!isLocked && IsNew())
		{
			ActivateNewIcon();
		}
	}

	private void UpdateLock(bool isLocked)
	{
		int price;
		bool flag = _unlockItemService.Environment.IsPurchasableType(environmentType, out price);
		_lockImage.gameObject.SetActive(isLocked && !flag);
		_mainIconButton.gameObject.SetActive(!isLocked || !flag);
		_shopButton.gameObject.SetActive(isLocked && flag);
		_mainButtonCanvasGroup.blocksRaycasts = !isLocked;
		_shopButtonCanvasGroup.blocksRaycasts = isLocked && flag;
		GameObject[] lockDeactiveobjects = _lockDeactiveobjects;
		foreach (GameObject gameObject in lockDeactiveobjects)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(!isLocked);
			}
		}
		Image[] backImages = _backImages;
		foreach (Image image in backImages)
		{
			if (image != null)
			{
				image.color = (isLocked ? new Color32(128, 128, 128, 128) : new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			}
		}
		if (_environmentNameText != null)
		{
			_environmentNameText.Set(GetEnvironmentLocalizeID(isLocked));
			_environmentNameText.Text.color = (isLocked ? new Color32(128, 128, 128, byte.MaxValue) : new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
		}
	}

	public bool IsLock()
	{
		return _unlockItemService.Environment.GetLockState(environmentType).IsLocked.CurrentValue;
	}

	private string GetEnvironmentLocalizeID(bool isLock)
	{
		if (isLock)
		{
			return "ui_lock_title";
		}
		return _masterDataLoader.EnvironmentMaster.GetEnvironment(environmentType).NameLocalizeID;
	}

	private bool IsExpired(bool isLock)
	{
		if (!isLock)
		{
			return false;
		}
		UnlockEnvironmentData unlockEnvironmentData = _masterDataLoader.UnlockEnvironmentMasterList.FirstOrDefault((UnlockEnvironmentData x) => x.ItemType == environmentType.ToName());
		if (unlockEnvironmentData == null)
		{
			return false;
		}
		if (!unlockEnvironmentData.TryGetUnlockExpire(out var dateTime))
		{
			return false;
		}
		return DateTime.Now > dateTime;
	}
}
