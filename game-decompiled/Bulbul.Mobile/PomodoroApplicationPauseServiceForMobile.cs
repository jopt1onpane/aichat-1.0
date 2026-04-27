using System;
using R3;
using VContainer;
using VContainer.Unity;

namespace Bulbul.Mobile;

public class PomodoroApplicationPauseServiceForMobile : IInitializable, IDisposable
{
	[Inject]
	private OnApplicationPauseService _applicationPauseService;

	[Inject]
	private PomodoroService _pomodoroService;

	[Inject]
	private IRoomGameSceneState _roomGameSceneState;

	private DisposableBag _disposableBag;

	private void Setup()
	{
		ObservableSubscribeExtensions.Subscribe(_applicationPauseService.OnSuspend, delegate
		{
			_ = _roomGameSceneState.CurrentMainState;
			_ = 14;
		}).AddTo(ref _disposableBag);
		_applicationPauseService.OnResume.Subscribe(delegate((DateTime, double) timeData)
		{
			if (_roomGameSceneState.CurrentMainState == RoomGameManager.MainState.Idle)
			{
				_pomodoroService.MoveAheadTimer((float)timeData.Item2);
			}
		}).AddTo(ref _disposableBag);
	}

	public void Dispose()
	{
		_disposableBag.Dispose();
	}

	void IInitializable.Initialize()
	{
		Setup();
	}
}
