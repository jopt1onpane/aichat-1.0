namespace Bulbul;

public class SpecialScenarioUtil
{
	public virtual bool IsUnlocked()
	{
		return false;
	}

	public virtual bool IsInProgress()
	{
		return false;
	}

	public virtual bool IsReadAll()
	{
		return false;
	}

	public virtual bool IsNeedNewIcon()
	{
		return false;
	}

	public virtual bool IsNeedUsePossibleAnnounce()
	{
		return false;
	}

	public virtual bool IsNeedUseFinishAnnounce()
	{
		return false;
	}

	public virtual bool IsValidPeriod()
	{
		return true;
	}
}
