using FastEnumUtility;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class TimeEnvironmentController : EnvironmentControllerBase
{
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

	[SerializeField]
	private EnvironmentType environmentType;

	[SerializeField]
	private WindowBehavior _windowBehavior;

	[SerializeField]
	private InteractableUI _mainIconUI;

	[SerializeField]
	private HoldButtonAnimation _mainIconHoldButton;

	[SerializeField]
	private RectTransform _newItemIcon;

	[SerializeField]
	private Button _mainIconButton;

	[SerializeField]
	private Image _lockImage;

	[SerializeField]
	private GameObject[] _lockDeactiveobjects;

	[SerializeField]
	private Image[] _backImages;

	[SerializeField]
	private TooltipTarget _toolTipTarget;

	private readonly ReactiveProperty<bool> _isNewIconActive = new ReactiveProperty<bool>();

	private DisposableBag scenarioDisposable;

	public override ReadOnlyReactiveProperty<bool> IsNewIconActive => _isNewIconActive;

	public WindowViewType WindowViewType => _windowBehavior.WindowViewType;

	public EnvironmentType EnvironmentType => environmentType;

	public Observable<Unit> OnClickMainIcon()
	{
		return _mainIconButton.OnClickAsObservable();
	}

	public void Setup()
	{
		_mainIconUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_windowBehavior.OnActivateWindow, delegate
		{
			OnActivateWindow();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_windowBehavior.OnDeactivateWindow, delegate
		{
			OnDeactivateWindow();
		}).AddTo(this);
		if (environmentType.TryConvertToWindowViewType(out var windowViewType))
		{
			_windowBehavior.Setup(windowViewType);
		}
		else
		{
			Debug.LogError($"環境{environmentType}をWindowViewTypeに変換できませんでした。");
		}
		_unlockItemService.Environment.GetLockState(environmentType).IsLocked.Subscribe(OnChangeLocked).AddTo(this);
	}

	public override void ApplyWindowBySaveData()
	{
		_windowBehavior.ApplyWindowBySaveData();
	}

	public void OnActivateWindow()
	{
		_mainIconUI.ActivateUseUI();
		NewIconDeactivateFirstPlay();
	}

	public void OnDeactivateWindow()
	{
		_mainIconUI.DeactivateUseUI();
	}

	public void ChangeWindowView(ChangeType changeType)
	{
		_windowBehavior.ChangeWindowView(changeType);
		_environmentListService.SetLastChangeWindowDateTime();
	}

	private string GetEnvironmentName()
	{
		return WindowViewType.ToName();
	}

	public void ActivateWindow()
	{
		if (!_unlockItemService.Environment.GetLockState(environmentType).IsLocked.CurrentValue)
		{
			ChangeWindowView(ChangeType.Activate);
			NewIconDeactivateFirstPlay();
		}
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
		if (!SaveDataManager.Instance.EnvironmentProgressData.PlayedEnvironment.Contains(GetEnvironmentName()))
		{
			SaveDataManager.Instance.EnvironmentProgressData.PlayedEnvironment.Add(GetEnvironmentName());
			SaveDataManager.Instance.SaveEnvironmentProgressData();
		}
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
			return !SaveDataManager.Instance.EnvironmentProgressData.PlayedEnvironment.Contains(GetEnvironmentName());
		}
		return false;
	}

	private void OnChangeLocked(bool isLocked)
	{
		UpdateLock(isLocked);
		if (!isLocked && IsNew())
		{
			ActivateNewIcon();
		}
	}

	private void UpdateLock(bool isLocked)
	{
		_lockImage.gameObject.SetActive(isLocked);
		_mainIconHoldButton.enabled = !isLocked;
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
		if (_toolTipTarget != null)
		{
			_toolTipTarget.SetContentLocalizeID(isLocked ? "ui_lock_guide" : GetEnvironmentLocalizeID(isLock: false));
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
}
