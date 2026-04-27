using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using Bulbul.Web;
using Cysharp.Threading.Tasks;
using FastEnumUtility;
using NestopiSystem;
using R3;
using UnityEngine.Localization.SmartFormat;
using UnityEngine.SceneManagement;
using VContainer;

namespace Bulbul;

public class AppAuth
{
	[Inject]
	private LanguageSupplier languageSupplier;

	[Inject]
	private GoogleAuthLogic googleAuthLogic = new GoogleAuthLogic();

	[Inject]
	private AppleAuthLogic appleAuthLogic = new AppleAuthLogic();

	[Inject]
	private SaveDataDirtyManager saveDataDirtyManager;

	[Inject]
	private SaveDataIO saveDataIO;

	[Inject]
	private LoadingScreen loadingScreen;

	[Inject]
	private WebApiGate webApiGate;

	[Inject]
	private SaveDataSync saveDataSync;

	private readonly Subject<(AccountType type, string token)> onAuth = new Subject<(AccountType, string)>();

	public Observable<(AccountType type, string token)> OnAuth => onAuth;

	private async UniTask<string> GoogleAccountVerify(CancellationToken ct)
	{
		try
		{
			string text = await googleAuthLogic.AuthAsync(ct);
			if (!text.IsNullOrEmpty())
			{
				onAuth.OnNext((AccountType.Google, text));
			}
			return text;
		}
		catch (Exception ex)
		{
			if (!(ex is OperationCanceledException))
			{
				Debug.LogError(ex);
			}
			return "";
		}
	}

	private async UniTask<string> AppleAccountVerify(CancellationToken ct)
	{
		try
		{
			string text = await appleAuthLogic.AuthAsync(ct);
			if (!text.IsNullOrEmpty())
			{
				onAuth.OnNext((AccountType.Apple, text));
			}
			return text;
		}
		catch (Exception ex)
		{
			if (!(ex is OperationCanceledException))
			{
				Debug.LogError(ex);
			}
			return "";
		}
	}

	public void GuestLogin()
	{
		onAuth.OnNext((AccountType.None, ""));
	}

	public async UniTask<string> Signin(AccountType type, CancellationToken ct, IUICanvasProvider canvasProvider)
	{
		IUniTaskAsyncDisposable uniTaskAsyncDisposable = null;
		if (DevicePlatform.Steam.IsPC())
		{
			string bodyId = (SaveDataManager.Instance.SettingData.IsAlwaysOnTop ? "ui_account_auth_wait_body_always_on_top" : "ui_account_auth_wait_body");
			CommonDialog commonDialog = CommonDialog.Create(delegate(CommonDialogOption o)
			{
				o.TitleID = "ui_account_auth_wait_title";
				o.BodyID = bodyId;
				o.Parent = canvasProvider.CommonDialogParent;
				o.Buttons = new CommonButton[1]
				{
					new CommonButton("ui_common_confirm_cancel", CommonButtonStyle.Normal, SystemSeType.Cancel)
				};
				o.EnableCloseOnClickButton = true;
			});
			CancellationTokenSource userCts = new CancellationTokenSource();
			commonDialog.SubmitOrCloseWaitAsync(ct).ContinueWith(delegate
			{
				userCts.CancelDispose();
			}).Forget();
			ct = CancellationTokenSource.CreateLinkedTokenSource(ct, userCts.Token).Token;
			uniTaskAsyncDisposable = commonDialog;
		}
		IUniTaskAsyncDisposable _ = uniTaskAsyncDisposable;
		object obj = null;
		int num = 0;
		string result = default(string);
		try
		{
			result = type switch
			{
				AccountType.None => "", 
				AccountType.Google => await GoogleAccountVerify(ct), 
				AccountType.Apple => await AppleAccountVerify(ct), 
				_ => throw new ArgumentOutOfRangeException("type", type, null), 
			};
			num = 1;
		}
		catch (object obj2)
		{
			obj = obj2;
		}
		if (_ != null)
		{
			await _.DisposeAsync();
		}
		object obj3 = obj;
		if (obj3 != null)
		{
			ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
		}
		if (num == 1)
		{
			return result;
		}
		string result2 = default(string);
		return result2;
	}

