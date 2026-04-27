public class HeroineCommonParameter
{
	public float FatigueAmount { get; private set; }

	public float DryThroatAmount { get; private set; }

	public float SleepinessAmount { get; private set; }

	public void Init()
	{
		FatigueAmount = 0f;
		DryThroatAmount = 0f;
		SleepinessAmount = 0f;
	}

	public void AddFatigue()
	{
	}

	public void AddDryThroat()
	{
	}
}
