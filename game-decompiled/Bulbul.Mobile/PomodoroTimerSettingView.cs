using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class PomodoroTimerSettingView : MonoBehaviour
{
	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private PomodoroTimerUI _pomodoroTimerUI;

	[SerializeField]
	private FacilityCommonActivateAnimationMobile _activator;

	[SerializeField]
	[Header("未購入者にadが出ることを周知するためのテキスト")]
	private TextMeshProUGUI _adNoticeText;

	private Subject<Unit> _onClose = new Subject<Unit>();

	private Subject<Unit> _onClickSkip = new Subject<Unit>();

	private Subject<Unit> _onClickPlay = new Subject<Unit>();

	private Subject<Unit> _onClickReset = new Subject<Unit>();

	public Observable<Unit> OnClose => _onClose;

	public Observable<Unit> OnClickSlip => _onClickSkip;

	public Observable<Unit> OnClickPlay => _onClickPlay;

	public Observable<Unit> OnClickReset => _onClickReset;

	public bool IsActive => base.gameObject.activeSelf;

	public void Setup()
	{
		_activator.Setup();
		_adNoticeText.gameObject.SetActive(value: true);
		ObservableSubscribeExtensions.Subscribe(_closeButton.OnClickAsObservable(), delegate
		{
			_onClose.OnNext(Unit.Default);
		}).AddTo(this);
		_pomodoroTimerUI.OnClickButtonSkipPomodoro.Subscribe(delegate(Unit _)
		{
			_onClickSkip.OnNext(_);
		}).AddTo(this);
		_pomodoroTimerUI.OnClickButtonReset.Subscribe(delegate(Unit _)
		{
			_onClickReset.OnNext(_);
		}).AddTo(this);
		_pomodoroTimerUI.OnClickButtonStartPomodoro.Subscribe(delegate(Unit _)
		{
			_onClickPlay.OnNext(_);
		}).AddTo(this);
	}

	public void Activate()
	{
		_activator.Activate();
	}

	public async UniTask Deactivate()
	{
		await _activator.Deactivate();
	}

	public void SetActiveAdNoticeText(bool active)
	{
		_adNoticeText.enabled = active;
	}
}
