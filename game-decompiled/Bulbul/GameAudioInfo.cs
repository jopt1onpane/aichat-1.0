using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using NestopiSystem;
using TagLib;
using UnityEngine;
using UnityEngine.Networking;

namespace Bulbul;

public class GameAudioInfo : IDisposable
{
	public bool IsUnlocked;

	public bool IsManualUnlocked;

	public AudioMode PathType;

	public AudioClip AudioClip;

	public AudioTag Tag;

	public string Title = "";

	public string Credit = "";

	public string LocalPath = "";

	public string UUID;

	public string OverwriteTitleForMobile;

	public string OverwriteCreditForMobile;

	public string AudioClipName
	{
		get
		{
			if (!(AudioClip == null))
			{
				return AudioClip.name;
			}
			return "";
		}
	}

	public static GameAudioInfo CreateNormal(AudioClip clip, AudioTag tag, string title, string credit, string uuid, bool isManualUnlocked, string overwriteTitleForMobile, string overwriteCreditForMobile)
	{
		return new GameAudioInfo
		{
			PathType = AudioMode.Normal,
			AudioClip = clip,
			Tag = tag,
			Title = title,
			Credit = credit,
			LocalPath = "",
			UUID = uuid,
			IsManualUnlocked = isManualUnlocked,
			OverwriteTitleForMobile = overwriteTitleForMobile,
			OverwriteCreditForMobile = overwriteCreditForMobile
		};
	}

	public static GameAudioInfo CreateByMaster(MusicData data)
	{
		return CreateNormal(data.AudioClip, data.Tag, data.Title, data.Credit, data.UUID, data.IsManualUnlock, data.overwriteTitleForMobile, data.overwriteCreditForMobile);
	}

	public static async UniTask<GameAudioInfo> CreateLocalFileAsync(string filePath, string uuid, CancellationToken ct)
	{
		var (audioClip, text, credit) = await DownloadAudioFile(filePath, ct);
		if (audioClip == null)
		{
			return new GameAudioInfo
			{
				IsUnlocked = true,
				PathType = AudioMode.LocalPc,
				AudioClip = null,
				Tag = AudioTag.Local,
				Title = text,
				Credit = credit,
				LocalPath = filePath,
				UUID = uuid
			};
		}
		if (string.IsNullOrEmpty(text))
		{
			text = GetAudioClipName(filePath);
		}
		audioClip.name = text;
		return new GameAudioInfo
		{
			IsUnlocked = true,
			PathType = AudioMode.LocalPc,
			AudioClip = audioClip,
			Tag = AudioTag.Local,
			Title = text,
			Credit = credit,
			LocalPath = filePath,
			UUID = uuid
		};
	}

	public static GameAudioInfo CreateLocalFile(string localpath)
	{
		return new GameAudioInfo
		{
			IsUnlocked = true,
			PathType = AudioMode.LocalPc,
			AudioClip = null,
			Tag = AudioTag.Local,
			Title = Path.GetFileNameWithoutExtension(localpath),
			Credit = "",
			LocalPath = localpath,
			UUID = Guid.NewGuid().ToString()
		};
	}

	public async UniTask<AudioClip> GetAudioClip(CancellationToken ct)
	{
		if (AudioClip != null)
		{
			return AudioClip;
		}
		if (PathType == AudioMode.LocalPc)
		{
			AudioClip = await LoadLocalFile(LocalPath, ct);
			return AudioClip;
		}
		Debug.LogError($"AudioClip is not set. Title:{Title},Credit:{Credit},PathType:{PathType},LocalPath:{LocalPath}");
		return null;
	}

	public static async UniTask<List<GameAudioInfo>> LoadLocalFiles(bool isFolder, CancellationToken ct)
	{
		IEnumerable<string> enumerable = await FileBrowser.OpenMusicFiles(isFolder, ct);
		List<GameAudioInfo> result = new List<GameAudioInfo>();
		foreach (string audioPath in enumerable)
		{
			var (audioClip, text, credit) = await DownloadAudioFile(audioPath, ct);
			if (audioClip == null)
			{
				result.Add(new GameAudioInfo
				{
					AudioClip = null
				});
				continue;
			}
			if (string.IsNullOrEmpty(text))
			{
				text = GetAudioClipName(audioPath);
			}
			audioClip.name = text;
			result.Add(new GameAudioInfo
			{
				IsUnlocked = true,
				PathType = AudioMode.LocalPc,
				AudioClip = audioClip,
				Tag = AudioTag.Local,
				Title = text,
				Credit = credit,
				LocalPath = audioPath,
				UUID = Guid.NewGuid().ToString()
			});
		}
		return result;
	}

