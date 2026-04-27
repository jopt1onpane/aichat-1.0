using System.Collections.Generic;

namespace Bulbul;

public static class AccountUtil
{
	public static void UpdateAuthButtonState(AuthButton appleButton, AuthButton googleButton)
	{
		InMemoryData.TryGetData<List<AccountType>>(out var data);
		if (data == null || data.Count == 0)
		{
			appleButton.SetState(isLinked: false);
			googleButton.SetState(isLinked: false);
		}
		else
		{
			appleButton.SetState(data.Contains(AccountType.Apple));
			googleButton.SetState(data.Contains(AccountType.Google));
		}
	}
}
