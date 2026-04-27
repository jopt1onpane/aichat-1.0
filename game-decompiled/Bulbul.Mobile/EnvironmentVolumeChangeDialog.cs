using System.Collections.Generic;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul.Mobile;

public class EnvironmentVolumeChangeDialog : MonoBehaviour
{
	[SerializeField]
	private EnvironmentVolumeListView _volumeChangeListView;

	[SerializeField]
	private Button[] _closeButtons;

	[SerializeField]
	private CommonWindowShowHideAnimation _showHideAnimation;

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private EnvironmentDataService _environmentDataService;

	[Inject]
	private EnvironmentApplicationService _applicationService;

	[Inject]
	private UnlockItemService _unlockItemService;

	[Inject]
	private MobileDemoEditionLockedTargetCheckService _mobileDemoEditionLockedTargetCheckService;

	private readonly List<EnvironmentVolumeItemModel> _models = new List<EnvironmentVolumeItemModel>();

	private readonly Subject<Unit> _onClose = new Subject<Unit>();

	public Observable<Unit> OnClose => _onClose;

	public void Initialize()
	{
		base.gameObject.SetActive(value: true);
		_volumeChangeListView.Initialize();
		_showHideAnimation.DeactivateOnHide = true;
		Button[] closeButtons = _closeButtons;
		for (int i = 0; i < closeButtons.Length; i++)
		{
			ObservableSubscribeExtensions.Subscribe(closeButtons[i].OnClickAsObservable(), delegate
			{
				Close();
			}).AddTo(this);
		}
		_volumeChangeListView.OnClickItemMainButton.Subscribe(delegate(EnvironmentType envType)
		{
			envType.TryConvertToAmbientSoundType(out var ambientSoundType);
			var (environmentVolumeItemModel, modelIndex) = GetItemModel(envType);
			if (!environmentVolumeItemModel.IsSoundActive)
			{
				float item = _environmentDataService.GetVolume(ambientSoundType).volume;
				environmentVolumeItemModel.Volume = ((item > 0f) ? item : 0.5f);
				bool isNew = environmentVolumeItemModel.IsNew;
				environmentVolumeItemModel.IsNew = false;
				_environmentDataService.SetVolume(ambientSoundType, environmentVolumeItemModel.Volume);
				_environmentDataService.SetMute(ambientSoundType, !environmentVolumeItemModel.IsSoundActive);
				if (isNew)
				{
					_environmentDataService.SetPlayed(environmentVolumeItemModel.EnvironmentType);
				}
			}
			else
			{
				environmentVolumeItemModel.Volume = 0f;
				_environmentDataService.SetMute(ambientSoundType, !environmentVolumeItemModel.IsSoundActive);
			}
			_volumeChangeListView.ReapplyModel(modelIndex);
			_applicationService.ApplySound(ambientSoundType, environmentVolumeItemModel.IsSoundActive, environmentVolumeItemModel.Volume);
		}).AddTo(this);
		_volumeChangeListView.OnVolumeSliderChange.Subscribe(delegate((EnvironmentType environmentType, float volume) x)
		{
			x.environmentType.TryConvertToAmbientSoundType(out var ambientSoundType);
			(EnvironmentVolumeItemModel model, int index) itemModel = GetItemModel(x.environmentType);
			EnvironmentVolumeItemModel item = itemModel.model;
			int item2 = itemModel.index;
			item.Volume = x.volume;
			bool isNew = item.IsNew;
			item.IsNew = false;
			_environmentDataService.SetVolume(ambientSoundType, item.Volume);
			_environmentDataService.SetMute(ambientSoundType, !item.IsSoundActive);
			if (isNew && item.IsSoundActive)
			{
				_environmentDataService.SetPlayed(item.EnvironmentType);
			}
			_volumeChangeListView.ReapplyModel(item2);
			_applicationService.ApplySound(ambientSoundType, item.IsSoundActive, item.Volume);
		}).AddTo(this);
		_showHideAnimation.Hide(immediate: true);
	}

	public void Open()
	{
	}

	private int GetSortOrder(EnvironmentType envType)
	{
		return _masterDataLoader.EnvironmentMaster.GetEnvironment(envType).SortOrder;
	}

	private void Close()
	{
		_showHideAnimation.Hide();
		_onClose.OnNext(Unit.Default);
	}

	private (EnvironmentVolumeItemModel model, int index) GetItemModel(EnvironmentType environmentType)
	{
		int num = _models.FindIndex((EnvironmentVolumeItemModel model) => model.EnvironmentType == environmentType);
		if (num == -1)
		{
			return (model: null, index: -1);
		}
		return (model: _models[num], index: num);
	}
}
