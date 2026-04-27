using System;
using System.Collections.Generic;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class NoteList
{
	public Dictionary<ulong, string> Titles = new Dictionary<ulong, string>();

	public List<ulong> PageOrderList = new List<ulong>();
}
