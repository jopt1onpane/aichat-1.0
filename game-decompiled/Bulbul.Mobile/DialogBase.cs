using System.Threading;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul.Mobile;

public class DialogBase : MonoBehaviour
{
	[Inject]
	protected SystemSeService _systemSeService;

	[SerializeField]
	private Button[] _closeButtons;

	[SerializeField]
	private CommonWindowShowHideAnimation _showHideAnimation;

	private readonly Subject<Unit> _onClose = new Subject<Unit>();

	private CancellationTokenSource _showCts;

	public Observable<Unit> OnClose => _onClose;

	protected bool IsShowing => _showHideAnimation.IsShowing;

	protected bool IsAnimating => _showHideAnimation.IsAnimating;

	protected bool AllowCloseByButtons { get; set; } = true;

	public CancellationToken ShowCancellationToken => _showCts.Token;

	protected void Init()
	{
		_showHideAnimation.DeactivateOnHide = true;
		Button[] closeButtons = _closeButtons;
		for (int i = 0; i < closeButtons.Length; i++)
		{
			ObservableSubscribeExtensions.Subscribe(closeButtons[i].OnClickAsObservable(), delegate
			{
				if (AllowCloseByButtons)
				{
					_systemSeService.PlayCancelSelect();
					Hide(invokeClose: false);
				}
			}).AddTo(this);
		}
		_showHideAnimation.Hide(immediate: true);
	}

	protected void Show()
	{
		_showCts?.Cancel();
		_showCts?.Dispose();
		_showHideAnimation.Show();
		_showCts = new CancellationTokenSource();
	}

	protected void Hide(bool invokeClose = true)
	{
		_showCts?.Cancel();
		_showCts?.Dispose();
		_showCts = null;
		_showHideAnimation.Hide();
		if (invokeClose)
		{
			_onClose.OnNext(Unit.Default);
		}
	}
}
