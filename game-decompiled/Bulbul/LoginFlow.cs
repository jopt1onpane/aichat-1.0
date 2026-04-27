using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using Bulbul.Mobile;
using Bulbul.Web;
using Cysharp.Threading.Tasks;
using NestopiSystem;
using R3;
using UnityEngine.CrashReportHandler;
using UnityEngine.SceneManagement;
using VContainer;

namespace Bulbul;

public class LoginFlow
{
	[Inject]
	private AppAuth appAuth;

	[Inject]
	private LanguageSupplier languageSupplier;

	[Inject]
	private SaveDataIO saveDataIO;

	[Inject]
	private SaveDataMigrator saveDataMigrator;

	[Inject]
	private SaveDataDirtyManager saveDataDirtyManager;

	[Inject]
	private IUICanvasProvider uiCanvasProvider;

	[Inject]
	private LoadingScreen loadingScreen;

	[Inject]
	private AuthUI authUI;

	[Inject]
	private SaveDataSync saveDataSync;

	public async UniTask LoginAsync(CancellationToken cancellation)
	{
		SaveDataManager.Instance.LoadAccountData();
		string deviceID = SaveDataManager.Instance.AccountData.DeviceID;
		bool isFirstLogin = false;
		string accountLinkToken = "";
		AccountType accountLinkType = AccountType.None;
		string accountLinkSessionID = "";
		bool isAccountTransferred = false;
		if (!ShouldLoginSteamVersion(deviceID))
		{
			return;
		}
		using (Disposable.Create(delegate
		{
			authUI.gameObject.SetActive(value: false);
		}))
		{
			if (string.IsNullOrEmpty(deviceID))
			{
				isFirstLogin = true;
				await TermsUpdateAsync(deviceID, sendWebApi: false, cancellation);
				(accountLinkType, accountLinkToken) = await AuthFlowAsync(cancellation);
				if (accountLinkType == AccountType.None)
				{
					SaveDataManager.Instance.AccountData.DeviceID = "";
					return;
				}
				WebApiResponse<SignupResponse> webApiResponse = await WebApi.GetAsync<Signup, SignupResponse>(cancellation);
				webApiResponse.ThrowException();
				AccountData accountData = SaveDataManager.Instance.AccountData;
				string deviceID2 = webApiResponse.Response.DeviceID;
				accountData.DeviceID = deviceID2;
				deviceID = webApiResponse.Response.DeviceID;
			}
			if (!string.IsNullOrEmpty(deviceID))
			{
				CrashReportHandler.SetUserMetadata("DeviceID", deviceID);
			}
			WebApiResponse<StartupCheckResponse> webApiResponse2 = await WebApi.GetAsync<StartupCheck, StartupCheckResponse>(new StartupCheck(deviceID, languageSupplier.Get()), cancellation);
			if (webApiResponse2.Response.IsMaintenance)
			{
				throw new MaintenanceException(webApiResponse2.Response.MainText);
			}
			if (webApiResponse2.Response.IsDeleteUser)
			{
				SaveDataManager.Instance.DeleteAllSaveData(excludeLocalData: false);
				SceneManager.LoadScene("Entry");
				throw new EntryBreakException();
			}
			webApiResponse2.Response.ThrowResetException(ResetReason.TermsUpdate);
			if (webApiResponse2.Response.IsConsentRequired)
			{
				await TermsUpdateAsync(deviceID, sendWebApi: true, cancellation);
			}
			using LoadingScreen.LoadingScope loadScope = (isFirstLogin ? loadingScreen.CreateLoadingScope() : loadingScreen.CreateEmptyScope());
			if (isFirstLogin && !accountLinkToken.IsNullOrEmpty() && accountLinkType != AccountType.None)
			{
				WebApiResponse<CheckAccountResponse> webApiResponse3 = await WebApi.PostAsync<CheckAccount, CheckAccountResponse>(new CheckAccount(deviceID, accountLinkToken, accountLinkType), cancellation);
				webApiResponse3.ThrowException();
				isAccountTransferred = webApiResponse3.Response.Status == CheckAccountStatus.LinkedOldUser;
				accountLinkSessionID = webApiResponse3.Response.LinkSessionID;
			}
			if (isFirstLogin)
			{
				if (!isAccountTransferred)
				{
					await SaveDataManager.Instance.Load();
					SaveDataManager.Instance.SaveAll();
					(await saveDataSync.AllUploadAsync(SaveDataFlash.AllReplace, deviceID, saveDataIO.GetDirtyTargets().ToArray(), checkLogin: false, cancellation)).ThrowException();
				}
				if (!accountLinkSessionID.IsNullOrEmpty())
				{
					(await WebApi.GetAsync<LinkAccount, ErrorCodeResponse>(new LinkAccount(deviceID, accountLinkSessionID, accountLinkType, isAccountTransferred), cancellation)).ThrowException();
				}
				SaveDataManager.Instance.SaveAccountData();
			}
			PlayerDataV3 result;
			ulong readNewsID = (saveDataIO.TryLoad<PlayerDataV3>(out result) ? result.ReadNewsID : 0);
			WebApiResponse<CheckLoginResponse> webApiResponse4 = await WebApi.GetAsync<CheckLogin, CheckLoginResponse>(new CheckLogin(deviceID, readNewsID, languageSupplier.Get()), cancellation);
			webApiResponse4.ThrowException();
			InMemoryData.GetOrSet(() => new NewsState()).AvailableNewNews.Value = webApiResponse4.Response.IsNewsBadge;
			if (isFirstLogin)
			{
				if (isAccountTransferred)
				{
					await saveDataSync.AllDownloadAsync(deviceID, cancellation);
					SaveDataManager.Instance.SaveFilesSnapshotData(FilesSnapshot.CreateGameDataDefault());
				}
			}
			else if (webApiResponse4.Response.IsLogin)
			{
				SlideFadeAnnounceDirection.AsyncPlayScope migrateScope = await saveDataMigrator.Migrate();
				object obj = null;
				try
				{
					(await saveDataDirtyManager.TryRestoreSnapshotFileAsync(cancellation)).ThrowException();
				}
				catch (object obj2)
				{
					obj = obj2;
				}
				await ((IAsyncDisposable)migrateScope/*cast due to .constrained prefix*/).DisposeAsync();
				object obj3 = obj;
				if (obj3 != null)
				{
					ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
				}
			}
			else
			{
				await saveDataSync.AllDownloadAsync(deviceID, cancellation);
				SaveDataManager.Instance.SaveFilesSnapshotData(FilesSnapshot.CreateGameDataDefault());
			}
			WebApiResponse<LoginResponse> webApiResponse5 = await WebApi.GetAsync<Login, LoginResponse>(new Login(deviceID, languageSupplier.Get()), cancellation);
			webApiResponse5.ThrowException();
			InMemoryData.SetData(webApiResponse5.Response.UnlockProducts);
			InMemoryData.SetData(webApiResponse5.Response.LinkedAccounts);
			InMemoryData.SetData(new ReviewState
			{
				IsAlreadyReviewed = webApiResponse5.Response.IsReview
			});
			InMemoryData.SetData(new ShopState
			{
				MaintenanceInfo = webApiResponse5.Response.ShopMaintenanceInfo
			});
			if (isFirstLogin && accountLinkType != AccountType.None)
			{
				loadScope.Dispose();
				await CommonDialog.Create(delegate(CommonDialogOption o)
				{
					o.TitleID = "ui_common_success";
					o.BodyID = "ui_account_link_success";
					o.EnableCloseOnClickButton = true;
					o.Buttons = new CommonButton[1]
					{
						new CommonButton("ui_common_confirm_ok")
					};
					o.Parent = uiCanvasProvider.CommonDialogParent;
				}).SubmitOrCloseWaitAsync(cancellation);
				await UniTask.WaitForSeconds(1, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellation);
			}
		}
	}

