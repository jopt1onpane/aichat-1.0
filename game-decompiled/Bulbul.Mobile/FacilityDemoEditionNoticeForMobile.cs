using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul.Mobile;

public class FacilityDemoEditionNoticeForMobile : MonoBehaviour
{
	[Inject]
	private PomodoroService _pomodoroService;

	[Inject]
	private IPlayerLevelUIService _playerLevelUIService;

	[Inject]
	private SystemSeService _systemSeService;

	[SerializeField]
	[Header("押すとレベルの制限を通知する")]
	private Button _playerLvButton;

	[SerializeField]
	private SimpleNoticeDialog _dialog;

	private bool _isWaitingLvUp;

	public void Setup()
	{
	}
}
