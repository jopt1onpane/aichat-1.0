using System;
using System.Globalization;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class TodoData
{
	public ulong UniqueID;

	public string TodoText;

	public TodoState CurrentState;

	[ES3Serializable]
	private string completeDateTimeString;

	[ES3Serializable]
	private string expireDateTimeString;

	[ES3NonSerializable]
	public DateTime? Completed
	{
		get
		{
			if (!DateTime.TryParseExact(completeDateTimeString, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
			{
				return null;
			}
			return result;
		}
	}

	[ES3NonSerializable]
	public DateTime? Expire
	{
		get
		{
			if (!DateTime.TryParseExact(expireDateTimeString, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
			{
				return null;
			}
			return result;
		}
	}

	public void SetCompleteTodoDatetime(DateTime? datetime)
	{
		completeDateTimeString = datetime?.ToString("yyyyMMddHHmmss") ?? "";
		CurrentState = (datetime.HasValue ? TodoState.Complete : TodoState.Working);
	}

	public void SetExpire(DateTime? datetime)
	{
		expireDateTimeString = datetime?.ToString("yyyyMMddHHmmss") ?? "";
	}

	public TodoData()
	{
		UniqueID = UniqueIDGenerator.GetNewID();
		TodoText = null;
		CurrentState = TodoState.Working;
	}

	public void Uncomplete()
	{
		completeDateTimeString = null;
		CurrentState = TodoState.Working;
	}

	public TodoData DeepCopy()
	{
		return new TodoData
		{
			UniqueID = UniqueID,
			TodoText = TodoText,
			CurrentState = CurrentState,
			completeDateTimeString = completeDateTimeString,
			expireDateTimeString = expireDateTimeString
		};
	}
}
