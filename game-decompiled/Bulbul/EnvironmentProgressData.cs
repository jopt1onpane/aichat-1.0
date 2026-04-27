using System;
using System.Collections.Generic;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class EnvironmentProgressData
{
	public List<string> PlayedEnvironment = new List<string>();
}
