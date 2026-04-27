using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class AddExpMeterUI : MonoBehaviour
{
	[Inject]
	private IPlayerLevelUIService _playerLevelUIService;

	[SerializeField]
	[Header("メーター画像\u3000経験値量によって上から表示するImage")]
	private Image _meterOverWriteImage;

	[SerializeField]
	[Header("回転速度")]
	private float _animSpeed;

	[SerializeField]
	[Header("サイズTo")]
	private float _toScale;

	[SerializeField]
	[Header("サイズ変更秒数")]
	private float _scaleDuration;

	private bool _isPlaying;

	private Vector3 _defaultScale;

	private Tween _tween;

	public void Setup()
	{
		_playerLevelUIService.OnUpdateUILevelData.Subscribe(delegate(LevelData x)
		{
			if (!_isPlaying)
			{
				StartAnim();
			}
			UpdateMeter(x);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_playerLevelUIService.OnEndAddExp, delegate
		{
			EndAnim();
		}).AddTo(this);
		_playerLevelUIService.OnEndLevelUP.Subscribe(delegate(LevelData x)
		{
			UpdateMeter(x);
		}).AddTo(this);
		_defaultScale = _meterOverWriteImage.transform.localScale;
		SyncWithSaveData();
	}

	private void StartAnim()
	{
		_tween?.Kill();
		_tween = _meterOverWriteImage.transform.DOScale(new Vector3(_toScale, _toScale, _toScale), _scaleDuration);
	}

	private void UpdateMeter(LevelData levelData)
	{
		if (levelData.CurrentExp == 0f)
		{
			_meterOverWriteImage.fillAmount = 0f;
		}
		else
		{
			_meterOverWriteImage.fillAmount = levelData.CurrentExp / levelData.NextLevelNecessaryExp;
		}
	}

	private void EndAnim()
	{
		_tween?.Kill();
		_tween = _meterOverWriteImage.transform.DOScale(_defaultScale, _scaleDuration);
	}

	public void SyncWithSaveData()
	{
		LevelData currentLevelData = LevelData.GetCurrentLevelData();
		if (currentLevelData != null)
		{
			UpdateMeter(currentLevelData);
		}
	}
}
