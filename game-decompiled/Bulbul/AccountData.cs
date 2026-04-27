using System;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class AccountData
{
	public string DeviceID;
}
