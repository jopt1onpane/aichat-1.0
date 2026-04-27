using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using Cysharp.Threading.Tasks;
using NestopiSystem;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Bulbul.Web;

public static class WebApi
{
	private static WebApiGate webApiGate;

	private static string baseURL;

	private const string suffix = ".php";

	private static readonly JsonSerializerOptions defaultOptions;

	private static NativeArray<byte> emptyJson;

	private static UniTask<TResponse> GetAsyncCore<TRequest, TResponse>(CancellationToken cancellationToken = default(CancellationToken)) where TResponse : IWebApiResponse
	{
		return GetAsyncCore<TResponse>(CreateRequestURL<TRequest>(), cancellationToken);
	}

	private static UniTask<TResponse> GetAsyncCore<TRequest, TResponse>(TRequest value, CancellationToken cancellationToken = default(CancellationToken)) where TResponse : IWebApiResponse
	{
		string text = CreateRequestURL<TRequest>();
		string text2 = CreateQuery(value);
		return GetAsyncCore<TResponse>(text + text2, cancellationToken);
	}

	private static async UniTask<TResponse> GetAsyncCore<TResponse>(string url, CancellationToken cancellationToken = default(CancellationToken))
	{
		using UnityWebRequest request = UnityWebRequest.Get(url);
		return await SendAsyncCore<TResponse>(request, cancellationToken);
	}

	private static async UniTask<TResponse> SendAsyncCore<TResponse>(UnityWebRequest request, CancellationToken cancellationToken = default(CancellationToken))
	{
		await request.SendWebRequest().ToUniTask(null, PlayerLoopTiming.Update, cancellationToken);
		NativeArray<byte>.ReadOnly source = request.downloadHandler.nativeData;
		if (source.Length == 0)
		{
			source = emptyJson.AsReadOnly();
		}
		JsonTypeInfo<TResponse> jsonTypeInfo = (JsonTypeInfo<TResponse>)SourceGenerationContext.Default.GetTypeInfo(typeof(TResponse));
		if (jsonTypeInfo == null)
		{
			ThrowIfResetResponse(source);
			Debug.LogWarning($"{typeof(TResponse)}はソースジェネレータに対応していません");
			return JsonSerializer.Deserialize<TResponse>(source, defaultOptions);
		}
		ThrowIfResetResponse(source);
		return JsonSerializer.Deserialize(source, jsonTypeInfo);
	}

	private static async UniTask<FileDownLoadResponse> DownloadFileCore<TRequest>(TRequest request, string directoryPath = "", CancellationToken cancellationToken = default(CancellationToken)) where TRequest : IFileRequest<FileDownLoadResponse>
	{
		string uri = CreateURLWithQuery(request);
		string text = ".downloadtmp";
		if (directoryPath.IsNullOrEmpty())
		{
			directoryPath = new ES3Settings(BulbulConstant.SaveDirectoryPath).FullPath;
		}
		string tempPath = Path.Combine(directoryPath, Guid.NewGuid().ToString() + text);
		using UnityWebRequest www = UnityWebRequest.Get(uri);
		DownloadHandlerFile downloadHandlerFile = new DownloadHandlerFile(tempPath);
		downloadHandlerFile.removeFileOnAbort = true;
		www.downloadHandler = downloadHandlerFile;
		try
		{
			await www.SendWebRequest().ToUniTask(null, PlayerLoopTiming.Update, cancellationToken);
			if (www.GetResponseHeader("Content-Type").Contains("application/octet-stream"))
			{
				string text2 = Path.Combine(directoryPath, request.FileName);
				if (File.Exists(text2))
				{
					string text3 = tempPath + "bak";
					File.Replace(tempPath, text2, text3);
					File.Delete(text3);
				}
				else
				{
					File.Move(tempPath, text2);
				}
				return new FileDownLoadResponse(new FileInfo(text2), ErrorCode.None);
			}
		}
		finally
		{
			if (File.Exists(tempPath))
			{
				File.Delete(tempPath);
			}
		}
		byte[] data = www.downloadHandler.data;
		ThrowIfResetResponse(data);
		return new FileDownLoadResponse(null, JsonSerializer.Deserialize(data, SourceGenerationContext.Default.ErrorCodeResponse).ErrorCode);
	}

