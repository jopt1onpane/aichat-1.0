using R3;
using UnityEngine;
using VContainer;

namespace Bulbul.Mobile;

public class NewEnvironmentMarkUIMobile : MonoBehaviour
{
	[Inject]
	private EnvironmentDataService _environmentDataService;

	[Inject]
	private UnlockItemService _unlockItemService;

	[SerializeField]
	private NewItemIcon _newIcon;

	private void Start()
	{
		_newIcon.SetIconActive(NewItemExists());
		ObservableSubscribeExtensions.Subscribe(_unlockItemService.Environment.OnLockStateChanged, delegate
		{
			_newIcon.SetIconActive(NewItemExists());
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_environmentDataService.OnFirstPlayed, delegate
		{
			_newIcon.SetIconActive(NewItemExists());
		}).AddTo(this);
	}

	private bool NewItemExists()
	{
		foreach (var (environmentType, environmentUnlockData) in _unlockItemService.Environment.LockStates)
		{
			if (!environmentUnlockData.IsNotLockCondition && !environmentUnlockData.IsLocked.CurrentValue && !_environmentDataService.HavePlayed(environmentType))
			{
				return true;
			}
		}
		return false;
	}
}
