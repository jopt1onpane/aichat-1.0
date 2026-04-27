using System;
using System.Collections.Generic;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class DecorationProgressData
{
	public List<string> PlayedDecoration = new List<string>();
}
