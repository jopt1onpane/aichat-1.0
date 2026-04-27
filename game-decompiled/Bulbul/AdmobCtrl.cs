using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

namespace Bulbul;

public class AdmobCtrl
{
	private enum MAIN_STATE
	{
		IDLE,
		WAIT_ORDER
	}

	private struct BannarAd
	{
		public BannerView View;

		public float Height;
	}

	private enum INTERSTITIAL_STATE
	{
		NONE = -1,
		IDLE,
		LOADING,
		WAIT_SHOW,
		WAIT_CLOSE,
		CLOSED
	}

	public enum INTERSTITIAL_ERROR
	{
		NONE,
		LOAD_FAILED,
		LOAD_UNKNOWN_FAILED,
		CANT_SHOW_AD,
		SHOW_FAILED
	}

	private enum REWARD_STATE
	{
		NONE = -1,
		IDLE,
		LOADING,
		WAIT_SHOW,
		WAIT_CLOSE,
		CLOSED
	}

	public enum REWARD_ERROR
	{
		NONE,
		LOAD_FAILED,
		LOAD_UNKNOWN_FAILED,
		CANT_SHOW_AD,
		SHOW_FAILED
	}

	private static readonly bool ADMOB_DEBUG = false;

	private static AdmobCtrl instance = new AdmobCtrl();

	private MAIN_STATE mainState;

	private bool isInitialize;

	private bool isDuringConsent;

	private GameObject consentForm;

	private BannarAd portraitBannerAd;

	private BannarAd landscapeBannerAd;

	private static readonly string adUnitId_B = "ca-app-pub-3940256099942544/2435281174";

	private InterstitialAd interstitialAd;

	private bool isCalledShowInterstitial;

	private INTERSTITIAL_STATE interstitialState;

	private INTERSTITIAL_ERROR errorByCloseInterstitialAd;

	private static readonly string adUnitId_I = "ca-app-pub-3940256099942544/4411468910";

	private RewardedAd rewardedAd;

	private bool isCalledShowReward;

	public bool isSuccessReward;

	private REWARD_STATE rewardState;

	private REWARD_STATE pre_rewardState = REWARD_STATE.NONE;

	private REWARD_ERROR errorByCloseRewardAd;

	private static readonly string adUnitId_R = "ca-app-pub-3940256099942544/1712485313";

	public bool CanRequestAds
	{
		get
		{
			if (ConsentInformation.ConsentStatus != ConsentStatus.Obtained)
			{
				return ConsentInformation.ConsentStatus == ConsentStatus.NotRequired;
			}
			return true;
		}
	}

	public bool NoNeedConsent => ConsentInformation.ConsentStatus == ConsentStatus.NotRequired;

	public bool RequirePrivacyOption => ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;

	public static AdmobCtrl GetInstance()
	{
		return instance;
	}

	public AdmobCtrl()
	{
		isInitialize = false;
		isDuringConsent = false;
		consentForm = null;
		portraitBannerAd.Height = 0f;
		portraitBannerAd.View = null;
		landscapeBannerAd.Height = 0f;
		landscapeBannerAd.View = null;
		interstitialAd = null;
		isCalledShowInterstitial = false;
		interstitialState = INTERSTITIAL_STATE.IDLE;
		rewardedAd = null;
		isCalledShowReward = false;
		isSuccessReward = false;
		rewardState = REWARD_STATE.IDLE;
	}

	private async UniTask<bool> InitializeUnityService()
	{
		bool result = false;
		try
		{
			if (UnityServices.State == ServicesInitializationState.Uninitialized)
			{
				await UnityServices.InitializeAsync(new InitializationOptions().SetEnvironmentName("production"));
			}
			result = true;
		}
		catch (Exception)
		{
		}
		return result;
	}

	public async UniTask<bool> Initialize(bool ads_init)
	{
		isDuringConsent = true;
		if (!isInitialize)
		{
			if (!(await InitializeUnityService()))
			{
				return false;
			}
			ConsentRequestParameters request;
			if (ADMOB_DEBUG)
			{
				string item = SystemInfo.deviceUniqueIdentifier.ToUpper();
				request = new ConsentRequestParameters
				{
					TagForUnderAgeOfConsent = false,
					ConsentDebugSettings = new ConsentDebugSettings
					{
						DebugGeography = DebugGeography.EEA,
						TestDeviceHashedIds = new List<string> { item }
					}
				};
			}
			else
			{
				request = new ConsentRequestParameters
				{
					TagForUnderAgeOfConsent = false
				};
			}
			ConsentInformation.Update(request, delegate(FormError updateError)
			{
				OnConsentInformationUpdateAsync(ads_init, updateError).Forget();
			});
		}
		else
		{
			isDuringConsent = false;
		}
		return true;
	}

