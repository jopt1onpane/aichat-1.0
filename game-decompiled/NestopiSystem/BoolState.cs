namespace NestopiSystem;

public class BoolState
{
	private bool prev;

	private bool current;

	public bool Current
	{
		set
		{
			Set(value);
		}
	}

	public bool Was
	{
		get
		{
			if (!prev)
			{
				return current;
			}
			return false;
		}
	}

	public bool Is => current;

	public bool Not => !current;

	public bool Done
	{
		get
		{
			if (prev)
			{
				return !current;
			}
			return false;
		}
	}

	public void Set(bool value)
	{
		prev = current;
		current = value;
	}
}
