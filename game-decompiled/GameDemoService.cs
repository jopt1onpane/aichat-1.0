using System;
using Bulbul;
using R3;
using UnityEngine;
using VContainer;

public class GameDemoService : MonoBehaviour
{
	[Inject]
	private IGameDemoView _gameDemoView;

	private Subject<float> _onChangeTime = new Subject<float>();

	private Subject<Unit> _onTimeLimit = new Subject<Unit>();

	private Subject<Unit> _onActivateUI = new Subject<Unit>();

	private bool _isUpdateDemoService;

	public Observable<float> OnChangeTime => _onChangeTime;

	public Observable<Unit> OnTimeLimit => _onTimeLimit;

	public Observable<Unit> OnActivateUI => _onActivateUI;

	public void Setup()
	{
		base.gameObject.SetActive(value: true);
		_gameDemoView.Setup(this);
		if (IsElapsedTimeLimit())
		{
			_onTimeLimit.OnNext(Unit.Default);
			return;
		}
		ObservableSubscribeExtensions.Subscribe(Observable.Interval(TimeSpan.FromSeconds(5.0)), delegate
		{
			SaveDataManager.Instance.SavePlayerData();
		});
		_onActivateUI.OnNext(Unit.Default);
		ObservableSubscribeExtensions.Subscribe(RoomLifetimeScope.Resolve<TutorialService>().OnClose, delegate
		{
		}).AddTo(this);
	}

	public void StartUpdate()
	{
		_isUpdateDemoService = true;
	}

	private void Update()
	{
		if (!_isUpdateDemoService)
		{
			return;
		}
		if (SaveDataManager.Instance.PlayerData.DemoRemainSeconds > 0f)
		{
			SaveDataManager.Instance.PlayerData.DemoRemainSeconds -= Time.deltaTime;
			if (SaveDataManager.Instance.PlayerData.DemoRemainSeconds < 0f)
			{
				SaveDataManager.Instance.PlayerData.DemoRemainSeconds = 0f;
				_onTimeLimit.OnNext(Unit.Default);
				SaveDataManager.Instance.SavePlayerData();
			}
		}
		_onChangeTime.OnNext(SaveDataManager.Instance.PlayerData.DemoRemainSeconds);
	}

	public void ShowTimeLimit()
	{
	}

	public void UpdateTimeLimit()
	{
	}

	public bool IsElapsedTimeLimit()
	{
		if (SaveDataManager.Instance.PlayerData.DemoRemainSeconds <= 0f)
		{
			return true;
		}
		return false;
	}
}
