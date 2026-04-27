using System;
using System.Collections.Generic;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Obsolete]
[DoNotObfuscateClass]
public class PointPurchaseData
{
	public List<string> PurchasedEnvironments = new List<string>();

	public List<string> PurchasedDecorations = new List<string>();

	public List<string> PurchasedAmbientSounds = new List<string>();

	public PointPurchaseDataV3 ToV3(int point)
	{
		return new PointPurchaseDataV3
		{
			Point = point,
			PurchasedEnvironments = PurchasedEnvironments,
			PurchasedDecorations = PurchasedDecorations,
			PurchasedAmbientSounds = PurchasedAmbientSounds
		};
	}
}