	public async UniTask<WebApiError> AccountLink(AccountType accountType, IUICanvasProvider canvasProvider, AccountTransferDialog transferDialog, CancellationToken ct)
	{
		if (webApiGate.AnyClosed())
		{
			return default(WebApiError);
		}
		List<AccountType> linkedTypes = InMemoryData.GetOrSet(() => new List<AccountType>());
		string deviceID = SaveDataManager.Instance.AccountData.DeviceID;
		WebApiError result = default(WebApiError);
		if (deviceID.IsNullOrEmpty())
		{
			using TermsDialog dialog = TermsDialog.Create(canvasProvider.UIParent.transform, isClosable: true);
			if (!(await dialog.SubmitWaitAsync(ct)))
			{
				result = default(WebApiError);
				return result;
			}
			WebApiResponse<SignupResponse> webApiResponse = await WebApi.GetAsync<Signup, SignupResponse>(ct);
			deviceID = webApiResponse.Response.DeviceID;
			if (webApiResponse.HasErrorOrReset)
			{
				result = webApiResponse.Error;
				return result;
			}
			WebApiResponse<StartupCheckResponse> webApiResponse2 = await WebApi.GetAsync<StartupCheck, StartupCheckResponse>(new StartupCheck(deviceID, languageSupplier.Get()), ct);
			if (webApiResponse2.HasErrorOrReset)
			{
				result = webApiResponse2.Error;
				return result;
			}
			WebApiResponse<ErrorCodeResponse> webApiResponse3 = await WebApi.GetAsync<UpdateTermsVer, ErrorCodeResponse>(new UpdateTermsVer(deviceID), ct);
			if (webApiResponse3.HasErrorOrReset)
			{
				result = webApiResponse3.Error;
				return result;
			}
		}
		string text = await Signin(accountType, ct, canvasProvider);
		if (text.IsNullOrEmpty())
		{
			return default(WebApiError);
		}
		if (webApiGate.AnyClosed())
		{
			return default(WebApiError);
		}
		WebApiResponse<CheckAccountResponse> account = await WebApi.PostAsync<CheckAccount, CheckAccountResponse>(new CheckAccount(deviceID, text, accountType), ct);
		if (account.HasErrorOrReset)
		{
			return account.Error;
		}
		bool isTransfer = false;
		WebApiError result2 = default(WebApiError);
		object obj;
		int num;
		object obj4;
		if (account.Response.OtherUserStatus != null)
		{
			UserStatus currentUserStatus = new UserStatus(SaveDataManager.Instance.PlayerData, account.Response.SenderUserStatus?.LastSaveDate);
			UserStatus otherUserStatus = account.Response.OtherUserStatus;
			AccountTransferDialog _ = await transferDialog.Open(accountType, currentUserStatus, otherUserStatus, ct);
			obj = null;
			num = 0;
			try
			{
				WebApiError webApiError = default(WebApiError);
				for (; !ct.IsCancellationRequested; webApiError = default(WebApiError))
				{
					int num2 = await transferDialog.SubmitOrCloseWaitAsync(ct);
					if (num2 < 0)
					{
						result2 = default(WebApiError);
					}
					else
					{
						bool transfer = num2 == 1;
						string body = ((num2 == 0) ? "ui_auth_other_account_transfer" : "ui_auth_other_account_notTransfer");
						CommonDialog dialog2 = CommonDialog.Create(delegate(CommonDialogOption o)
						{
							o.TitleID = "ui_common_confirm";
							o.BodyID = body;
							o.BodySelector = (string s) => s.FormatSmart(accountType.ToName());
							o.Parent = canvasProvider.CommonDialogParent;
							o.Buttons = new CommonButton[2]
							{
								new CommonButton("ui_common_confirm_yes"),
								new CommonButton("ui_common_confirm_no", CommonButtonStyle.Normal, SystemSeType.Cancel)
							};
						});
						object obj2 = null;
						int num3 = 0;
						try
						{
							int num4 = await dialog2.SubmitOrCloseWaitAsync(ct);
							if (webApiGate.AnyClosed())
							{
								webApiError = default(WebApiError);
								num3 = 2;
							}
							else if (num4 == 0)
							{
								isTransfer = transfer;
								if (isTransfer)
								{
									webApiGate.AccountGate.Value = false;
								}
								num3 = 1;
							}
						}
						catch (object obj3)
						{
							obj2 = obj3;
						}
						if ((object)dialog2 != null)
						{
							await dialog2.DisposeAsync();
						}
						obj4 = obj2;
						if (obj4 != null)
						{
							ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
						}
						switch (num3)
						{
						case 2:
							result2 = webApiError;
							break;
						default:
							continue;
						case 1:
							goto end_IL_0867;
						}
					}
					num = 1;
					break;
					continue;
					end_IL_0867:
					break;
				}
			}
			catch (object obj3)
			{
				obj = obj3;
			}
			if ((object)_ != null)
			{
				await _.DisposeAsync();
			}
			obj4 = obj;
			if (obj4 != null)
			{
				ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
			}
			if (num == 1)
			{
				return result2;
			}
			result2 = default(WebApiError);
		}
		using (loadingScreen.CreateLoadingScope())
		{
			if (SaveDataManager.Instance.AccountData.DeviceID.IsNullOrEmpty() && !isTransfer)
			{
				WebApiError webApiError2 = await saveDataSync.AllUploadAsync(SaveDataFlash.AllReplace, deviceID, saveDataIO.GetDirtyTargets().ToArray(), checkLogin: false, ct);
				if (webApiError2.HasErrorOrReset)
				{
					webApiGate.AccountGate.Value = true;
					result = webApiError2;
					return result;
				}
			}
			WebApiResponse<ErrorCodeResponse> webApiResponse4 = await WebApi.GetAsync<LinkAccount, ErrorCodeResponse>(new LinkAccount(deviceID, account.Response.LinkSessionID, accountType, isTransfer), ct);
			if (webApiResponse4.HasErrorOrReset)
			{
				webApiGate.AccountGate.Value = true;
				result = webApiResponse4.Error;
				return result;
			}
		}
		if (!linkedTypes.Contains(accountType))
		{
			linkedTypes.Add(accountType);
		}
		if (SaveDataManager.Instance.AccountData.DeviceID.IsNullOrEmpty())
		{
			SaveDataManager.Instance.AccountData.DeviceID = deviceID;
			SaveDataManager.Instance.SaveAccountData();
		}
		saveDataDirtyManager.StartDirtyObserve();
		CommonDialog endDialog = CommonDialog.Create(delegate(CommonDialogOption o)
		{
			o.TitleID = "ui_common_success";
			o.BodyID = "ui_account_link_success";
			o.Parent = canvasProvider.CommonDialogParent;
			o.Buttons = new CommonButton[1]
			{
				new CommonButton("ui_common_confirm_ok")
			};
			o.EnableCloseOnClickButton = true;
		});
		obj = null;
		num = 0;
		try
		{
			await endDialog.SubmitOrCloseWaitAsync(ct);
			if (isTransfer)
			{
				SaveDataManager.Instance.DeleteSafeSaveKey();
				SceneManager.LoadScene("Entry");
				result2 = default(WebApiError);
			}
			else
			{
				result2 = default(WebApiError);
			}
			num = 1;
		}
		catch (object obj3)
		{
			obj = obj3;
		}
		if ((object)endDialog != null)
		{
			await endDialog.DisposeAsync();
		}
		obj4 = obj;
		if (obj4 != null)
		{
			ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
		}
		if (num == 1)
		{
			return result2;
		}
		return result;
	}