	private async UniTask OnConsentInformationUpdateAsync(bool ads_init, FormError updateError)
	{
		await UniTask.SwitchToMainThread();
		if (updateError != null)
		{
			Debug.LogError("*****: ConsentInformation.Update: GDPRの同意ステータスを取得できません: " + updateError.ToString());
			isDuringConsent = false;
			return;
		}
		if (CanRequestAds)
		{
			if (ads_init)
			{
				MobileAdsInit();
			}
			else
			{
				isDuringConsent = false;
			}
			return;
		}
		ConsentForm.LoadAndShowConsentFormIfRequired(async delegate(FormError showError)
		{
			await UniTask.SwitchToMainThread();
			if (showError != null)
			{
				isDuringConsent = false;
			}
			else if (CanRequestAds && ads_init)
			{
				MobileAdsInit();
			}
			else
			{
				isDuringConsent = false;
			}
		});
	}

	private void MobileAdsInit()
	{
		MobileAds.Initialize(async delegate(InitializationStatus initStatus)
		{
			await UniTask.SwitchToMainThread();
			if (initStatus != null)
			{
				Dictionary<string, AdapterStatus> adapterStatusMap = initStatus.getAdapterStatusMap();
				if (adapterStatusMap != null)
				{
					foreach (KeyValuePair<string, AdapterStatus> item in adapterStatusMap)
					{
						_ = item;
					}
				}
				isInitialize = true;
			}
			isDuringConsent = false;
		});
	}

	public void ShowPrivacyOptionsForm(bool ads_init)
	{
		isDuringConsent = true;
		ConsentForm.ShowPrivacyOptionsForm(async delegate(FormError showError)
		{
			await UniTask.SwitchToMainThread();
			if (showError != null)
			{
				Debug.LogError($"*****: ConsentForm.ShowPrivacyOptionsForm: ErrorCode={showError.ErrorCode}, {showError.Message}");
				isDuringConsent = false;
			}
			else if (CanRequestAds && ads_init)
			{
				if (!isInitialize)
				{
					MobileAdsInit();
				}
				else
				{
					isDuringConsent = false;
				}
			}
			else
			{
				isDuringConsent = false;
			}
		});
	}

	public void DeactivateAllAd()
	{
		DestroyBannerAds();
		DestroyInterstitialAd();
		DestroyRewardAd();
	}

	public void Update()
	{
		if (isDuringConsent && consentForm == null)
		{
			consentForm = GameObject.Find("/ConsentForm(Clone)");
			if (consentForm != null)
			{
				consentForm.GetComponent<Canvas>().sortingOrder = 1010;
			}
		}
		InterstitialUpdate();
		RewardUpdate();
	}

	public bool IsCanRequestAd()
	{
		if (isInitialize)
		{
			return CanRequestAds;
		}
		return false;
	}

	public void DestroyBannerAds()
	{
		DestroyPortraitBannerAd();
		DestroyLandscapeBannerAd();
	}

	private string GetBannerAdUnitID()
	{
		string text = null;
		if (ADMOB_DEBUG)
		{
			return "ca-app-pub-3940256099942544/9214589741";
		}
		return adUnitId_B;
	}

	public void CreatePortraitBannerAd()
	{
		if (isInitialize)
		{
			DestroyPortraitBannerAd();
			AdSize portraitAnchoredAdaptiveBannerAdSizeWithWidth = AdSize.GetPortraitAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
			AdPosition position = AdPosition.Bottom;
			string bannerAdUnitID = GetBannerAdUnitID();
			portraitBannerAd.View = new BannerView(bannerAdUnitID, portraitAnchoredAdaptiveBannerAdSizeWithWidth, position);
			ListenToPortraitBannerAdEvents();
			AdRequest request = new AdRequest();
			portraitBannerAd.View.LoadAd(request);
		}
	}