	public static void SetWebApiGate(WebApiGate webApiGate)
	{
		WebApi.webApiGate = webApiGate;
	}

	private static void ThrowIfResetResponse(ReadOnlySpan<byte> json)
	{
		try
		{
			ResetReasonResponse resetReasonResponse = JsonSerializer.Deserialize(json, ResetReasonResponseGenerationContext.Default.ResetReasonResponse);
			if (resetReasonResponse.ResetReason == ResetReason.None)
			{
				return;
			}
			throw new AppResetException(resetReasonResponse.ResetReason);
		}
		catch (Exception ex) when (ex is JsonException || ex is NotSupportedException)
		{
		}
	}

	private static string CreateURLWithQuery<TRequest>(TRequest request)
	{
		return CreateRequestURL<TRequest>() + CreateQuery(request);
	}

	private static string CreateRequestURL<TRequest>()
	{
		string text = CreateBaseURL();
		Type typeFromHandle = typeof(TRequest);
		string text2 = typeFromHandle.Name;
		WebApiRequestAttribute customAttribute = typeFromHandle.GetCustomAttribute<WebApiRequestAttribute>();
		if (customAttribute != null && !customAttribute.Route.IsNullOrEmpty())
		{
			text2 = customAttribute.Route;
		}
		return string.Create(text.Length + text2.Length + ".php".Length, (text, text2, ".php"), delegate(Span<char> buffer, (string baseURL, string typeName, string suffix) state)
		{
			(string baseURL, string typeName, string suffix) tuple = state;
			string item = tuple.baseURL;
			string item2 = tuple.typeName;
			string item3 = tuple.suffix;
			int length = item.Length;
			MemoryExtensions.AsSpan(item).CopyTo(buffer);
			buffer[length] = char.ToLowerInvariant(item2[0]);
			for (int i = 1; i < item2.Length; i++)
			{
				buffer[length + i] = item2[i];
			}
			length += item2.Length;
			ReadOnlySpan<char> readOnlySpan = MemoryExtensions.AsSpan(item3);
			Span<char> span = buffer;
			int num = length;
			readOnlySpan.CopyTo(span.Slice(num, span.Length - num));
		});
	}

	private static string CreateQuery<TRequest>(TRequest request)
	{
		IEnumerable<MemberInfo> enumerable = from m in typeof(TRequest).GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			where m is FieldInfo || m is PropertyInfo
			select m;
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		foreach (MemberInfo item in enumerable)
		{
			WebApiQueryParamAttribute customAttribute = item.GetCustomAttribute<WebApiQueryParamAttribute>();
			if (customAttribute == null)
			{
				continue;
			}
			object obj = ((item is PropertyInfo propertyInfo) ? propertyInfo.GetValue(request) : ((!(item is FieldInfo fieldInfo)) ? null : fieldInfo.GetValue(request)));
			object obj2 = obj;
			if (obj2 != null)
			{
				if (obj2 is Enum)
				{
					obj2 = Convert.ToInt32(obj2);
				}
				string arg = ((num == 0) ? "?" : "&");
				stringBuilder.Append($"{arg}{customAttribute.Key}={obj2}");
				num++;
			}
		}
		return stringBuilder.ToString();
	}

