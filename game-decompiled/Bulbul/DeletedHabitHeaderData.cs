using System;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class DeletedHabitHeaderData
{
	public string HabitID;

	public string Title;
}
