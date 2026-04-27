using System;
using System.Collections.Generic;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class PageDataV2
{
	public class EqualityComparer : IEqualityComparer<PageDataV2>
	{
		public bool Equals(PageDataV2 x, PageDataV2 y)
		{
			if (x == y)
			{
				return true;
			}
			if (x == null)
			{
				return false;
			}
			if (y == null)
			{
				return false;
			}
			if (x.GetType() != y.GetType())
			{
				return false;
			}
			return x.UniqueID == y.UniqueID;
		}

		public int GetHashCode(PageDataV2 obj)
		{
			return obj.UniqueID.GetHashCode();
		}
	}

	public ulong UniqueID;

	public string MainText;

	public PageDataV2()
	{
		UniqueID = UniqueIDGenerator.GetNewID();
		MainText = null;
	}
}
