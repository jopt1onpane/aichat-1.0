using R3;

namespace Bulbul.Web;

public class WebApiGate
{
	public readonly ReactiveProperty<bool> LoginGate = new ReactiveProperty<bool>(value: true);

	public readonly ReactiveProperty<bool> AccountGate = new ReactiveProperty<bool>(value: true);

	public readonly ReactiveProperty<bool> ResetGate = new ReactiveProperty<bool>(value: true);

	public bool AnyClosed()
	{
		if (LoginGate.Value && AccountGate.Value)
		{
			return !ResetGate.Value;
		}
		return true;
	}
}
