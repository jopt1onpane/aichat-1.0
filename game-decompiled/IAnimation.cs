internal interface IAnimation
{
	void Init()
	{
	}

	bool IsComplete()
	{
		return false;
	}

	void Play(MyTweenPlayType playType);
}
