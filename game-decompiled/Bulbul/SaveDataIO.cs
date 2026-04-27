using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Bulbul.Web;
using Cysharp.Text;
using NestopiSystem;
using Unio;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class SaveDataIO
{
	public bool IsBackupBreakFile;

	private WebApiGate webApiGate;

	[Preserve]
	public SaveDataIO(WebApiGate webApiGate)
	{
		this.webApiGate = webApiGate;
		ES3Settings eS3Settings = new ES3Settings();
		eS3Settings.location = ES3.Location.File;
		_ = eS3Settings.FullPath;
		CreateSaveDirectoryIfNotExist();
	}

	private static bool IsBackupRequired<T>()
	{
		return typeof(T).Name switch
		{
			"PlayerData" => true, 
			"PlayerDataV3" => true, 
			"SettingData" => true, 
			"MusicSettingV2" => true, 
			"EnviromentPresetsData" => true, 
			"DecorationPresetsData" => true, 
			"ScenarioProgressData" => true, 
			"PointPurchaseData" => true, 
			"PointPurchaseDataV3" => true, 
			"CollaborationSaveData" => true, 
			"FilesSnapshot" => true, 
			"AccountData" => true, 
			_ => false, 
		};
	}

	public void Save<T>(T value)
	{
		if (webApiGate.AccountGate.Value)
		{
			ISaveDataMetaGenerator generator = SaveDataMetaProvider<T>.Generator;
			string text = Path.Combine(BulbulConstant.SaveDirectoryPath, generator.FileName);
			if (IsBackupRequired<T>())
			{
				CreateBackup<T>(generator, text);
			}
			ES3.Save(generator.Key, value, text);
		}
	}

	public void Save<T>(T value, string prefix)
	{
		if (webApiGate.AccountGate.Value)
		{
			ISaveDataMetaGenerator generator = SaveDataMetaProvider<T>.Generator;
			string text = (prefix.IsNullOrEmpty() ? Path.Combine(BulbulConstant.SaveDirectoryPath, generator.FileName) : Path.Combine(BulbulConstant.SaveDirectoryPath, generator.FileNameWithoutExtension + "_" + prefix + generator.Extension));
			if (IsBackupRequired<T>())
			{
				CreateBackup<T>(generator, text);
			}
			ES3.Save(generator.Key, value, text);
		}
	}

	public T Load<T>()
	{
		ISaveDataMetaGenerator generator = SaveDataMetaProvider<T>.Generator;
		string text = Path.Combine(BulbulConstant.SaveDirectoryPath, generator.FileName);
		try
		{
			if (!ES3.KeyExists(generator.Key, text))
			{
				return TryRestore<T>(text, setBackupBreakFile: false);
			}
			return ES3.Load<T>(generator.Key, text);
		}
		catch (FormatException exception)
		{
			Debug.LogException(exception);
			return TryRestore<T>(text, setBackupBreakFile: true);
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			Debug.LogError($"正常に{typeof(T).Name}を読み込めませんでした\n{ex}");
			return TryRestore<T>(text, setBackupBreakFile: true);
		}
	}

	public bool TryLoad<T>(out T result)
	{
		result = default(T);
		ISaveDataMetaGenerator generator = SaveDataMetaProvider<T>.Generator;
		string filePath = Path.Combine(BulbulConstant.SaveDirectoryPath, generator.FileName);
		return TryLoadByPath<T>(filePath, out result);
	}

	public T Load<T>(string prefix)
	{
		ISaveDataMetaGenerator generator = SaveDataMetaProvider<T>.Generator;
		string text = (prefix.IsNullOrEmpty() ? Path.Combine(BulbulConstant.SaveDirectoryPath, generator.FileName) : Path.Combine(BulbulConstant.SaveDirectoryPath, generator.FileNameWithoutExtension + "_" + prefix + generator.Extension));
		try
		{
			if (!ES3.KeyExists(generator.Key, text))
			{
				return TryRestore<T>(text, setBackupBreakFile: false);
			}
			return ES3.Load<T>(generator.Key, text);
		}
		catch (FormatException exception)
		{
			Debug.LogException(exception);
			return TryRestore<T>(text, setBackupBreakFile: true);
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			Debug.LogError($"正常に{typeof(T).Name}を読み込めませんでした\n{ex}");
			return TryRestore<T>(text, setBackupBreakFile: true);
		}
	}

	public bool TryLoad<T>(string prefix, out T result)
	{
		result = default(T);
		ISaveDataMetaGenerator generator = SaveDataMetaProvider<T>.Generator;
		string filePath = (prefix.IsNullOrEmpty() ? Path.Combine(BulbulConstant.SaveDirectoryPath, generator.FileName) : Path.Combine(BulbulConstant.SaveDirectoryPath, generator.FileNameWithoutExtension + "_" + prefix + generator.Extension));
		return TryLoadByPath<T>(filePath, out result);
	}

	public bool TryLoad<T>(string prefix, string extension, out T result)
	{
		result = default(T);
		ISaveDataMetaGenerator generator = SaveDataMetaProvider<T>.Generator;
		string filePath = (prefix.IsNullOrEmpty() ? Path.Combine(BulbulConstant.SaveDirectoryPath, generator.FileNameWithoutExtension + extension) : Path.Combine(BulbulConstant.SaveDirectoryPath, generator.FileNameWithoutExtension + "_" + prefix + extension));
		return TryLoadByPath<T>(filePath, out result);
	}

	public bool TryLoadByPath<T>(string filePath, out T result)
	{
		result = default(T);
		try
		{
			string key = SaveDataMetaProvider<T>.Generator.Key;
			if (!ES3.KeyExists(key, filePath))
			{
				return TryRestore<T>(filePath, out result, setBackupBreakFile: false);
			}
			result = ES3.Load<T>(key, filePath);
			return true;
		}
		catch (FormatException exception)
		{
			Debug.LogException(exception);
			return TryRestore<T>(filePath, out result, setBackupBreakFile: true);
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			Debug.LogError($"正常に{typeof(T).Name}を読み込めませんでした\n{ex}");
			return TryRestore<T>(filePath, out result, setBackupBreakFile: true);
		}
	}

	public bool TryLoadFile<T>(string prefix, string extension, out FileInfo result)
	{
		ISaveDataMetaGenerator generator = SaveDataMetaProvider<T>.Generator;
		if (extension.IsNullOrEmpty())
		{
			extension = generator.Extension;
		}
		string filePath = (prefix.IsNullOrEmpty() ? Path.Combine(BulbulConstant.SaveDirectoryPath, generator.FileNameWithoutExtension + extension) : Path.Combine(BulbulConstant.SaveDirectoryPath, generator.FileNameWithoutExtension + "_" + prefix + extension));
		return TryLoadFile<T>(filePath, out result);
	}

	public bool TryLoadFile<T>(string filePath, out FileInfo result)
	{
		result = null;
		T result2;
		try
		{
			if (!ES3.KeyExists(SaveDataMetaProvider<T>.Generator.Key, filePath))
			{
				if (TryRestore<T>(filePath, out result2, setBackupBreakFile: false))
				{
					result = new FileInfo(new ES3Settings(filePath).FullPath);
					return result.Exists;
				}
				return false;
			}
			result = new FileInfo(new ES3Settings(filePath).FullPath);
			return result.Exists;
		}
		catch (FormatException exception)
		{
			Debug.LogException(exception);
			if (TryRestore<T>(filePath, out result2, setBackupBreakFile: true))
			{
				result = new FileInfo(new ES3Settings(filePath).FullPath);
				return result.Exists;
			}
			return false;
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			Debug.LogError($"正常に{typeof(T).Name}を読み込めませんでした\n{ex}");
			if (TryRestore<T>(filePath, out result2, setBackupBreakFile: true))
			{
				result = new FileInfo(new ES3Settings(filePath).FullPath);
				return result.Exists;
			}
			return false;
		}
	}

	public bool TryLoadAll<T>(out IEnumerable<T> result)
	{
		result = null;
		DirectoryInfo directoryInfo = new DirectoryInfo(new ES3Settings(BulbulConstant.SaveDirectoryPath).FullPath);
		if (!directoryInfo.Exists)
		{
			return false;
		}
		result = GetFiles(this, directoryInfo);
		return true;
		static IEnumerable<T> GetFiles(SaveDataIO @this, DirectoryInfo directory)
		{
			FileInfo[] files = directory.GetFiles();
			foreach (FileInfo fileInfo in files)
			{
				if (fileInfo.Name.StartsWith(SaveDataMetaProvider<T>.Generator.FileNameWithoutExtension + "_"))
				{
					string filePath = Path.Combine(BulbulConstant.SaveDirectoryPath, fileInfo.Name);
					if (@this.TryLoadByPath<T>(filePath, out var result2))
					{
						yield return result2;
					}
				}
			}
		}
	}

	public void SaveJson<T>(T value, string prefix = "")
	{
		ISaveDataMetaGenerator generator = SaveDataMetaProvider<T>.Generator;
		string path = (prefix.IsNullOrEmpty() ? Path.Combine(BulbulConstant.SaveDirectoryPath, generator.FileName) : Path.Combine(BulbulConstant.SaveDirectoryPath, generator.FileNameWithoutExtension + "_" + prefix + generator.Extension));
		string fullPath = new ES3Settings(path).FullPath;
		if (IsBackupRequired<T>())
		{
			CreateBackupJson<T>(path);
		}
		byte[] array = JsonSerializer.SerializeToUtf8Bytes(value, FileSaveSourceGenerationContext.Default.Options);
		NativeFile.WriteAllBytes(fullPath, array);
	}

	public bool TryLoadJson<T>(out T result, string prefix = "")
	{
		result = default(T);
		ISaveDataMetaGenerator generator = SaveDataMetaProvider<T>.Generator;
		string path = (prefix.IsNullOrEmpty() ? Path.Combine(BulbulConstant.SaveDirectoryPath, generator.FileName) : Path.Combine(BulbulConstant.SaveDirectoryPath, generator.FileNameWithoutExtension + "_" + prefix + generator.Extension));
		string fullPath = new ES3Settings(path).FullPath;
		try
		{
			if (!File.Exists(fullPath))
			{
				return TryRestoreJson<T>(path, out result, setBackupBreakFile: false);
			}
			result = ReadJsonDeserialize<T>(fullPath);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			Debug.LogError($"正常に{typeof(T).Name}を読み込めませんでした\n{ex}");
			return TryRestoreJson<T>(path, out result, setBackupBreakFile: true);
		}
	}

	private bool CreateBackupJson<T>(string path)
	{
		try
		{
			string fullPath = new ES3Settings(path).FullPath;
			fullPath = fullPath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
			if (!File.Exists(fullPath))
			{
				return false;
			}
			if (!IsSafeJsonSaveFile<T>(fullPath))
			{
				return false;
			}
			string text = fullPath + ".bak";
			if (File.Exists(text))
			{
				File.Replace(fullPath, text, null);
			}
			else
			{
				File.Move(fullPath, text);
			}
			return true;
		}
		catch (Exception arg)
		{
			Debug.LogWarning($"バックアップファイルの作成に失敗しました: {path}\n{arg}");
			return false;
		}
	}

	private bool IsSafeJsonSaveFile<T>(string fullPath)
	{
		try
		{
			ReadJsonDeserialize<T>(fullPath);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			Debug.LogError($"正常に{typeof(T).Name}を読み込め無かった為、バックアップファイルを更新出来ませんでした\n{ex}");
			return false;
		}
	}

	private T ReadJsonDeserialize<T>(string fullPath)
	{
		using FileStream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		byte[] array = ArrayPool<byte>.Shared.Rent((int)fileStream.Length);
		try
		{
			int length = fileStream.Read(array, 0, array.Length);
			return JsonSerializer.Deserialize<T>(MemoryExtensions.AsSpan(array, 0, length), FileSaveSourceGenerationContext.Default.Options);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(array);
		}
	}

	private bool TryRestoreJson<T>(string path, out T result, bool setBackupBreakFile)
	{
		result = default(T);
		string fullPath = new ES3Settings(path).FullPath;
		if (File.Exists(fullPath))
		{
			BreakJsonFileBackup(fullPath);
		}
		if (!IsBackupRequired<T>())
		{
			return false;
		}
		if (TryRestoreFromJsonBackup<T>(fullPath, out result))
		{
			return result != null;
		}
		if (setBackupBreakFile)
		{
			IsBackupBreakFile = true;
		}
		return false;
	}

	private void BreakJsonFileBackup(string fullPath)
	{
		try
		{
			File.Move(fullPath, $"{fullPath}_{DateTime.Now: yyyyMMddhhmmss}.bak");
			Debug.LogError("ファイルが壊れていたので「" + fullPath + "」を作成しました");
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private bool TryRestoreFromJsonBackup<T>(string fullPath, out T result)
	{
		result = default(T);
		try
		{
			string text = fullPath + ".bak";
			if (!File.Exists(text))
			{
				return false;
			}
			result = ReadJsonDeserialize<T>(fullPath);
			File.Copy(text, fullPath, overwrite: true);
			Debug.LogWarning(typeof(T).Name + "をバックアップ(json)から復旧しました: " + text + " -> " + fullPath);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			Debug.LogError($"{typeof(T).Name}のバックアップ(json)からの復旧に失敗しました\n{ex}");
			return false;
		}
	}

	public void SavePrefs<T>(T value)
	{
		string key = SaveDataMetaProvider<T>.Generator.Key;
		string value2 = JsonSerializer.Serialize(value, FileSaveSourceGenerationContext.Default.Options);
		PlayerPrefs.SetString(key, value2);
		PlayerPrefs.Save();
	}

	public bool TryLoadPrefs<T>(out T value)
	{
		value = default(T);
		string key = SaveDataMetaProvider<T>.Generator.Key;
		if (!PlayerPrefs.HasKey(key))
		{
			return false;
		}
		string json = PlayerPrefs.GetString(key);
		try
		{
			value = JsonSerializer.Deserialize<T>(json, FileSaveSourceGenerationContext.Default.Options);
		}
		catch
		{
			return false;
		}
		return true;
	}

	public void SetPrefsKeyOnly<T>(string key = "")
	{
		key = (key.IsNullOrEmpty() ? SaveDataMetaProvider<T>.Generator.Key : ZString.Concat(SaveDataMetaProvider<T>.Generator.Key, "_", key));
		PlayerPrefs.SetInt(key, 0);
		PlayerPrefs.Save();
	}

	public bool HasPrefsKey<T>(string key = "")
	{
		key = (key.IsNullOrEmpty() ? SaveDataMetaProvider<T>.Generator.Key : ZString.Concat(SaveDataMetaProvider<T>.Generator.Key, "_", key));
		return PlayerPrefs.HasKey(key);
	}

	public void DeletePrefsKey<T>(string key = "")
	{
		key = (key.IsNullOrEmpty() ? SaveDataMetaProvider<T>.Generator.Key : ZString.Concat(SaveDataMetaProvider<T>.Generator.Key, "_", key));
		PlayerPrefs.DeleteKey(key);
	}

	public bool Delete<T>(string prefix = null)
	{
		ISaveDataMetaGenerator generator = SaveDataMetaProvider<T>.Generator;
		string filePath = (prefix.IsNullOrEmpty() ? Path.Combine(BulbulConstant.SaveDirectoryPath, generator.FileName) : Path.Combine(BulbulConstant.SaveDirectoryPath, generator.FileNameWithoutExtension + "_" + prefix + generator.Extension));
		try
		{
			ES3.DeleteFile(filePath);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			Debug.LogError($"正常に{typeof(T).Name}_{prefix}を削除できませんでした\n{ex}");
			return false;
		}
	}

	public void RenameMigratedFile<T>(string prefix = null)
	{
		ISaveDataMetaGenerator generator = SaveDataMetaProvider<T>.Generator;
		string fullPath = new ES3Settings(prefix.IsNullOrEmpty() ? Path.Combine(BulbulConstant.SaveDirectoryPath, generator.FileName) : Path.Combine(BulbulConstant.SaveDirectoryPath, generator.FileNameWithoutExtension + "_" + prefix + generator.Extension)).FullPath;
		if (File.Exists(fullPath))
		{
			string text = fullPath + ".migrated";
			if (File.Exists(text))
			{
				File.Replace(fullPath, text, null);
			}
			else
			{
				File.Move(fullPath, text);
			}
		}
		string path = fullPath + ".bak";
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}

	public void RenameMigratedFile(FileInfo migratedFile)
	{
		string fullName = migratedFile.FullName;
		string text = fullName + ".migrated";
		if (File.Exists(text))
		{
			File.Replace(fullName, text, null);
		}
		else
		{
			File.Move(fullName, text);
		}
	}

	public bool DeleteAll()
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(new ES3Settings(BulbulConstant.SaveDirectoryPath).FullPath);
		if (!directoryInfo.Exists)
		{
			return true;
		}
		try
		{
			FileInfo[] files = directoryInfo.GetFiles();
			foreach (FileInfo fileInfo in files)
			{
				try
				{
					fileInfo.Delete();
				}
				catch
				{
					Debug.LogError(fileInfo.Name + "は削除できませんでした");
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			Debug.LogError(string.Format("正常に{0}を実行できませんでした\n{1}", "DeleteAll", ex));
			return false;
		}
	}

	public bool DeleteAllExcludeLocalData()
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(new ES3Settings(BulbulConstant.SaveDirectoryPath).FullPath);
		if (!directoryInfo.Exists)
		{
			return true;
		}
		string[] source = new string[3]
		{
			SaveDataMetaProvider<AccountData>.Generator.FileName,
			SaveDataMetaProvider<AccountData>.Generator.FileName + ".bak",
			SaveDataMetaProvider<LocalMusicSetting>.Generator.FileName
		};
		try
		{
			FileInfo[] files = directoryInfo.GetFiles();
			foreach (FileInfo fileInfo in files)
			{
				if (!source.Contains(fileInfo.Name, StringComparer.InvariantCultureIgnoreCase))
				{
					try
					{
						fileInfo.Delete();
					}
					catch
					{
						Debug.LogError(fileInfo.Name + "は削除できませんでした");
					}
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			Debug.LogError(string.Format("正常に{0}を実行できませんでした\n{1}", "DeleteAllExcludeLocalData", ex));
			return false;
		}
	}

	public bool KeyExists<T>()
	{
		ISaveDataMetaGenerator generator = SaveDataMetaProvider<T>.Generator;
		string filePath = Path.Combine(BulbulConstant.SaveDirectoryPath, generator.FileName);
		return ES3.KeyExists(generator.Key, filePath);
	}

	public bool FileExists<T>(string prefix = null)
	{
		ISaveDataMetaGenerator generator = SaveDataMetaProvider<T>.Generator;
		return File.Exists(prefix.IsNullOrEmpty() ? Path.Combine(BulbulConstant.SaveDirectoryPath, generator.FileName) : Path.Combine(BulbulConstant.SaveDirectoryPath, generator.FileNameWithoutExtension + "_" + prefix + generator.Extension));
	}

	private void CreateSaveDirectoryIfNotExist()
	{
		if (!Directory.Exists(Path.Combine(Application.persistentDataPath, BulbulConstant.SaveDirectoryPath)))
		{
			Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, BulbulConstant.SaveDirectoryPath));
		}
	}

	private void BreakFileBackup(string path)
	{
		try
		{
			path = new ES3Settings(path).FullPath;
			File.Move(path, $"{path}_{DateTime.Now: yyyyMMddhhmmss}.bak");
			Debug.LogError("ファイルが壊れていたので「" + path + "」を作成しました");
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private bool CreateBackup<T>(ISaveDataMetaGenerator meta, string path)
	{
		try
		{
			string fullPath = new ES3Settings(path).FullPath;
			fullPath = fullPath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
			if (!File.Exists(fullPath))
			{
				return false;
			}
			if (!IsSafeSaveFile<T>(meta, path))
			{
				return false;
			}
			string text = fullPath + ".bak";
			if (File.Exists(text))
			{
				File.Replace(fullPath, text, null);
			}
			else
			{
				File.Move(fullPath, text);
			}
			return true;
		}
		catch (Exception arg)
		{
			Debug.LogWarning($"バックアップファイルの作成に失敗しました: {path}\n{arg}");
			return false;
		}
	}

	private bool IsSafeSaveFile<T>(ISaveDataMetaGenerator meta, string path)
	{
		try
		{
			ES3.Load<T>(meta.Key, path);
			return true;
		}
		catch (FormatException ex)
		{
			Debug.LogException(ex);
			Debug.LogError($"{typeof(T).Name}が破損している為バックアップファイルを更新出来ませんでした\n{ex}");
			return false;
		}
		catch (Exception ex2)
		{
			Debug.LogException(ex2);
			Debug.LogError($"正常に{typeof(T).Name}を読み込め無かった為、バックアップファイルを更新出来ませんでした\n{ex2}");
			return false;
		}
	}

	private T TryRestore<T>(string path, bool setBackupBreakFile)
	{
		if (File.Exists(new ES3Settings(path).FullPath))
		{
			BreakFileBackup(path);
		}
		if (!IsBackupRequired<T>())
		{
			return default(T);
		}
		if (TryRestoreFromBackup<T>(path, out var result))
		{
			return result;
		}
		if (setBackupBreakFile)
		{
			IsBackupBreakFile = true;
		}
		return default(T);
	}

	private bool TryRestore<T>(string path, out T result, bool setBackupBreakFile)
	{
		result = default(T);
		if (File.Exists(new ES3Settings(path).FullPath))
		{
			BreakFileBackup(path);
		}
		if (!IsBackupRequired<T>())
		{
			return false;
		}
		if (TryRestoreFromBackup<T>(path, out result))
		{
			return result != null;
		}
		if (setBackupBreakFile)
		{
			IsBackupBreakFile = true;
		}
		return false;
	}

	private bool TryRestoreFromBackup<T>(string path, out T result)
	{
		result = default(T);
		try
		{
			string fullPath = new ES3Settings(path).FullPath;
			string text = fullPath + ".bak";
			if (!File.Exists(text))
			{
				return false;
			}
			ISaveDataMetaGenerator generator = SaveDataMetaProvider<T>.Generator;
			result = ES3.Load<T>(generator.Key, text);
			File.Copy(text, fullPath, overwrite: true);
			Debug.LogWarning(typeof(T).Name + "をバックアップから復旧しました: " + text + " -> " + fullPath);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			Debug.LogError($"{typeof(T).Name}のバックアップからの復旧に失敗しました\n{ex}");
			return false;
		}
	}

	public IEnumerable<string> GetDirtyTargets()
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(new ES3Settings(BulbulConstant.SaveDirectoryPath).FullPath);
		if (!directoryInfo.Exists)
		{
			yield break;
		}
		foreach (FileInfo item in directoryInfo.EnumerateFiles())
		{
			if (!(item.Extension != ".es3"))
			{
				yield return item.FullName;
			}
		}
	}
}