	public void DestroyPortraitBannerAd()
	{
		if (portraitBannerAd.View != null)
		{
			portraitBannerAd.View.Destroy();
			portraitBannerAd.View = null;
			portraitBannerAd.Height = 0f;
		}
	}

	public void CreateOrShowPortraitBannerAd()
	{
		if (portraitBannerAd.View == null)
		{
			CreatePortraitBannerAd();
		}
		else
		{
			portraitBannerAd.View.Show();
		}
	}

	public void HidePortraitBannerAd()
	{
		if (portraitBannerAd.View != null)
		{
			portraitBannerAd.View.Hide();
		}
	}

	private void ListenToPortraitBannerAdEvents()
	{
		portraitBannerAd.View.OnBannerAdLoaded += delegate
		{
			if (portraitBannerAd.View != null)
			{
				portraitBannerAd.Height = portraitBannerAd.View.GetHeightInPixels();
			}
		};
		portraitBannerAd.View.OnBannerAdLoadFailed += delegate(LoadAdError error)
		{
			Debug.LogError("***BannerAd Portrait***: OnBannerAdLoadFailed: " + error);
			DeactivateAllAd();
		};
		portraitBannerAd.View.OnAdPaid += delegate
		{
		};
		portraitBannerAd.View.OnAdImpressionRecorded += delegate
		{
		};
		portraitBannerAd.View.OnAdClicked += delegate
		{
		};
		portraitBannerAd.View.OnAdFullScreenContentOpened += delegate
		{
		};
		portraitBannerAd.View.OnAdFullScreenContentClosed += delegate
		{
		};
	}

	public void CreateLandscapeBannerAd()
	{
		if (isInitialize)
		{
			DestroyLandscapeBannerAd();
			AdSize landscapeAnchoredAdaptiveBannerAdSizeWithWidth = AdSize.GetLandscapeAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
			AdPosition position = AdPosition.Bottom;
			string bannerAdUnitID = GetBannerAdUnitID();
			landscapeBannerAd.View = new BannerView(bannerAdUnitID, landscapeAnchoredAdaptiveBannerAdSizeWithWidth, position);
			ListenToLandscapeBannerAdEvents();
			AdRequest request = new AdRequest();
			landscapeBannerAd.View.LoadAd(request);
		}
	}

	public void DestroyLandscapeBannerAd()
	{
		if (landscapeBannerAd.View != null)
		{
			landscapeBannerAd.View.Destroy();
			landscapeBannerAd.View = null;
			landscapeBannerAd.Height = 0f;
		}
	}

	public void CreateOrShowLandscapeBannerAd()
	{
		if (landscapeBannerAd.View == null)
		{
			CreateLandscapeBannerAd();
		}
		else
		{
			landscapeBannerAd.View.Show();
		}
	}

	public void HideLandscapeBannerAd()
	{
		if (landscapeBannerAd.View != null)
		{
			landscapeBannerAd.View.Hide();
		}
	}

	private void ListenToLandscapeBannerAdEvents()
	{
		landscapeBannerAd.View.OnBannerAdLoaded += delegate
		{
			if (landscapeBannerAd.View != null)
			{
				landscapeBannerAd.Height = landscapeBannerAd.View.GetHeightInPixels();
			}
		};
		landscapeBannerAd.View.OnBannerAdLoadFailed += delegate(LoadAdError error)
		{
			Debug.LogError("***BannerAd Landspace***: OnBannerAdLoadFailed: " + error);
			DeactivateAllAd();
		};
		landscapeBannerAd.View.OnAdPaid += delegate
		{
		};
		landscapeBannerAd.View.OnAdImpressionRecorded += delegate
		{
		};
		landscapeBannerAd.View.OnAdClicked += delegate
		{
		};
		landscapeBannerAd.View.OnAdFullScreenContentOpened += delegate
		{
		};
		landscapeBannerAd.View.OnAdFullScreenContentClosed += delegate
		{
		};
	}

	public bool IsClosedInterstitialAd()
	{
		return interstitialState == INTERSTITIAL_STATE.CLOSED;
	}

	public INTERSTITIAL_ERROR GetErrorByCloseInterstitialAd()
	{
		return errorByCloseInterstitialAd;
	}

