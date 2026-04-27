namespace Bulbul;

public interface ISettingUI
{
	void Setup();

	bool IsActive();

	void Activate();

	void Deactivate();
}
