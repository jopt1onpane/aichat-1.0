using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using NestopiSystem;

namespace Bulbul.Web;

public class SaveDataSync
{
	private SaveDataIO saveDataIO;

	public SaveDataSync(SaveDataIO saveDataIO)
	{
		this.saveDataIO = saveDataIO;
	}

	public async UniTask AllDownloadAsync(string deviceId, CancellationToken ct)
	{
		WebApiResponse<GetSaveDataListResponse> webApiResponse = await WebApi.GetAsync<GetSaveDataList, GetSaveDataListResponse>(new GetSaveDataList(deviceId), ct);
		webApiResponse.ThrowException();
		DirectoryInfo directory = new DirectoryInfo(new ES3Settings(BulbulConstant.SaveDirectoryPath).FullPath);
		SaveDataManager.Instance.DeleteAllSaveData(excludeLocalData: true);
		string[] list = webApiResponse.Response.List;
		if (list != null && list.Length > 0)
		{
			await UniTask.WhenAll(webApiResponse.Response.List.Select((string fileName) => DownloadFileAsync(deviceId, directory.FullName, fileName, ct)));
		}
	}

	private async UniTask DownloadFileAsync(string deviceId, string directoryPath, string fileName, CancellationToken ct)
	{
		(await WebApi.FileDownloadAsync(new GetSaveData(deviceId, fileName), directoryPath, ct)).ThrowException();
	}

	public async UniTask<WebApiResponse<ErrorCodeResponse>> AllUploadParallelAsync(string deviceID, FileInfo[] upload, string[] deleted, int retryCount, CancellationToken ct)
	{
		WebApiResponse<ErrorCodeResponse> result = default(WebApiResponse<ErrorCodeResponse>);
		UserStatus userStatus = new UserStatus(SaveDataManager.HasInstance ? SaveDataManager.Instance.PlayerData : null);
		for (int i = 0; i < retryCount; i++)
		{
			result = await WebApi.PostAsync<SetSaveData, ErrorCodeResponse>(new SetSaveData(deviceID, isCheckLogin: true, userStatus, upload, deleted), ct);
			if (result.ResetReason != ResetReason.None)
			{
				return result;
			}
			if (!result.HasError)
			{
				return result;
			}
		}
		result.LogException();
		return result;
	}

	public async UniTask<WebApiError> AllUploadAsync(SaveDataFlash flash, string deviceID, string[] filePath, bool checkLogin, CancellationToken ct, string sessionID = "")
	{
		bool sendFlash = false;
		string[] deletedBuffer = null;
		UserStatus userStatus = new UserStatus(SaveDataManager.HasInstance ? SaveDataManager.Instance.PlayerData : null);
		for (int i = 0; i < filePath.Length; i++)
		{
			FileInfo fileInfo = new FileInfo(filePath[i]);
			FileInfo fileInfo2 = (fileInfo.Exists ? fileInfo : null);
			string[] deletedNames = null;
			if (fileInfo2 == null)
			{
				if (deletedBuffer == null)
				{
					deletedBuffer = new string[1];
				}
				deletedBuffer[0] = fileInfo.Name;
				deletedNames = deletedBuffer;
			}
			SaveDataFlash saveDataFlash = ((i == filePath.Length - 1) ? flash : SaveDataFlash.None);
			WebApiResponse<SyncSaveDataResponse> webApiResponse = await WebApi.PostAsync<SyncSaveData, SyncSaveDataResponse>(new SyncSaveData(deviceID, sessionID, saveDataFlash, checkLogin, (saveDataFlash != SaveDataFlash.None) ? userStatus : null, fileInfo2, deletedNames), ct);
			if (webApiResponse.HasError)
			{
				return webApiResponse.Error;
			}
			if (!webApiResponse.Response.SessionID.IsNullOrEmpty())
			{
				sessionID = webApiResponse.Response.SessionID;
			}
			if (i == filePath.Length - 1)
			{
				sendFlash = true;
				break;
			}
		}
		if (!sendFlash && !sessionID.IsNullOrEmpty())
		{
			WebApiResponse<ErrorCodeResponse> webApiResponse2 = await WebApi.PostAsync<SetSaveFinish, ErrorCodeResponse>(new SetSaveFinish(deviceID, sessionID, SaveDataFlash.DiffOnly, isCheckLogin: true, userStatus), ct);
			if (webApiResponse2.HasError)
			{
				return webApiResponse2.Error;
			}
		}
		return default(WebApiError);
	}

	public async UniTask<WebApiError> UploadChunkAsync(SaveDataFlash flash, string deviceID, IEnumerable<string> filePath, int chunk, bool checkLogin, CancellationToken ct, string sessionID = "")
	{
		foreach (IEnumerable<string> item in filePath.Chunk(chunk))
		{
			string[] source = item.ToArray();
			FileInfo[] array = (from x in source.Where(File.Exists)
				select new FileInfo(x)).ToArray();
			string[] array2 = source.Where((string x) => !File.Exists(x)).ToArray();
			if (array.Length != 0 || array2.Length != 0)
			{
				WebApiResponse<SyncSaveDataResponse> webApiResponse = await WebApi.PostAsync<SyncSaveDatas, SyncSaveDataResponse>(new SyncSaveDatas(deviceID, sessionID, SaveDataFlash.None, checkLogin, null, array, array2), ct);
				webApiResponse.LogException();
				if (webApiResponse.HasErrorOrReset)
				{
					return webApiResponse.Error;
				}
				if (!webApiResponse.Response.SessionID.IsNullOrEmpty())
				{
					sessionID = webApiResponse.Response.SessionID;
				}
			}
		}
		if (flash != SaveDataFlash.None && !sessionID.IsNullOrEmpty())
		{
			UserStatus userStatus = new UserStatus(SaveDataManager.HasInstance ? SaveDataManager.Instance.PlayerData : null);
			WebApiResponse<ErrorCodeResponse> webApiResponse2 = await WebApi.PostAsync<SetSaveFinish, ErrorCodeResponse>(new SetSaveFinish(deviceID, sessionID, flash, isCheckLogin: true, userStatus), ct);
			webApiResponse2.LogException();
			if (webApiResponse2.HasErrorOrReset)
			{
				return webApiResponse2.Error;
			}
		}
		return default(WebApiError);
	}
}
