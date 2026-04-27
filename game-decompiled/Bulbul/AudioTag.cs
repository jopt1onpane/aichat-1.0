using System;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Flags]
[DoNotRename]
public enum AudioTag
{
	Original = 1,
	Special = 2,
	Other = 4,
	Favorite = 8,
	Local = 0x10,
	All = 0x1F
}
