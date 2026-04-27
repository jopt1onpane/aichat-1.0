using DG.Tweening;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class AddExpOutsideRotateAnim : MonoBehaviour
{
	[Inject]
	private IPlayerLevelUIService _playerLevelUIService;

	[SerializeField]
	[Header("経験値上昇時に回転させるImage")]
	private Image _addExpRotateImage;

	[SerializeField]
	[Header("回転速度")]
	private float _animSpeed;

	[SerializeField]
	[Header("フェード秒数")]
	private float _fadeDuration;

	private bool _isPlaying;

	private Tween _tween;

	public void Setup()
	{
		ObservableSubscribeExtensions.Subscribe(_playerLevelUIService.OnUpdateUILevelData, delegate
		{
			if (!_isPlaying)
			{
				StartAnim();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(from x in this.UpdateAsObservable()
			where _isPlaying
			select x, delegate
		{
			UpdateAnim();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_playerLevelUIService.OnEndAddExp, delegate
		{
			EndAnim();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_playerLevelUIService.OnReadyLevelUP, delegate
		{
			EndAnim();
		}).AddTo(this);
	}

	private void StartAnim()
	{
		if (!_isPlaying)
		{
			_isPlaying = true;
			_addExpRotateImage.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
			_tween?.Kill();
			_tween = _addExpRotateImage.DOFade(1f, _fadeDuration);
		}
	}

	private void UpdateAnim()
	{
		Vector3 eulerAngles = _addExpRotateImage.transform.rotation.eulerAngles;
		eulerAngles.z -= _animSpeed * Time.deltaTime;
		_addExpRotateImage.transform.rotation = Quaternion.Euler(eulerAngles);
	}

	private void EndAnim()
	{
		_tween?.Kill();
		_tween = _addExpRotateImage.DOFade(0f, _fadeDuration).OnComplete(delegate
		{
			_isPlaying = false;
		});
	}
}
