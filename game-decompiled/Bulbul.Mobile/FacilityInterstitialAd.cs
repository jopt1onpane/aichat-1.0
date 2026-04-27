using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace Bulbul.Mobile;

public class FacilityInterstitialAd : MonoBehaviour, IInterstitialAdController
{
	[Inject]
	private LoadingScreen _loadingScreen;

	[Inject]
	private ScenarioReader scenarioReader;

	[Inject]
	private SceneFadeControllerProvider fadeControllerProvider;

	[SerializeField]
	private InterstitialAdView _interstitialAdView;

	[SerializeField]
	private ObjectsActiveChecker _facilitiesWindowChecker;

	private bool _isShowingAd;

	bool IInterstitialAdController.IsNeedAd => IsNeedAd();

	public void Setup()
	{
		_isShowingAd = false;
		_interstitialAdView.Setup();
	}

	private bool IsNeedAd()
	{
		return false;
	}

	public bool CanShowAd()
	{
		if (_facilitiesWindowChecker.CheckActive())
		{
			ScreenOrientation orientation = Screen.orientation;
			if (orientation != ScreenOrientation.LandscapeLeft && orientation != ScreenOrientation.LandscapeRight)
			{
				goto IL_0039;
			}
		}
		if (!scenarioReader.IsPlayingLongStoryOrTutorial())
		{
			return fadeControllerProvider.Controller.IsComplete();
		}
		goto IL_0039;
		IL_0039:
		return false;
	}

	async UniTask IInterstitialAdController.ShowAdAsync(CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();
		if (_isShowingAd)
		{
			return;
		}
		try
		{
			ScreenOrientationManagerForMobile.Instance.SetAutoRotateTemporaryLock(isLock: true);
			_isShowingAd = true;
			_interstitialAdView.SetActiveAdRaycastBlocker(active: true);
			AdmobCtrl adMob = AdmobCtrl.GetInstance();
			adMob.CreateInterstitialAd();
			using (_loadingScreen.CreateLoadingScope())
			{
				_loadingScreen.SetActiveBg(active: false);
				await _interstitialAdView.PlayNoticeAnimation(ct);
				adMob.ShowInterstitialAd();
				await UniTask.WaitUntil(() => adMob.IsClosedInterstitialAd(), PlayerLoopTiming.Update, ct);
			}
		}
		finally
		{
			_interstitialAdView.SetActiveAdRaycastBlocker(active: false);
			_isShowingAd = false;
			ScreenOrientationManagerForMobile.Instance.SetAutoRotateTemporaryLock(isLock: false);
		}
	}
}