	private void InterstitialUpdate()
	{
		switch (interstitialState)
		{
		case INTERSTITIAL_STATE.WAIT_SHOW:
			if (isCalledShowInterstitial)
			{
				isCalledShowInterstitial = false;
				ShowInterstitialAd();
			}
			break;
		case INTERSTITIAL_STATE.IDLE:
		case INTERSTITIAL_STATE.LOADING:
		case INTERSTITIAL_STATE.WAIT_CLOSE:
		case INTERSTITIAL_STATE.CLOSED:
			break;
		}
	}

	public void CreateInterstitialAd()
	{
		if (!IsCanRequestAd())
		{
			Debug.LogWarning("*****: CreateInterstitialAd: Not initialize or Cant request ads");
			return;
		}
		if (interstitialAd != null)
		{
			DestroyInterstitialAd();
		}
		if (interstitialAd != null)
		{
			return;
		}
		interstitialState = INTERSTITIAL_STATE.LOADING;
		AdRequest request = new AdRequest();
		string text = null;
		text = ((!ADMOB_DEBUG) ? adUnitId_I : "ca-app-pub-3940256099942544/1033173712");
		InterstitialAd.Load(text, request, async delegate(InterstitialAd ad, LoadAdError error)
		{
			await UniTask.SwitchToMainThread();
			if (error != null)
			{
				Debug.LogError("Interstitial ad failed to load an ad with error : " + error);
				errorByCloseInterstitialAd = INTERSTITIAL_ERROR.LOAD_FAILED;
				interstitialState = INTERSTITIAL_STATE.CLOSED;
			}
			else if (ad == null)
			{
				Debug.LogError("Unexpected error: Interstitial load event fired with null ad and null error.");
				errorByCloseInterstitialAd = INTERSTITIAL_ERROR.LOAD_UNKNOWN_FAILED;
				interstitialState = INTERSTITIAL_STATE.CLOSED;
			}
			else
			{
				interstitialAd = ad;
				if (!interstitialAd.CanShowAd())
				{
					Debug.LogError("Interstitial Ad Loaded & Cant Show Ad");
					errorByCloseInterstitialAd = INTERSTITIAL_ERROR.CANT_SHOW_AD;
					interstitialState = INTERSTITIAL_STATE.CLOSED;
				}
				else
				{
					ListenToInterstitialAdEvents();
					interstitialState = INTERSTITIAL_STATE.WAIT_SHOW;
				}
			}
		});
	}

	public void ShowInterstitialAd()
	{
		if (interstitialState == INTERSTITIAL_STATE.LOADING)
		{
			isCalledShowInterstitial = true;
		}
		else if (interstitialState == INTERSTITIAL_STATE.WAIT_SHOW)
		{
			interstitialAd.Show();
			interstitialState = INTERSTITIAL_STATE.WAIT_CLOSE;
		}
		else if (interstitialState != INTERSTITIAL_STATE.WAIT_CLOSE)
		{
			CreateInterstitialAd();
			isCalledShowInterstitial = true;
		}
	}

	public void DestroyInterstitialAd()
	{
		if (interstitialAd != null)
		{
			interstitialAd.Destroy();
			interstitialAd = null;
		}
		isCalledShowInterstitial = false;
		errorByCloseInterstitialAd = INTERSTITIAL_ERROR.NONE;
		interstitialState = INTERSTITIAL_STATE.IDLE;
	}

	private void ListenToInterstitialAdEvents()
	{
		interstitialAd.OnAdPaid += delegate
		{
		};
		interstitialAd.OnAdImpressionRecorded += delegate
		{
		};
		interstitialAd.OnAdClicked += delegate
		{
		};
		interstitialAd.OnAdFullScreenContentOpened += delegate
		{
		};
		interstitialAd.OnAdFullScreenContentClosed += delegate
		{
			interstitialState = INTERSTITIAL_STATE.CLOSED;
		};
		interstitialAd.OnAdFullScreenContentFailed += delegate(AdError error)
		{
			Debug.LogError("***Interstitial***: OnAdFullScreenContentFailed: " + error);
			errorByCloseInterstitialAd = INTERSTITIAL_ERROR.SHOW_FAILED;
			interstitialState = INTERSTITIAL_STATE.CLOSED;
		};
	}

	public bool IsClosedRewardAd()
	{
		return rewardState == REWARD_STATE.CLOSED;
	}

	public bool IsSucceededReward()
	{
		return isSuccessReward;
	}

	public REWARD_ERROR GetErrorByCloseRewardAd()
	{
		return errorByCloseRewardAd;
	}

