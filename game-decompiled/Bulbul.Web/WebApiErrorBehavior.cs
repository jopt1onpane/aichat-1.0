using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using VContainer;

namespace Bulbul.Web;

public class WebApiErrorBehavior
{
	[Inject]
	private IUICanvasProvider uiCanvasProvider;

	[Inject]
	private WebApiGate webApiGate;

	public async UniTask<bool> ErrorDialog(WebApiError error, CancellationToken ct)
	{
		if (error.WasLogout())
		{
			await LogoutDialog(ct);
			return true;
		}
		if (error.ResetReason != ResetReason.None)
		{
			await ToEntrySimple(error, ct);
			return true;
		}
		if (error.HasError)
		{
			if (error.Exception is UnityWebRequestException ex)
			{
				int errorCode = ((ex.ResponseCode != 0L) ? ((int)ex.ResponseCode) : ((int)ex.Result));
				CommonDialog dialog = CommonDialog.Create(delegate(CommonDialogOption o)
				{
					o.TitleID = "ui_common_error";
					o.BodyID = "ui_common_error_errorCode2";
					o.BodySelector = (string s) => string.Format(s, errorCode);
					o.Parent = uiCanvasProvider.CommonDialogParent;
					o.EnableCloseOnClickButton = true;
					o.Buttons = new CommonButton[1]
					{
						new CommonButton("ui_common_confirm_ok")
					};
				});
				object obj = null;
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
				object obj3 = obj;
				if (obj3 != null)
				{
					ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
				}
			}
			else
			{
				CommonDialog dialog = CommonDialog.Create(delegate(CommonDialogOption o)
				{
					o.TitleID = "ui_common_error";
					o.BodyID = "ui_common_error_errorCode";
					o.BodySelector = (string s) => string.Format(s, error.ErrorValue);
					o.EnableCloseOnClickButton = true;
					o.Buttons = new CommonButton[1]
					{
						new CommonButton("ui_common_confirm_ok")
					};
					o.Parent = uiCanvasProvider.CommonDialogParent;
				});
				object obj = null;
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
				object obj3 = obj;
				if (obj3 != null)
				{
					ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
				}
			}
		}
		return false;
	}

	public async UniTask ToEntrySimple(WebApiError error, CancellationToken ct)
	{
		CommonDialog dialog = CommonDialog.Create(delegate(CommonDialogOption o)
		{
			o.TitleID = GetErrorTitleID(error);
			o.BodyID = GetErrorBodyID(error);
			o.BodySelector = (string s) => string.Format(s, error.ErrorValue);
			o.EnableCloseOnClickButton = true;
			o.Buttons = new CommonButton[1]
			{
				new CommonButton("ui_common_confirm_ok")
			};
			o.Parent = uiCanvasProvider.CommonDialogParent;
		});
		object obj = null;
		int num = 0;
		try
		{
			await dialog.SubmitOrCloseWaitAsync(ct);
			SaveDataManager.Instance.DeleteSafeSaveKey();
			SceneManager.LoadScene("Entry");
			num = 1;
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
		if (num != 1)
		{
		}
	}

	public async UniTask LogoutDialog(CancellationToken ct)
	{
		webApiGate.LoginGate.Value = false;
		SaveDataManager.Instance.DeleteSafeSaveKey();
		CommonDialog dialog = CommonDialog.Create(delegate(CommonDialogOption o)
		{
			o.TitleID = "ui_logout_title";
			o.BodyID = "ui_login_failed_body";
			o.Parent = uiCanvasProvider.CommonDialogParent;
		});
		object obj = null;
		int num = 0;
		try
		{
			await UniTask.Never(ct);
			num = 1;
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
		if (num != 1)
		{
		}
	}

	private string GetErrorTitleID(WebApiError error)
	{
		return error.ResetReason switch
		{
			ResetReason.Maintenance => "ui_maintenance_title2", 
			ResetReason.AppForceUpdate => "ui_app_update_title", 
			ResetReason.TermsUpdate => "ui_terms_text", 
			ResetReason.OtherDeviceLogin => "ui_logout_title", 
			_ => "ui_common_error", 
		};
	}

	private string GetErrorBodyID(WebApiError error)
	{
		return error.ResetReason switch
		{
			ResetReason.Maintenance => "ui_maintenance_body", 
			ResetReason.AppForceUpdate => "ui_app_update_body", 
			ResetReason.TermsUpdate => "ui_terms_body2", 
			ResetReason.OtherDeviceLogin => "ui_login_failed_body", 
			_ => "ui_common_error_errorCode2", 
		};
	}
}
