using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using UnityEngine;

public static class Debug
{
	private const string CheckStartKey = "(DS)\n";

	private const string CheckEndKey = "\n(DE)";

	private const string _aesKey = "51924673801452367290785134286910";

	private const string _aesIv = "1452736452618903";

	public static JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
	{
		IncludeFields = true
	};

	public static void LogDeveloperCheck(string log)
	{
		log = AesEncrypt("LogDeveloperCheck: " + log);
		log = "(DS)\n" + log + "\n(DE)";
		UnityEngine.Debug.Log(log);
	}

	public static void LogWarning(string log)
	{
		log = AesEncrypt("LogWarning: " + log);
		log = "(DS)\n" + log + "\n(DE)";
		UnityEngine.Debug.LogWarning(log);
	}

	public static void LogWarning(StringBuilder stringBuilder)
	{
		string text = stringBuilder.ToString();
		text = AesEncrypt("LogWarning: " + text);
		text = "(DS)\n" + text + "\n(DE)";
		UnityEngine.Debug.LogWarning(text);
	}

	public static void LogError(string log)
	{
		log = AesEncrypt("LogError: " + log);
		log = "(DS)\n" + log + "\n(DE)";
		UnityEngine.Debug.LogError(log);
	}

	public static void LogError(Exception exception)
	{
		string text = AesEncrypt("LogError: " + exception.GetType().Name + "\n" + exception.Message);
		text = "(DS)\n" + text + "\n(DE)";
		UnityEngine.Debug.LogError(text);
	}

	public static void LogException(Exception exception)
	{
		string text = AesEncrypt("LogException: " + exception.GetType().Name + "\n" + exception.Message);
		text = "(DS)\n" + text + "\n(DE)";
		UnityEngine.Debug.LogError(text);
		UnityEngine.Debug.LogException(exception);
	}

	[Conditional("XXX")]
	public static void Log(string log)
	{
		UnityEngine.Debug.Log(log);
	}

	[Conditional("XXX")]
	public static void Assert(bool condition)
	{
	}

	[Conditional("XXX")]
	public static void Assert(bool condition, string message)
	{
	}

	[Conditional("XXX")]
	public static void Assert(bool condition, string format, params object[] args)
	{
	}

	[Conditional("XXX")]
	public static void Break()
	{
		UnityEngine.Debug.Break();
	}

	[Conditional("XXX")]
	public static void ClearDeveloperConsole()
	{
		UnityEngine.Debug.ClearDeveloperConsole();
	}

	[Conditional("XXX")]
	public static void DrawLine(Vector3 start, Vector3 end)
	{
		UnityEngine.Debug.DrawLine(start, end);
	}

	[Conditional("XXX")]
	public static void DrawLine(Vector3 start, Vector3 end, Color color)
	{
		UnityEngine.Debug.DrawLine(start, end, color);
	}

	[Conditional("XXX")]
	public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration)
	{
		UnityEngine.Debug.DrawLine(start, end, color, duration);
	}

	[Conditional("XXX")]
	public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration, bool depthTest)
	{
		UnityEngine.Debug.DrawLine(start, end, color, duration, depthTest);
	}

	[Conditional("XXX")]
	public static void DrawRay(Vector3 start, Vector3 dir)
	{
		UnityEngine.Debug.DrawRay(start, dir);
	}

	[Conditional("XXX")]
	public static void DrawRay(Vector3 start, Vector3 dir, Color color)
	{
		UnityEngine.Debug.DrawRay(start, dir, color);
	}

	[Conditional("XXX")]
	public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration)
	{
		UnityEngine.Debug.DrawRay(start, dir, color, duration);
	}

	[Conditional("XXX")]
	public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration, bool depthTest)
	{
		UnityEngine.Debug.DrawRay(start, dir, color, duration, depthTest);
	}

	[Conditional("XXX")]
	public static void Log(object message, UnityEngine.Object context = null)
	{
		UnityEngine.Debug.Log(message, context);
	}

	[Conditional("XXX")]
	public static void LogFormat(string format, params object[] args)
	{
		UnityEngine.Debug.LogFormat(format, args);
	}

	[Conditional("XXX")]
	public static void LogFormat(UnityEngine.Object context, string format, params object[] args)
	{
		UnityEngine.Debug.LogFormat(context, format, args);
	}

	[Conditional("XXX")]
	public static void LogError(object message, UnityEngine.Object context = null)
	{
		UnityEngine.Debug.LogError(message, context);
	}

	[Conditional("XXX")]
	public static void LogErrorFormat(string format, params object[] args)
	{
		UnityEngine.Debug.LogErrorFormat(format, args);
	}

	[Conditional("XXX")]
	public static void LogErrorFormat(UnityEngine.Object context, string format, params object[] args)
	{
		UnityEngine.Debug.LogErrorFormat(context, format, args);
	}

	[Conditional("XXX")]
	public static void LogException(Exception exception, UnityEngine.Object context)
	{
		UnityEngine.Debug.LogException(exception, context);
	}

	[Conditional("XXX")]
	public static void LogWarning(object message, UnityEngine.Object context = null)
	{
		UnityEngine.Debug.LogWarning(message, context);
	}

	[Conditional("XXX")]
	public static void LogWarningFormat(string format, params object[] args)
	{
		UnityEngine.Debug.LogWarningFormat(format, args);
	}

	[Conditional("XXX")]
	public static void LogWarningFormat(UnityEngine.Object context, string format, params object[] args)
	{
		UnityEngine.Debug.LogWarningFormat(context, format, args);
	}

	public static string AesEncrypt(string plain_text)
	{
		using Aes aes = Aes.Create();
		using ICryptoTransform transform = aes.CreateEncryptor(Encoding.UTF8.GetBytes("51924673801452367290785134286910"), Encoding.UTF8.GetBytes("1452736452618903"));
		using MemoryStream memoryStream = new MemoryStream();
		using CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
		using StreamWriter streamWriter = new StreamWriter(cryptoStream);
		streamWriter.Write(plain_text);
		streamWriter.Flush();
		cryptoStream.FlushFinalBlock();
		return Convert.ToBase64String(memoryStream.ToArray());
	}

	public static string AesDecrypt(string base64_text)
	{
		byte[] buffer = Convert.FromBase64String(base64_text);
		using Aes aes = Aes.Create();
		using ICryptoTransform transform = aes.CreateDecryptor(Encoding.UTF8.GetBytes("51924673801452367290785134286910"), Encoding.UTF8.GetBytes("1452736452618903"));
		using MemoryStream stream = new MemoryStream(buffer);
		using CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Read);
		using StreamReader streamReader = new StreamReader(stream2);
		return streamReader.ReadToEnd();
	}
}
