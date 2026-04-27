using System;
using System.Collections.Generic;
using GUPS.Obfuscator.Attribute;
using NestopiSystem;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class NoteDataV2
{
	public readonly NoteList NoteList = new NoteList();

	private readonly List<PageDataV2> pageCache = new List<PageDataV2>();

	private readonly int maxCacheCount = 3;

	public IReadOnlyList<PageDataV2> PageCache => pageCache;

	public NoteDataV2()
	{
	}

	public NoteDataV2(NoteList noteList)
	{
		NoteList = noteList;
	}

	public NoteDataV2(NoteList noteList, List<PageDataV2> pageCache)
	{
		NoteList = noteList;
		this.pageCache = pageCache;
	}

	public bool AddPage(PageDataV2 pageData)
	{
		if (!NoteList.Titles.TryAdd(pageData.UniqueID, pageData.MainText))
		{
			return false;
		}
		pageCache.Add(pageData);
		if (pageCache.Count > maxCacheCount)
		{
			pageCache.RemoveAt(0);
		}
		NoteList.PageOrderList.Add(pageData.UniqueID);
		SaveDataManager.Instance.SavePageData(pageData);
		return true;
	}

	public void RemovePage(ulong pageID)
	{
		NoteList.PageOrderList.Remove(pageID);
		NoteList.Titles.Remove(pageID);
		pageCache.RemoveAll((PageDataV2 x) => x.UniqueID == pageID);
		SaveDataManager.Instance.DeletePageData(pageID);
	}

	public void SwapPage(ulong target, ulong origin)
	{
		Core(NoteList.PageOrderList, target, origin);
		SaveDataManager.Instance.SaveNoteList();
		static void Core(IList<ulong> list, ulong item, ulong num3)
		{
			int num = list.IndexOf(item);
			if (num >= 0)
			{
				int num2 = 0;
				if (num3 != 0L)
				{
					num2 = list.IndexOf(num3);
				}
				if (num2 >= 0)
				{
					if (num3 != 0L && num > num2)
					{
						num2++;
					}
					list.Move(num, num2);
				}
			}
		}
	}

	public bool SetTitleText(ulong pageID, string inputText)
	{
		if (!NoteList.Titles.ContainsKey(pageID))
		{
			return false;
		}
		NoteList.Titles[pageID] = inputText;
		return true;
	}

	public bool SetMainText(ulong pageID, string inputText, out PageDataV2 pageData)
	{
		if (pageCache.TryGetFirst(pageID, (PageDataV2 p, ulong id) => p.UniqueID == id, out pageData))
		{
			pageData.MainText = inputText;
		}
		else
		{
			pageData = new PageDataV2
			{
				UniqueID = pageID,
				MainText = inputText
			};
			pageCache.Add(pageData);
			if (pageCache.Count > maxCacheCount)
			{
				pageCache.RemoveAt(0);
			}
		}
		SaveDataManager.Instance.SavePageData(pageData);
		return true;
	}

	public PageDataV2 GetPageData(ulong pageID)
	{
		if (pageCache.Count != 0 && pageCache.TryGetFirst(pageID, (PageDataV2 p, ulong id) => p.UniqueID == id, out var first))
		{
			return first;
		}
		first = SaveDataManager.Instance.LoadPageData(pageID) ?? new PageDataV2
		{
			UniqueID = pageID
		};
		pageCache.Add(first);
		if (pageCache.Count > maxCacheCount)
		{
			pageCache.RemoveAt(0);
		}
		return first;
	}
}