	private static List<IMultipartFormSection> CreateFormData<TRequest>(TRequest request)
	{
		List<IMultipartFormSection> list = new List<IMultipartFormSection>();
		foreach (MemberInfo item in from m in typeof(TRequest).GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			where m is FieldInfo || m is PropertyInfo
			select m)
		{
			WebApiFormAttribute customAttribute = item.GetCustomAttribute<WebApiFormAttribute>();
			if (customAttribute == null)
			{
				continue;
			}
			object obj = ((item is PropertyInfo propertyInfo) ? propertyInfo.GetValue(request) : ((!(item is FieldInfo fieldInfo)) ? null : fieldInfo.GetValue(request)));
			object obj2 = obj;
			if (obj2 == null)
			{
				continue;
			}
			string key = customAttribute.Key;
			if (!(obj2 is byte[] data))
			{
				if (!(obj2 is NativeArray<byte> nativeArray))
				{
					if (!(obj2 is string data2))
					{
						if (!(obj2 is string[] array))
						{
							if (!(obj2 is Enum value))
							{
								if (!(obj2 is FileInfo fileInfo))
								{
									if (obj2 is FileInfo[] array2)
									{
										if (array2 == null || array2.Length == 0)
										{
											continue;
										}
										FileInfo[] array3 = array2;
										foreach (FileInfo fileInfo2 in array3)
										{
											if (!fileInfo2.Exists)
											{
												break;
											}
											list.Add(new MultipartFormFileSection(key, File.ReadAllBytes(fileInfo2.FullName), fileInfo2.Name, "application/octet-stream"));
										}
									}
									else if (obj2 == null)
									{
										list.Add(new MultipartFormDataSection(key, (string)null));
									}
									else
									{
										JsonTypeInfo typeInfo = SourceGenerationContext.Default.GetTypeInfo(obj2.GetType());
										if (typeInfo == null)
										{
											Debug.LogWarning($"{obj2.GetType()}はソースジェネレータに対応していません");
											list.Add(new MultipartFormDataSection(key, JsonSerializer.SerializeToUtf8Bytes(obj2, defaultOptions), "application/json"));
										}
										list.Add(new MultipartFormDataSection(key, JsonSerializer.SerializeToUtf8Bytes(obj2, typeInfo), "application/json"));
									}
								}
								else if (fileInfo.Exists)
								{
									list.Add(new MultipartFormFileSection(key, File.ReadAllBytes(fileInfo.FullName), fileInfo.Name, "application/octet-stream"));
								}
							}
							else
							{
								list.Add(new MultipartFormDataSection(key, Convert.ToInt32(value).ToString()));
							}
						}
						else
						{
							string[] array4 = array;
							foreach (string data3 in array4)
							{
								list.Add(new MultipartFormDataSection(key, data3));
							}
						}
					}
					else
					{
						list.Add(new MultipartFormDataSection(key, data2));
					}
				}
				else
				{
					list.Add(new MultipartFormDataSection(key, nativeArray.ToArray()));
				}
			}
			else
			{
				list.Add(new MultipartFormDataSection(key, data));
			}
		}
		return list;
	}

	private static string CreateBaseURL()
	{
		if (baseURL.IsNullOrEmpty())
		{
			WebEnvConfig webEnvConfig = Resources.Load<WebEnvConfig>("WebEnv/WebEnvConfig");
			if (!webEnvConfig)
			{
				throw new ArgumentNullException("config");
			}
			baseURL = webEnvConfig.ActiveProfile.WebApiUrl;
		}
		return baseURL;
	}

	static WebApi()
	{
		defaultOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
		emptyJson = new NativeArray<byte>(Encoding.UTF8.GetBytes("{}"), Allocator.Persistent);
	}

	public static async UniTask<WebApiResponse<TResponse>> GetAsync<TRequest, TResponse>(CancellationToken cancellationToken = default(CancellationToken)) where TRequest : IRequest<TResponse> where TResponse : IWebApiResponse
	{
		string uri = CreateRequestURL<TRequest>();
		using UnityWebRequest uwr = UnityWebRequest.Get(uri);
		return await SendAsync<TResponse>(uwr, cancellationToken);
	}

