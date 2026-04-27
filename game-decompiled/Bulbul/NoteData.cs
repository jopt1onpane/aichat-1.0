using System;
using System.Collections.Generic;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[Obsolete]
[DoNotObfuscateClass]
public class NoteData
{
	public Dictionary<ulong, PageData> PageDic = new Dictionary<ulong, PageData>();

	public List<ulong> PageOrderList = new List<ulong>();
}
