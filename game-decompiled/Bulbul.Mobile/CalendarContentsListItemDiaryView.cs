using R3;
using UnityEngine;

namespace Bulbul.Mobile;

public class CalendarContentsListItemDiaryView : MonoBehaviour
{
	[SerializeField]
	[Header("日記部分以外の高さ")]
	private float _heightExcludingDiary;

	[SerializeField]
	private AutoSizingHeightInputFieldView _autoSizingHeightInputFieldView;

	private Subject<(float, bool)> _onValueChangedHeight = new Subject<(float, bool)>();

	private bool _isInit;

	public AutoSizingHeightInputFieldView AutoSizingHeightInputFieldView => _autoSizingHeightInputFieldView;

	public float HeightExcludingDiary => _heightExcludingDiary;

	public float TotalHeight => _heightExcludingDiary + _autoSizingHeightInputFieldView.Height;

	public Observable<(float, bool)> OnValueChangedHeight => _onValueChangedHeight;

	private void Init()
	{
		_autoSizingHeightInputFieldView.OnValueChangedHeight.Subscribe(delegate((float, float, bool) heights)
		{
			_onValueChangedHeight.OnNext((heights.Item2 + _heightExcludingDiary, heights.Item3));
		}).AddTo(this);
		_isInit = true;
	}

	public void UpdateView(CalendarDiaryViewModel model)
	{
		if (!_isInit)
		{
			Init();
		}
		_autoSizingHeightInputFieldView.SetText(model.DiaryText, isNoticeChagedText: false, isChangeHeight: true);
	}
}
