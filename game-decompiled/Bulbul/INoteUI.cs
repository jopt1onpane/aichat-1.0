namespace Bulbul;

public interface INoteUI
{
	void Setup();

	void UpdateUI();

	bool IsActive();

	void Activate();

	void Deactivate();

	void InitializePosition();
}
