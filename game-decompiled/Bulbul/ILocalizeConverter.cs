using TMPro;

namespace Bulbul;

public interface ILocalizeConverter
{
	string Convert(string originalText);

	void Bind(TMP_Text text, string originalText);
}