	private async UniTask<AudioClip> LoadLocalFile(string filepath, CancellationToken ct)
	{
		LocalPath = filepath;
		var (audioClip, text, credit) = await DownloadAudioFile(filepath, ct);
		if (audioClip == null)
		{
			Debug.LogError(filepath + " is not audio");
			return null;
		}
		Title = (string.IsNullOrEmpty(text) ? GetAudioClipName(filepath) : text);
		audioClip.name = Title;
		Credit = credit;
		return audioClip;
	}

	private static async UniTask<(AudioClip Clip, string Title, string Credit)> DownloadAudioFile(string uri, CancellationToken ct)
	{
		AudioClip audioClip = null;
		try
		{
			using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(uri, GetAudioType(uri));
			((DownloadHandlerAudioClip)request.downloadHandler).streamAudio = true;
			await request.SendWebRequest().ToUniTask(null, PlayerLoopTiming.Update, ct);
			UnityWebRequest.Result result = request.result;
			if (result == UnityWebRequest.Result.ConnectionError || result == UnityWebRequest.Result.ProtocolError)
			{
				Debug.LogError(request.error);
				return (null, "", "");
			}
			audioClip = DownloadHandlerAudioClip.GetContent(request);
			if (Mathf.Approximately(audioClip.length, 0f))
			{
				Debug.LogError("Failed to load audio clip at: " + uri);
				return (null, "", "");
			}
		}
		catch (Exception ex)
		{
			bool flag = true;
			if (ex is UnityWebRequestException ex2 && ex2.ResponseCode == 404)
			{
				flag = false;
			}
			if (flag)
			{
				Debug.LogException(ex);
			}
		}
		if (audioClip == null)
		{
			return (null, "", "");
		}
		_ = audioClip.name;
		string item = "";
		try
		{
			(string Title, string Credit) audioMetaData = GetAudioMetaData(uri);
			_ = audioMetaData.Title;
			item = audioMetaData.Credit;
		}
		catch (Exception)
		{
		}
		string name = audioClip.name;
		return (audioClip, name, item);
		static AudioType GetAudioType(string path)
		{
			return Path.GetExtension(path).ToLower() switch
			{
				".mp3" => AudioType.MPEG, 
				".wav" => AudioType.WAV, 
				".ogg" => AudioType.OGGVORBIS, 
				_ => AudioType.UNKNOWN, 
			};
		}
	}

	public async UniTask ReloadLocalAudio(CancellationToken ct)
	{
		if (PathType == AudioMode.LocalPc)
		{
			UnloadAudioClip();
			AudioClip = await LoadLocalFile(LocalPath, ct);
		}
	}

	private static (string Title, string Credit) GetAudioMetaData(string path)
	{
		TagLib.File file = TagLib.File.Create(path);
		return (Title: file.Tag.Title, Credit: file.Tag.FirstPerformer);
	}

	private static string GetAudioClipName(string path)
	{
		return Path.GetFileNameWithoutExtension(path);
	}

	public void Dispose()
	{
		UnloadAudioClip();
	}

	public void UnloadAudioClip()
	{
		if (AudioClip != null)
		{
			if (PathType == AudioMode.LocalPc)
			{
				UnityEngine.Object.Destroy(AudioClip);
			}
			else
			{
				AudioClip.UnloadAudioData();
			}
			AudioClip = null;
		}
	}

	public string GetPlatformAudioTitle()
	{
		if (!string.IsNullOrEmpty(OverwriteTitleForMobile))
		{
			return OverwriteTitleForMobile;
		}
		return Title;
	}

	public string GetPlatformCredit()
	{
		if (!string.IsNullOrEmpty(OverwriteCreditForMobile))
		{
			return OverwriteCreditForMobile;
		}
		return Credit;
	}
}
