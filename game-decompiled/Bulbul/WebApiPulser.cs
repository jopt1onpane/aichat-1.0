using System;
using System.Threading;
using Bulbul.Mobile;
using Bulbul.Web;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.Networking;
using VContainer.Unity;

namespace Bulbul;

public class WebApiPulser : IStartable, IDisposable
{
	private readonly ReactiveProperty<bool> lastSaveDataUploadSuccess = new ReactiveProperty<bool>(value: true);

	private readonly CancellationTokenSource cancellation = new CancellationTokenSource();

	private readonly WebApiGate webApiGate;

	private readonly WebApiErrorBehavior webApiErrorBehavior;

	private readonly SaveDataDirtyManager saveDataDirtyManager;

	private readonly ScenarioReader scenarioReader;

	private readonly LanguageSupplier languageSupplier;

	public ReadOnlyReactiveProperty<bool> LastSaveDataUploadSuccess => lastSaveDataUploadSuccess;

	public WebApiPulser(WebApiGate webApiGate, WebApiErrorBehavior webApiErrorBehavior, SaveDataDirtyManager saveDataDirtyManager, ScenarioReader scenarioReader, LanguageSupplier languageSupplier)
	{
		this.webApiGate = webApiGate;
		this.webApiErrorBehavior = webApiErrorBehavior;
		this.saveDataDirtyManager = saveDataDirtyManager;
		this.scenarioReader = scenarioReader;
		this.languageSupplier = languageSupplier;
	}

	public void Start()
	{
		SaveDataDirtyCheckStart(cancellation.Token).Forget();
		ServerHealthCheckStart(cancellation.Token).Forget();
	}

	private async UniTask SaveDataDirtyCheckStart(CancellationToken token)
	{
		while (!cancellation.IsCancellationRequested && !webApiGate.AnyClosed())
		{
			await UniTask.WaitForSeconds(Mathf.Max(SaveDataManager.Instance.SettingData.SaveDataSyncInterval.CurrentValue.AsSeconds(), SaveDataSyncInterval.Sec30.AsSeconds()), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
			if (webApiGate.AnyClosed())
			{
				break;
			}
			if (string.IsNullOrEmpty(SaveDataManager.Instance.AccountData?.DeviceID) || (scenarioReader.IsPlayingScenario() && scenarioReader.IsPlayingLongStoryOrTutorial()))
			{
				continue;
			}
			try
			{
				WebApiError webApiError = await saveDataDirtyManager.FlashAsync(token);
				if (!webApiGate.AccountGate.CurrentValue)
				{
					break;
				}
				if (webApiError.WasLogout())
				{
					await webApiErrorBehavior.LogoutDialog(token);
					break;
				}
				bool flag = webApiError.ErrorCode.IsSaveDataKind();
				bool flag2 = webApiError.Exception is UnityWebRequestException ex && (ex.Result == UnityWebRequest.Result.ConnectionError || ex.ResponseCode == 500);
				if (webApiError.ResetReason != ResetReason.None || (webApiError.HasError && !flag && !flag2))
				{
					await webApiErrorBehavior.ToEntrySimple(webApiError, token);
					break;
				}
				lastSaveDataUploadSuccess.OnNext(!webApiError.HasErrorOrReset);
				if (webApiGate.AnyClosed())
				{
					break;
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	private async UniTask ServerHealthCheckStart(CancellationToken token)
	{
		while (!cancellation.IsCancellationRequested && !webApiGate.AnyClosed())
		{
			await UniTask.WaitForSeconds(10, ignoreTimeScale: false, PlayerLoopTiming.Update, token);
			if (webApiGate.AnyClosed())
			{
				break;
			}
			string text = SaveDataManager.Instance.AccountData?.DeviceID;
			if (string.IsNullOrEmpty(text) || (scenarioReader.IsPlayingScenario() && scenarioReader.IsPlayingLongStoryOrTutorial()))
			{
				continue;
			}
			WebApiResponse<CheckLoginResponse> self = await WebApi.GetAsync<CheckLogin, CheckLoginResponse>(new CheckLogin(text, SaveDataManager.Instance.PlayerData.ReadNewsID, languageSupplier.Get()), token);
			if (!webApiGate.AccountGate.CurrentValue)
			{
				break;
			}
			if (self.WasLogout() || (!self.HasErrorOrReset && !self.Response.IsLogin))
			{
				await webApiErrorBehavior.LogoutDialog(token);
				break;
			}
			if (self.HasErrorOrReset)
			{
				if (!(self.Exception is UnityWebRequestException ex) || (ex.Result != UnityWebRequest.Result.ConnectionError && ex.ResponseCode != 500))
				{
					await webApiErrorBehavior.ToEntrySimple(self.Error, token);
					break;
				}
				continue;
			}
			if (webApiGate.AnyClosed())
			{
				break;
			}
			InMemoryData.GetOrSet(() => new NewsState()).AvailableNewNews.Value = self.Response.IsNewsBadge;
			InMemoryData.GetOrSet(() => new ShopState()).MaintenanceInfo = self.Response.ShopMaintenanceInfo;
		}
	}

	public void Dispose()
	{
		cancellation?.Cancel();
		cancellation?.Dispose();
	}
}