	public void RewardUpdate()
	{
		switch (rewardState)
		{
		case REWARD_STATE.WAIT_SHOW:
			if (isCalledShowReward)
			{
				isCalledShowReward = false;
				ShowRewardedAd();
			}
			break;
		}
		_ = pre_rewardState;
		_ = rewardState;
		pre_rewardState = rewardState;
	}

	public bool IsLoadedRewardAd()
	{
		return rewardState == REWARD_STATE.WAIT_SHOW;
	}

	public void CreateRewardedAd()
	{
		if (!IsCanRequestAd())
		{
			Debug.LogWarning("*****: CreateRewardedAd: Not initialize or Cant request ads");
			return;
		}
		if (rewardedAd != null)
		{
			DestroyRewardAd();
		}
		if (rewardedAd != null)
		{
			return;
		}
		rewardState = REWARD_STATE.LOADING;
		AdRequest request = new AdRequest();
		string text = null;
		text = ((!ADMOB_DEBUG) ? adUnitId_R : "ca-app-pub-3940256099942544/5224354917");
		RewardedAd.Load(text, request, async delegate(RewardedAd ad, LoadAdError error)
		{
			await UniTask.SwitchToMainThread();
			if (error != null)
			{
				Debug.LogError("*****: Rewarded ad failed to load an ad with error : " + error);
				errorByCloseRewardAd = REWARD_ERROR.LOAD_FAILED;
				rewardState = REWARD_STATE.CLOSED;
			}
			else if (ad == null)
			{
				Debug.LogError("*****: Unexpected error: Rewarded load event fired with null ad and null error.");
				errorByCloseRewardAd = REWARD_ERROR.LOAD_UNKNOWN_FAILED;
				rewardState = REWARD_STATE.CLOSED;
			}
			else
			{
				rewardedAd = ad;
				if (!rewardedAd.CanShowAd())
				{
					Debug.LogError("*****: Cant Show Reward Ad");
					errorByCloseRewardAd = REWARD_ERROR.CANT_SHOW_AD;
					rewardState = REWARD_STATE.CLOSED;
				}
				else
				{
					ListenToRewardAdEvents();
					rewardState = REWARD_STATE.WAIT_SHOW;
				}
			}
		});
	}

	public void ShowRewardedAd()
	{
		if (rewardState == REWARD_STATE.LOADING)
		{
			isCalledShowReward = true;
		}
		else if (rewardState == REWARD_STATE.WAIT_SHOW)
		{
			rewardState = REWARD_STATE.WAIT_CLOSE;
			rewardedAd.Show(delegate
			{
				isSuccessReward = true;
			});
		}
		else if (rewardState != REWARD_STATE.WAIT_CLOSE)
		{
			CreateRewardedAd();
			isCalledShowReward = true;
		}
	}

	public void DestroyRewardAd()
	{
		if (rewardedAd != null)
		{
			rewardedAd.Destroy();
			rewardedAd = null;
		}
		isCalledShowReward = false;
		isSuccessReward = false;
		errorByCloseRewardAd = REWARD_ERROR.NONE;
		rewardState = REWARD_STATE.IDLE;
	}

	private void ListenToRewardAdEvents()
	{
		rewardedAd.OnAdPaid += delegate
		{
		};
		rewardedAd.OnAdImpressionRecorded += delegate
		{
		};
		rewardedAd.OnAdClicked += delegate
		{
		};
		rewardedAd.OnAdFullScreenContentOpened += delegate
		{
		};
		rewardedAd.OnAdFullScreenContentClosed += delegate
		{
			rewardState = REWARD_STATE.CLOSED;
		};
		rewardedAd.OnAdFullScreenContentFailed += delegate(AdError error)
		{
			Debug.LogError("***Reward***: OnAdFullScreenContentFailed: " + error);
			errorByCloseRewardAd = REWARD_ERROR.SHOW_FAILED;
			rewardState = REWARD_STATE.CLOSED;
		};
	}

	public void SetVolume(float volume)
	{
		if (isInitialize)
		{
			MobileAds.SetApplicationVolume(volume);
			DestroyInterstitialAd();
			DestroyRewardAd();
		}
	}

	public void ConsentInfoReset()
	{
		if (ADMOB_DEBUG)
		{
			ConsentInformation.Reset();
		}
	}
}