	private async UniTask<(AccountType type, string token)> AuthFlowAsync(CancellationToken ct)
	{
		authUI.Interactable = true;
		authUI.gameObject.SetActive(value: true);
		(AccountType, string) result = await appAuth.OnAuth.ToUniTask(useFirstValue: true, ct);
		authUI.Interactable = false;
		return result;
	}

	private async UniTask TermsUpdateAsync(string deviceID, bool sendWebApi, CancellationToken ct)
	{
		using (TermsDialog dialog = TermsDialog.Create(uiCanvasProvider.CommonDialogParent, isClosable: false))
		{
			await dialog.SubmitWaitAsync(ct);
		}
		if (sendWebApi)
		{
			(await WebApi.GetAsync<UpdateTermsVer, ErrorCodeResponse>(new UpdateTermsVer(deviceID), ct)).ThrowException();
		}
	}

	private bool ShouldLoginSteamVersion(string deviceID)
	{
		if (!deviceID.IsNullOrEmpty())
		{
			return true;
		}
		if (!saveDataIO.TryLoad<PlayerDataV3>(out var result))
		{
			if (!saveDataIO.TryLoad<PlayerData>(out var result2))
			{
				return true;
			}
			return result2.IsNeedTutorial;
		}
		return result.IsNeedTutorial;
	}
}
