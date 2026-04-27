using Bulbul;
using VContainer;

public class UnlockItemService
{
	public enum ConditionsType
	{
		None,
		Scenario,
		PointPurchase
	}

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private UnlockMusic unlockMusic;

	[Inject]
	private UnlockConditionService _conditionService;

	private UnlockEnvironment _environment = new UnlockEnvironment();

	private UnlockDecoration _decoration = new UnlockDecoration();

	public UnlockEnvironment Environment => _environment;

	public UnlockDecoration Decoration => _decoration;

	public void Setup()
	{
		_environment.Setup(_masterDataLoader, _conditionService);
		_decoration.Setup(_masterDataLoader, _conditionService);
		unlockMusic.Setup(_masterDataLoader);
	}

	public void UnlockUpdate(ConditionsType conditionsType, string arg1, string arg2 = "0", string arg3 = "0")
	{
		_environment.UnlockUpdate(conditionsType, arg1, arg2, arg3);
		_decoration.UnlockUpdate(conditionsType, arg1, arg2, arg3);
		unlockMusic.UnlockUpdate(conditionsType, arg1, arg2, arg3);
	}

	public void UnlockUpdateByData()
	{
		_environment.UnlockUpdateByData();
		_decoration.UnlockUpdateByData();
	}
}
