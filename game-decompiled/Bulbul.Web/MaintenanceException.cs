namespace Bulbul.Web;

public class MaintenanceException : AppResetException
{
	public readonly string MainText;

	public MaintenanceException(string mainText)
		: base(ResetReason.Maintenance)
	{
		MainText = mainText;
	}
}