	public async UniTask<WebApiError> AccountUnlink(AccountType accountType, CancellationToken ct)
	{
		string deviceID = SaveDataManager.Instance.AccountData.DeviceID;
		if (deviceID.IsNullOrEmpty())
		{
			return default(WebApiError);
		}
		WebApiResponse<ErrorCodeResponse> webApiResponse = await WebApi.GetAsync<UnlinkAccount, ErrorCodeResponse>(new UnlinkAccount(deviceID, accountType), ct);
		if (webApiResponse.HasErrorOrReset)
		{
			return webApiResponse.Error;
		}
		if (InMemoryData.TryGetData<List<AccountType>>(out var data))
		{
			data.Remove(accountType);
		}
		return default(WebApiError);
	}

	public async UniTask<WebApiError> DeleteAccount(IUICanvasProvider uiCanvasProvider, CancellationToken ct)
	{
		if (webApiGate.AnyClosed())
		{
			return default(WebApiError);
		}
		CommonDialog dialog = CommonDialog.Create(delegate(CommonDialogOption o)
		{
			o.TitleID = "ui_account_delete_sub_title";
			o.BodyID = "ui_account_delete_info_1";
			o.Parent = uiCanvasProvider.CommonDialogParent;
			o.Buttons = new CommonButton[2]
			{
				new CommonButton("ui_account_delete_exec", CommonButtonStyle.Submit),
				new CommonButton("ui_account_delete_cancel", CommonButtonStyle.Normal, SystemSeType.Cancel)
			};
			o.EnableCloseOnClickButton = true;
		});
		object obj = null;
		int num = 0;
		WebApiError result = default(WebApiError);
		try
		{
			if (await dialog.SubmitOrCloseWaitAsync(ct) != 0)
			{
				result = default(WebApiError);
				num = 1;
			}
		}
		catch (object obj2)
		{
			obj = obj2;
		}
		if ((object)dialog != null)
		{
			await dialog.DisposeAsync();
		}
		object obj3 = obj;
		if (obj3 != null)
		{
			ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
		}
		if (num == 1)
		{
			return result;
		}
		result = default(WebApiError);
		dialog = CommonDialog.Create(delegate(CommonDialogOption o)
		{
			o.TitleID = "ui_account_delete_sub_title";
			o.BodyID = "ui_account_delete_info_2";
			o.Parent = uiCanvasProvider.CommonDialogParent;
			o.Buttons = new CommonButton[2]
			{
				new CommonButton("ui_account_delete_exec", CommonButtonStyle.Submit),
				new CommonButton("ui_account_delete_cancel", CommonButtonStyle.Normal, SystemSeType.Cancel)
			};
			o.EnableCloseOnClickButton = true;
		});
		obj = null;
		num = 0;
		try
		{
			if (await dialog.SubmitOrCloseWaitAsync(ct) != 0)
			{
				result = default(WebApiError);
				num = 1;
			}
		}
		catch (object obj2)
		{
			obj = obj2;
		}
		if ((object)dialog != null)
		{
			await dialog.DisposeAsync();
		}
		obj3 = obj;
		if (obj3 != null)
		{
			ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
		}
		if (num == 1)
		{
			return result;
		}
		if (webApiGate.AnyClosed())
		{
			return default(WebApiError);
		}
		ct.ThrowIfCancellationRequested();
		webApiGate.AccountGate.Value = false;
		using (loadingScreen.CreateLoadingScope())
		{
			if (!string.IsNullOrEmpty(SaveDataManager.Instance.AccountData?.DeviceID))
			{
				WebApiResponse<ErrorCodeResponse> webApiResponse = await WebApi.GetAsync<DeleteAccount, ErrorCodeResponse>(new DeleteAccount(SaveDataManager.Instance.AccountData.DeviceID), ct);
				if (webApiResponse.HasErrorOrReset)
				{
					webApiGate.AccountGate.Value = true;
					return webApiResponse.Error;
				}
			}
			SaveDataManager.Instance.DeleteAllSaveData(excludeLocalData: false);
		}
		dialog = CommonDialog.Create(delegate(CommonDialogOption o)
		{
			o.TitleID = "ui_account_delete_sub_title";
			o.BodyID = "ui_account_delete_info_3";
			o.Parent = uiCanvasProvider.CommonDialogParent;
			o.Buttons = new CommonButton[1]
			{
				new CommonButton("ui_common_confirm_ok")
			};
			o.EnableCloseOnClickButton = true;
		});
		obj = null;
		try
		{
			await dialog.SubmitOrCloseWaitAsync(ct);
		}
		catch (object obj2)
		{
			obj = obj2;
		}
		if ((object)dialog != null)
		{
			await dialog.DisposeAsync();
		}
		obj3 = obj;
		if (obj3 != null)
		{
			ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
		}
		SaveDataManager.Instance.DeleteAllSaveData(excludeLocalData: false);
		SceneManager.LoadScene("Entry");
		return default(WebApiError);
	}
}
