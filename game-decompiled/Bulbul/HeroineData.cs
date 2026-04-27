using System;

namespace Bulbul;

[Serializable]
public class HeroineData
{
	public StartDirectionData StartDirection = new StartDirectionData();

	public void LoadSetup()
	{
		StartDirection.Setup();
	}
}
