using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Bulbul.Web;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine.Device;
using VContainer;

namespace Bulbul;

public class SaveDataDirtyManager
{
	[Inject]
	private SaveDataIO saveDataIO;

	[Inject]
	private WebApiGate webApiGate;

	[Inject]
	private SaveDataSync saveDataSync;

	private readonly float flushInterval = 20f;

	private DateTime lastFlushTime = DateTime.MaxValue;

	private bool isObserving;

	private readonly Subject<WebApiError> onEndFlash = new Subject<WebApiError>();

	public bool IsUploading { get; private set; }

	public Observable<WebApiError> OnEndFlash => onEndFlash;

	public void StartDirtyObserve()
	{
		if (!isObserving)
		{
			lastFlushTime = DateTime.Now;
			isObserving = true;
			if (!SaveDataManager.Instance.TryLoadFilesSnapshotData(out var result) || result.files.Count == 0)
			{
				SaveDataManager.Instance.SaveFilesSnapshotData(FilesSnapshot.CreateGameDataDefault());
			}
		}
	}

	public async UniTask<WebApiError> TryRestoreSnapshotFileAsync(CancellationToken ct)
	{
		if (SaveDataManager.Instance.TryLoadFilesSnapshotData(out var result) && result.files.Count != 0)
		{
			return await FlashAsync(ct);
		}
		return default(WebApiError);
	}

	public bool IsDirty()
	{
		FilesSnapshot result = SaveDataManager.Instance.FilesSnapshot;
		if ((result == null || result.files.Count == 0) && !SaveDataManager.Instance.TryLoadFilesSnapshotData(out result))
		{
			return false;
		}
		if (result.files.Count == 0)
		{
			return false;
		}
		FilesSnapshot newSnapshot = FilesSnapshot.CreateGameDataDefault();
		FileSnapShotDiff fileSnapShotDiff = FileSnapShotDiff.Create(result, newSnapshot, FilesSnapshot.DefaultTargetDirectory);
		if (fileSnapShotDiff.AddedOrModified.Length == 0)
		{
			return fileSnapShotDiff.DeletedFileNames.Length != 0;
		}
		return true;
	}

	public async UniTask<WebApiError> FlashAsync(CancellationToken ct)
	{
		if (!isObserving || (!IsUploading && string.IsNullOrEmpty(SaveDataManager.Instance.AccountData?.DeviceID)) || webApiGate.AnyClosed())
		{
			return EndFlashAction(default(WebApiError));
		}
		lastFlushTime = DateTime.Now;
		FilesSnapshot nextSnapshot = FilesSnapshot.CreateGameDataDefault();
		FileSnapShotDiff fileSnapShotDiff = FileSnapShotDiff.Create(SaveDataManager.Instance.FilesSnapshot, nextSnapshot, FilesSnapshot.DefaultTargetDirectory);
		if (fileSnapShotDiff.AddedOrModified.Length == 0 && fileSnapShotDiff.DeletedFileNames.Length == 0)
		{
			return EndFlashAction(default(WebApiError));
		}
		long num = fileSnapShotDiff.AddedOrModifiedSize / 1024 / 1024;
		Debug.LogDeveloperCheck($"<color=orange>[SaveDataDirty] SnapShotDiff. Size:{fileSnapShotDiff.AddedOrModifiedSize:N0}B AorM:{string.Join(',', fileSnapShotDiff.AddedOrModified.Select((FileInfo x) => x.Name))} Del:{string.Join(',', fileSnapShotDiff.DeletedFileNames)}</color>");
		IsUploading = true;
		using (Disposable.Create(this, delegate(SaveDataDirtyManager @this)
		{
			@this.IsUploading = false;
		}))
		{
			WebApiError error;
			if (num < 8)
			{
				error = await UploadAll(fileSnapShotDiff.AddedOrModified, fileSnapShotDiff.DeletedFileNames, ct);
			}
			else
			{
				string[] anyChanged = fileSnapShotDiff.AddedOrModified.Select((FileInfo x) => x.FullName).Concat(fileSnapShotDiff.DeletedFileNames.Select((string x) => Path.Combine(FilesSnapshot.DefaultTargetDirectory, x))).Distinct()
					.ToArray();
				error = await UploadSeparate(anyChanged, ct);
			}
			if (!error.HasErrorOrReset && !webApiGate.AnyClosed() && Application.isPlaying)
			{
				SaveDataManager.Instance.SaveFilesSnapshotData(nextSnapshot);
			}
			return EndFlashAction(error);
		}
	}

	public async UniTask<WebApiError> ScheduledFlashAsync(CancellationToken ct)
	{
		if (string.IsNullOrEmpty(SaveDataManager.Instance.AccountData?.DeviceID))
		{
			return default(WebApiError);
		}
		if (DateTime.Now - lastFlushTime < TimeSpan.FromSeconds(flushInterval))
		{
			return default(WebApiError);
		}
		return await FlashAsync(ct);
	}

	private async UniTask<WebApiError> UploadAll(FileInfo[] addedOrUpdated, string[] deleted, CancellationToken ct)
	{
		string deviceID = SaveDataManager.Instance.AccountData.DeviceID;
		if (addedOrUpdated.Length <= 15)
		{
			Debug.LogDeveloperCheck($"<color=orange>[SaveDataDirty] SaveDataSync.AllUploadParallelAsync. addedOrUpdated.Length:{addedOrUpdated.Length}</color>");
			return (await saveDataSync.AllUploadParallelAsync(deviceID, addedOrUpdated, deleted, 3, ct)).Error;
		}
		string sessionID = "";
		if (deleted.Length != 0)
		{
			Debug.LogDeveloperCheck($"<color=orange>[SaveDataDirty] deleted.Length:{deleted.Length}</color>");
			WebApiResponse<SyncSaveDataResponse> webApiResponse = await WebApi.PostAsync<SyncSaveData, SyncSaveDataResponse>(new SyncSaveData(deviceID, "", SaveDataFlash.None, isCheckLogin: true, null, null, deleted), ct);
			webApiResponse.LogException();
			if (webApiResponse.HasErrorOrReset)
			{
				return webApiResponse.Error;
			}
			sessionID = webApiResponse.Response.SessionID;
		}
		Debug.LogDeveloperCheck($"<color=orange>[SaveDataDirty] SaveDataSync.UploadChunkAsync. addedOrUpdated.Length:{addedOrUpdated.Length}</color>");
		IEnumerable<string> filePath = addedOrUpdated.Select((FileInfo x) => x.FullName);
		return await saveDataSync.UploadChunkAsync(SaveDataFlash.DiffOnly, deviceID, filePath, 15, checkLogin: true, ct, sessionID);
	}

	private async UniTask<WebApiError> UploadSeparate(string[] anyChanged, CancellationToken ct)
	{
		Debug.LogDeveloperCheck("<color=orange>[SaveDataDirty] UploadSeparate</color>");
		return await saveDataSync.AllUploadAsync(SaveDataFlash.DiffOnly, SaveDataManager.Instance.AccountData.DeviceID, anyChanged, checkLogin: true, ct);
	}

	public void DirtyClear()
	{
		Debug.LogDeveloperCheck("<color=orange>[SaveDataDirty] DirtyClear</color>");
		SaveDataManager.Instance.SaveFilesSnapshotData(new FilesSnapshot(new List<FileMeta>(), 0L));
	}

	private WebApiError EndFlashAction(WebApiError error)
	{
		onEndFlash.OnNext(error);
		return error;
	}
}
