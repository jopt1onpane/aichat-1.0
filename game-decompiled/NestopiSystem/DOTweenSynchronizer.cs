using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace NestopiSystem;

public class DOTweenSynchronizer
{
	private readonly Dictionary<string, HashSet<Tween>> syncTweens = new Dictionary<string, HashSet<Tween>>();

	public Tween Register(Tween tween, string key = null, bool autoUnregister = true)
	{
		if (tween == null || !tween.active)
		{
			return tween;
		}
		if (string.IsNullOrEmpty(key))
		{
			key = GetDefaultSyncKey(tween);
		}
		if (!syncTweens.TryGetValue(key, out var value))
		{
			value = new HashSet<Tween>();
			syncTweens.Add(key, value);
		}
		value.Add(tween);
		if (autoUnregister)
		{
			tween.OnKill(delegate
			{
				Unregister(tween);
			});
		}
		return tween;
	}

	public void Unregister(Tween tween, string key = null)
	{
		if (tween == null)
		{
			return;
		}
		if (tween.active)
		{
			if (string.IsNullOrEmpty(key))
			{
				key = GetDefaultSyncKey(tween);
			}
			if (syncTweens.TryGetValue(key, out var value))
			{
				value.Remove(tween);
				if (value.Count == 0)
				{
					syncTweens.Remove(key);
				}
			}
			return;
		}
		foreach (HashSet<Tween> value2 in syncTweens.Values)
		{
			value2.Remove(tween);
		}
		foreach (KeyValuePair<string, HashSet<Tween>> item in syncTweens.Where((KeyValuePair<string, HashSet<Tween>> kvp) => kvp.Value.Count == 0).Reverse())
		{
			syncTweens.Remove(item.Key);
		}
	}

	private static string GetDefaultSyncKey(Tween tween)
	{
		if (tween == null || !tween.active)
		{
			return null;
		}
		float num = tween.Duration(includeLoops: false);
		if (Mathf.Approximately(num, 0f))
		{
			Debug.LogWarning("Durationが0のTweenが指定されました。");
		}
		return $"duration_{num}";
	}

	public Tween Sync(Tween tween, string key = null)
	{
		if (syncTweens.Count == 0)
		{
			return tween;
		}
		if (string.IsNullOrEmpty(key))
		{
			key = GetDefaultSyncKey(tween);
		}
		if (!syncTweens.ContainsKey(key))
		{
			return tween;
		}
		Tween tween2 = syncTweens.GetValueOrDefault(key).FirstOrDefault(delegate(Tween x)
		{
			if (x != tween && x.active && x.IsPlaying())
			{
				GameObject obj = x.target as GameObject;
				if ((object)obj == null || obj.activeInHierarchy)
				{
					return (x.target as Component)?.gameObject?.activeInHierarchy ?? true;
				}
			}
			return false;
		});
		if (tween2 == null)
		{
			return tween;
		}
		float num = 0f;
		num = tween2.position;
		if (tween.hasLoops && IsYoyoBackwards(tween2))
		{
			num += tween.Duration(includeLoops: false);
		}
		tween.Goto(num);
		return tween;
	}

	public void Clear()
	{
		if (syncTweens.Count == 0)
		{
			return;
		}
		foreach (HashSet<Tween> value in syncTweens.Values)
		{
			value.Clear();
		}
		syncTweens.Clear();
	}

	private static bool IsYoyoBackwards(Tween tween)
	{
		if (!tween.hasLoops)
		{
			return false;
		}
		return !Mathf.Approximately(tween.ElapsedPercentage(includeLoops: false), tween.ElapsedDirectionalPercentage());
	}
}
