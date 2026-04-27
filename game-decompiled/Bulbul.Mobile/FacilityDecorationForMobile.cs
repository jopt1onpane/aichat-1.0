using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using FastEnumUtility;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul.Mobile;

public class FacilityDecorationForMobile : MonoBehaviour
{
	private static readonly int _categoryMax = 11;

	[Inject]
	private DecorationService _decorationService;

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private UnlockItemService _unlockItemService;

	[Inject]
	private DecorationDataService _decorationDataService;

	[Inject]
	private PlayerPointService _playerPointService;

	[Inject]
	private RoomCameraManager _roomCameraManager;

	[Inject]
	private MobileDemoEditionLockedTargetCheckService _mobileDemoEditionLockedTargetCheckService;

	[SerializeField]
	private DecorationWindowView _decorationWindowView;

	[SerializeField]
	private GameObject _decorationMainSceneButtonNewIcon;

	private bool _isActive;

	private DecorationService.DecorationCategoryType _currentCategoryType = DecorationService.DecorationCategoryType.Cup;

	private bool _isCurrentCategoryUnlocked = true;

	private Dictionary<DecorationService.DecorationSkinType, string> _decorationSkinTypeNameCache = new Dictionary<DecorationService.DecorationSkinType, string>();

	private bool _isDirtyData;

	private bool _lastCheckedUpgradePassPurchasedState;

	public Observable<Unit> OnClickClose => _decorationWindowView.OnClickCloseButton;

	private void CreateNameCache()
	{
		foreach (Member<DecorationService.DecorationSkinType> member in FastEnum.GetMembers<DecorationService.DecorationSkinType>())
		{
			_decorationSkinTypeNameCache.Add(member.Value, member.Value.ToString());
		}
	}

	public void Setup()
	{
	}

	public void UpdateFacility()
	{
		if (_isDirtyData)
		{
			_decorationWindowView.ListView.SetCategory(_currentCategoryType);
			_isDirtyData = false;
		}
	}

	private int GetCurrentPresetIndex()
	{
		return _decorationDataService.GetCurrentPresetIndex();
	}

	private bool CheckPresetSaveSlotDiff(int idx)
	{
		return _decorationDataService.HasDifferenceFromPreset(idx);
	}

	private int GetPlayerPoint()
	{
		return _playerPointService.Point;
	}

	private void UpdateCategoryKnown()
	{
		for (int i = 1; i <= _categoryMax; i++)
		{
			DecorationService.DecorationCategoryType decorationCategoryType = (DecorationService.DecorationCategoryType)i;
			bool isKnown = IsCategoryKnown(decorationCategoryType);
			_decorationWindowView.ListView.SetCategoryKnown(decorationCategoryType, isKnown);
		}
	}

	private bool CheckDeactivationSkins(DecorationService.DecorationCategoryType categoryType)
	{
		foreach (var (skinType, decorationData2) in SaveDataManager.Instance.DecorationSaveData.DecorationDic)
		{
			if (_masterDataLoader.DecorationMaster.GetModelBySkin(skinType).CategoryType == categoryType && decorationData2.IsActive.CurrentValue)
			{
				return false;
			}
		}
		return true;
	}

	private DecorationSkinListItemModelForMobile.DeactivationCategoryType EnableDeactivation(DecorationService.DecorationCategoryType categoryType)
	{
		switch (categoryType)
		{
		case DecorationService.DecorationCategoryType.Book:
		case DecorationService.DecorationCategoryType.Badge:
			return DecorationSkinListItemModelForMobile.DeactivationCategoryType.Other;
		case DecorationService.DecorationCategoryType.Glasses:
			if (!_unlockItemService.Decoration.GetLockState(DecorationService.DecorationSkinType.Grasses_2).IsLocked.CurrentValue)
			{
				return DecorationSkinListItemModelForMobile.DeactivationCategoryType.Glass;
			}
			return DecorationSkinListItemModelForMobile.DeactivationCategoryType.None;
		default:
			return DecorationSkinListItemModelForMobile.DeactivationCategoryType.None;
		}
	}

	private bool IsCategoryKnown(DecorationService.DecorationCategoryType categoryType)
	{
		return DecorationListUI.IsCategoryKnown(categoryType, _masterDataLoader, _unlockItemService);
	}

