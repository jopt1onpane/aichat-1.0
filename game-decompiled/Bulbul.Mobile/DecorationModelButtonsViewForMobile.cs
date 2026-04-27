using R3;
using UnityEngine;
using VContainer;

namespace Bulbul.Mobile;

public class DecorationModelButtonsViewForMobile : MonoBehaviour
{
	[Inject]
	private SystemSeService _systemSeService;

	[SerializeField]
	private DecorationModelButtonViewForMobile[] _buttons;

	[SerializeField]
	private DecorationModelTypeIconDBForMobile _modelIconDB;

	private DecorationService.DecorationModelType[] _currentModelTypes;

	private Subject<DecorationService.DecorationModelType> _onChangedDecorationModelType = new Subject<DecorationService.DecorationModelType>();

	private int _currentIdx = -1;

	private DecorationService.DecorationCategoryType _currentCategory;

	public Observable<DecorationService.DecorationModelType> OnChangedDecorationModelType => _onChangedDecorationModelType;

	private void OnDestroy()
	{
		_onChangedDecorationModelType.Dispose();
	}

	private void Start()
	{
		DecorationModelButtonViewForMobile[] buttons = _buttons;
		for (int i = 0; i < buttons.Length; i++)
		{
			buttons[i].SetSelected(isSelected: false);
		}
		if (_currentIdx != -1)
		{
			_buttons[_currentIdx].SetSelected(isSelected: true);
		}
	}

	public void Setup()
	{
		int num = 0;
		DecorationModelButtonViewForMobile[] buttons = _buttons;
		foreach (DecorationModelButtonViewForMobile obj in buttons)
		{
			int i2 = num;
			ObservableSubscribeExtensions.Subscribe(obj.OnClickButton, delegate
			{
				_systemSeService.PlaySelect();
				OnClickButton(i2);
			}).AddTo(this);
			num++;
		}
	}

	public void Set(DecorationService.DecorationCategoryType categoryType, DecorationService.DecorationModelType[] modelTypes)
	{
		_currentCategory = categoryType;
		_currentModelTypes = modelTypes;
		int num = 0;
		DecorationModelButtonViewForMobile[] buttons = _buttons;
		foreach (DecorationModelButtonViewForMobile decorationModelButtonViewForMobile in buttons)
		{
			if (num >= modelTypes.Length)
			{
				decorationModelButtonViewForMobile.Deactivate();
				continue;
			}
			decorationModelButtonViewForMobile.Activate();
			DecorationService.DecorationModelType decorationModelType = modelTypes[num];
			(Sprite, Sprite) modelTypeIcon = _modelIconDB.GetModelTypeIcon(decorationModelType);
			decorationModelButtonViewForMobile.SetIcon(modelTypeIcon.Item1, modelTypeIcon.Item2);
			num++;
		}
	}

	public void SelectButton(int idx, bool isForce = false)
	{
		if (_currentIdx != idx || isForce)
		{
			if (_currentIdx != -1)
			{
				_buttons[_currentIdx].SetSelected(isSelected: false);
			}
			_buttons[idx].SetSelected(isSelected: true);
			_currentIdx = idx;
			_onChangedDecorationModelType.OnNext(_currentModelTypes[idx]);
		}
	}

	private void OnClickButton(int idx)
	{
		SelectButton(idx);
	}

	public void HideAllButtons()
	{
		DecorationModelButtonViewForMobile[] buttons = _buttons;
		for (int i = 0; i < buttons.Length; i++)
		{
			buttons[i].Deactivate();
		}
	}
}
