using System;
using System.Collections.Generic;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class PointPurchaseDataV3
{
	public int Point;

	public List<string> PurchasedEnvironments = new List<string>();

	public List<string> PurchasedDecorations = new List<string>();

	public List<string> PurchasedAmbientSounds = new List<string>();
}