	public static async UniTask<WebApiResponse<TResponse>> GetAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default(CancellationToken)) where TRequest : IRequest<TResponse> where TResponse : IWebApiResponse
	{
		string uri = CreateURLWithQuery(request);
		using UnityWebRequest uwr = UnityWebRequest.Get(uri);
		return await SendAsync<TResponse>(uwr, cancellationToken);
	}

	public static async UniTask<WebApiResponse<TResponse>> PostAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default(CancellationToken)) where TRequest : IRequest<TResponse> where TResponse : IWebApiResponse
	{
		string uri = CreateURLWithQuery(request);
		List<IMultipartFormSection> multipartFormSections = CreateFormData(request);
		using UnityWebRequest uwr = UnityWebRequest.Post(uri, multipartFormSections);
		return await SendAsync<TResponse>(uwr, cancellationToken);
	}

	private static async UniTask<WebApiResponse<TResponse>> SendAsync<TResponse>(UnityWebRequest request, CancellationToken cancellationToken = default(CancellationToken)) where TResponse : IWebApiResponse
	{
		try
		{
			TResponse response = await SendAsyncCore<TResponse>(request, cancellationToken);
			ThrowIfHasErrorCode(response);
			return new WebApiResponse<TResponse>(response, null, UnityWebRequest.Result.Success, response.ErrorCode, ResetReason.None);
		}
		catch (AppResetException ex)
		{
			webApiGate.ResetGate.Value = false;
			return new WebApiResponse<TResponse>(default(TResponse), ex, UnityWebRequest.Result.Success, ErrorCode.None, ex.ResetReason);
		}
		catch (UnityWebRequestException ex2)
		{
			UnityEngine.Debug.LogError(ex2);
			return new WebApiResponse<TResponse>(default(TResponse), ex2, ex2.Result, ErrorCode.None, ResetReason.None);
		}
		catch (ErrorCodeException ex3)
		{
			return new WebApiResponse<TResponse>(default(TResponse), ex3, UnityWebRequest.Result.ProtocolError, ex3.ErrorCode, ResetReason.None);
		}
		catch (Exception ex4) when (ex4 is JsonException || ex4 is NotSupportedException)
		{
			return new WebApiResponse<TResponse>(default(TResponse), ex4, UnityWebRequest.Result.DataProcessingError, ErrorCode.None, ResetReason.None);
		}
		catch (Exception exception)
		{
			return new WebApiResponse<TResponse>(default(TResponse), exception, UnityWebRequest.Result.Success, ErrorCode.None, ResetReason.None);
		}
	}

	public static async UniTask<WebApiResponse<FileDownLoadResponse>> FileDownloadAsync<TRequest>(TRequest request, string directoryPath = "", CancellationToken cancellationToken = default(CancellationToken)) where TRequest : IFileRequest<FileDownLoadResponse>
	{
		try
		{
			FileDownLoadResponse response = await DownloadFileCore(request, directoryPath, cancellationToken);
			ThrowIfHasErrorCode(response);
			return new WebApiResponse<FileDownLoadResponse>(response, null, UnityWebRequest.Result.Success, response.ErrorCode, ResetReason.None);
		}
		catch (AppResetException ex)
		{
			webApiGate.ResetGate.Value = false;
			return new WebApiResponse<FileDownLoadResponse>(default(FileDownLoadResponse), ex, UnityWebRequest.Result.Success, ErrorCode.None, ex.ResetReason);
		}
		catch (UnityWebRequestException ex2)
		{
			UnityEngine.Debug.LogError(ex2);
			return new WebApiResponse<FileDownLoadResponse>(default(FileDownLoadResponse), ex2, ex2.Result, ErrorCode.None, ResetReason.None);
		}
		catch (ErrorCodeException ex3)
		{
			return new WebApiResponse<FileDownLoadResponse>(default(FileDownLoadResponse), ex3, UnityWebRequest.Result.ProtocolError, ex3.ErrorCode, ResetReason.None);
		}
		catch (Exception ex4) when (ex4 is JsonException || ex4 is NotSupportedException)
		{
			return new WebApiResponse<FileDownLoadResponse>(default(FileDownLoadResponse), ex4, UnityWebRequest.Result.DataProcessingError, ErrorCode.None, ResetReason.None);
		}
		catch (Exception exception)
		{
			return new WebApiResponse<FileDownLoadResponse>(default(FileDownLoadResponse), exception, UnityWebRequest.Result.Success, ErrorCode.None, ResetReason.None);
		}
	}

	private static void ThrowIfHasErrorCode<TResponse>(TResponse response) where TResponse : IWebApiResponse
	{
		if (response.ErrorCode == ErrorCode.DeviceLogout)
		{
			webApiGate.LoginGate.Value = false;
			throw new LogoutException();
		}
		if (response.ErrorCode != ErrorCode.None)
		{
			throw new ErrorCodeException(response.ErrorCode);
		}
	}
}
