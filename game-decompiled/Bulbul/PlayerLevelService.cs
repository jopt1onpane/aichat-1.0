using R3;

namespace Bulbul;

public class PlayerLevelService
{
	private bool _isCurrentLevelUpDirection;

	private Subject<float> _onAddExp = new Subject<float>();

	private Subject<Unit> _onAddedExp = new Subject<Unit>();

	public bool IsCurrentLevelUpDirection => _isCurrentLevelUpDirection;

	public Observable<float> OnAddExp => _onAddExp;

	public Observable<Unit> OnAddedExp => _onAddedExp;

	public void AddExp(float exp)
	{
		_isCurrentLevelUpDirection = true;
		_onAddExp.OnNext(exp);
	}

	public void AddedExp()
	{
		_onAddedExp.OnNext(Unit.Default);
	}

	public void OnFinishLevelUpDirection()
	{
		_isCurrentLevelUpDirection = false;
	}
}
