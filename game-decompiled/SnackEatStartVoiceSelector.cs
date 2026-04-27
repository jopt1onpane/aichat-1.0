public class SnackEatStartVoiceSelector
{
	private RandomList _voiceList = new RandomList();

	public void Setup()
	{
		_voiceList.Setup(1, 2);
	}

	public int TakeNextVoice()
	{
		int next = _voiceList.GetNext();
		_voiceList.UseNext();
		return next;
	}
}
