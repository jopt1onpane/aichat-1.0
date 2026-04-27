using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class ShopBannerView : MonoBehaviour
{
	[SerializeField]
	private BannerView _bannerView;

	[SerializeField]
	private Button _backButton;

	[SerializeField]
	private Button _nextButton;

	[SerializeField]
	private float _switchSec;

	private float _timer;

	public void Setup()
	{
		_backButton.gameObject.SetActive(_bannerView.ContentsSum > 1);
		_nextButton.gameObject.SetActive(_bannerView.ContentsSum > 1);
		_bannerView.Setup();
		_bannerView.SetBannerIdx(0);
		ObservableSubscribeExtensions.Subscribe(_backButton.OnClickAsObservable(), delegate
		{
			_bannerView.MoveBackBanner();
			_timer = 0f;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_nextButton.OnClickAsObservable(), delegate
		{
			_bannerView.MoveNextBanner();
			_timer = 0f;
		}).AddTo(this);
	}

	public void ResetBannerIdx()
	{
		_bannerView.SetBannerIdx(0);
	}

	public void Update()
	{
		if (_bannerView.ContentsSum > 1)
		{
			_timer += Time.deltaTime;
			if (_timer >= _switchSec)
			{
				_timer = 0f;
				_bannerView.MoveNextBanner();
			}
		}
	}

	private void OnDisable()
	{
		_timer = 0f;
	}
}