	private DecorationService.DecorationModelType[] SearchModels(DecorationService.DecorationCategoryType categoryType)
	{
		return (from x in _masterDataLoader.DecorationMaster.GetModelTypesByCategory(categoryType)
			where x != DecorationService.DecorationModelType.Glasses_None && IsModelKnown(x)
			select x).ToArray();
	}

	private bool IsModelKnown(DecorationService.DecorationModelType modelType)
	{
		IEnumerable<DecorationSkinMasterData> skinsByModel = _masterDataLoader.DecorationMaster.GetSkinsByModel(modelType);
		if (skinsByModel.Any((DecorationSkinMasterData x) => IsSkinUnlocked(x.SkinType)))
		{
			return true;
		}
		if (skinsByModel.All((DecorationSkinMasterData x) => IsPurchasableType(x.SkinType)))
		{
			return true;
		}
		return false;
		bool IsPurchasableType(DecorationService.DecorationSkinType skinType)
		{
			int price;
			return _unlockItemService.Decoration.IsPurchasableType(skinType, out price);
		}
		bool IsSkinUnlocked(DecorationService.DecorationSkinType skinType)
		{
			return !_unlockItemService.Decoration.GetLockState(skinType).IsLocked.CurrentValue;
		}
	}

	private bool IsNewSkin(DecorationService.DecorationSkinType skinType)
	{
		UnlockDecoration.IDecorationUnlockData lockState = _unlockItemService.Decoration.GetLockState(skinType);
		if (lockState.IsNotLockCondition)
		{
			return false;
		}
		if (lockState.IsLocked.CurrentValue)
		{
			return false;
		}
		return !SaveDataManager.Instance.DecorationProgressData.PlayedDecoration.Contains(GetSkinTypeName(skinType));
	}

	private string GetSkinTypeName(DecorationService.DecorationSkinType skinType)
	{
		if (_decorationSkinTypeNameCache.TryGetValue(skinType, out var value))
		{
			return value;
		}
		Debug.LogError($"存在しないDecorationSkinTypeを指定しています {skinType}");
		return "";
	}

	private void UpdateMainSceneButtonNewIcon()
	{
		bool flag = false;
		foreach (Member<DecorationService.DecorationSkinType> member in FastEnum.GetMembers<DecorationService.DecorationSkinType>())
		{
			if (IsNewSkin(member.Value))
			{
				flag = true;
				break;
			}
		}
		if (_decorationMainSceneButtonNewIcon.gameObject.activeSelf != flag)
		{
			_decorationMainSceneButtonNewIcon.SetActive(flag);
		}
	}

	public async UniTask Activate()
	{
		UpdateCategoryKnown();
		bool isUnlockedCategory = _decorationWindowView.ListView.GetIsUnlockedCategory(_currentCategoryType);
		if (_isCurrentCategoryUnlocked != isUnlockedCategory)
		{
			_isDirtyData = true;
		}
		_isActive = true;
		_roomCameraManager?.SetActiveDecorationCam(isActive: true);
		_roomCameraManager?.SetActiveDecorationDeskCam(IsDeskCategory(_currentCategoryType));
		_roomCameraManager?.SetActiveDecorationChairCam(_currentCategoryType == DecorationService.DecorationCategoryType.Chair);
		await _decorationWindowView.Activate();
	}

	public async UniTask Deactivate()
	{
		_roomCameraManager?.SetActiveDecorationChairCam(isActive: false);
		_roomCameraManager?.SetActiveDecorationDeskCam(isActive: false);
		_roomCameraManager?.SetActiveDecorationCam(isActive: false, delegate
		{
		});
		await _decorationWindowView.Deactivate();
		_isActive = false;
	}

	private bool IsDeskCategory(DecorationService.DecorationCategoryType type)
	{
		switch (type)
		{
		case DecorationService.DecorationCategoryType.Cup:
		case DecorationService.DecorationCategoryType.BookLayout:
		case DecorationService.DecorationCategoryType.Book:
		case DecorationService.DecorationCategoryType.Keyboard:
		case DecorationService.DecorationCategoryType.StandLight:
		case DecorationService.DecorationCategoryType.Desk:
		case DecorationService.DecorationCategoryType.CoffeeMaker:
			return true;
		default:
			return false;
		}
	}
}
