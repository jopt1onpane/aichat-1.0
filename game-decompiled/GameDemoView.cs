using Bulbul;
using DG.Tweening;
using R3;
using TMPro;
using UnityEngine;

public class GameDemoView : MonoBehaviour, IGameDemoView
{
	[SerializeField]
	private GameObject _timeLimitObject;

	[SerializeField]
	private TextMeshProUGUI _timeText;

	[SerializeField]
	private CanvasGroup _playEndCanvasGroup;

	private GameDemoService _gameDemoService;

	private int _displayHour;

	private int _displayMinute;

	public bool IsPlayEndCanvasActiveSelf => _playEndCanvasGroup.gameObject.activeSelf;

	public bool IsTimeLimitObjectActiveSelf => _timeLimitObject.activeSelf;

	private void Awake()
	{
		_timeLimitObject.SetActive(value: false);
		_playEndCanvasGroup.gameObject.SetActive(value: false);
	}

	public void Setup(GameDemoService gameDemoService)
	{
		_gameDemoService = gameDemoService;
		_playEndCanvasGroup.gameObject.SetActive(value: false);
		_playEndCanvasGroup.alpha = 0f;
		_gameDemoService.OnChangeTime.Subscribe(delegate(float dateTime)
		{
			UpdateDateAndTime(dateTime);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_gameDemoService.OnTimeLimit, delegate
		{
			ActivateThankyouForPlaying();
		}).AddTo(this);
		UpdateDateAndTime(SaveDataManager.Instance.PlayerData.DemoRemainSeconds);
		_timeLimitObject.SetActive(value: true);
	}

	private void UpdateDateAndTime(float remainSeconds)
	{
		int num = (int)remainSeconds / 3600;
		int num2 = (int)remainSeconds % 3600 / 60;
		int num3 = (int)remainSeconds % 60;
		_timeText.text = $"{num:00}:{num2:00}:{num3:00}";
	}

	public void ActivateTimeLimit()
	{
		_timeLimitObject.gameObject.SetActive(value: true);
	}

	public void DeactivateTimeLimit()
	{
		_timeLimitObject.gameObject.SetActive(value: false);
	}

	public void ActivateThankyouForPlaying()
	{
		_playEndCanvasGroup.gameObject.SetActive(value: true);
		_playEndCanvasGroup.DOFade(1f, 0.5f);
	}

	public void DeactivateThankyouForPlaying()
	{
		_playEndCanvasGroup.gameObject.SetActive(value: false);
		_playEndCanvasGroup.DOFade(0f, 0f);
	}
}
