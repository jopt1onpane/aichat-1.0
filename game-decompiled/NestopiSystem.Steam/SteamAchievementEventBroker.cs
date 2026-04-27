using R3;
using Steamworks;

namespace NestopiSystem.Steam;

public class SteamAchievementEventBroker
{
	private readonly Callback<UserStatsReceived_t> userStatsReceived;

	private readonly Callback<UserStatsStored_t> userStatsStored;

	private readonly Callback<UserAchievementStored_t> userAchievementStored;

	private readonly Subject<UserStatsReceived_t> onUserStatsReceived = new Subject<UserStatsReceived_t>();

	private readonly Subject<UserStatsStored_t> onUserStatsStored = new Subject<UserStatsStored_t>();

	private readonly Subject<UserAchievementStored_t> onAchievementStored = new Subject<UserAchievementStored_t>();

	private readonly SteamManager steamManager;

	private CGameID gameID;

	public Observable<UserStatsReceived_t> OnUserStatsReceived => onUserStatsReceived;

	public Observable<UserStatsStored_t> OnUserStatsStored => onUserStatsStored;

	public Observable<UserAchievementStored_t> OnAchievementStored => onAchievementStored;

	public SteamAchievementEventBroker(SteamManager steamManager)
	{
		if (!steamManager.IsInitialized)
		{
			throw new SteamAPIFailedInitializeException();
		}
		this.steamManager = steamManager;
		gameID = new CGameID(SteamUtils.GetAppID());
		userStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceivedCore);
		userStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStoredCore);
		userAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStoredCore);
	}

	private void OnUserStatsReceivedCore(UserStatsReceived_t args)
	{
		if (steamManager.IsInitialized && args.m_nGameID == gameID.m_GameID)
		{
			if (args.m_eResult != EResult.k_EResultOK)
			{
				onUserStatsReceived.OnErrorResume(new FailedOnUserStatsReceived($"Failed Receive stats and achievements from Steam. code is {args.m_eResult}\n", args.m_eResult));
			}
			else
			{
				onUserStatsReceived.OnNext(args);
			}
		}
	}

	private void OnUserStatsStoredCore(UserStatsStored_t args)
	{
		if (args.m_nGameID == gameID.m_GameID && args.m_eResult != EResult.k_EResultOK)
		{
			if (args.m_eResult == EResult.k_EResultInvalidParam)
			{
				onUserStatsReceived.OnErrorResume(new FailedOnUserStatsStoredCore("StoreStats - some failed to validate. code is " + args.m_eResult, args.m_eResult));
				UserStatsReceived_t args2 = new UserStatsReceived_t
				{
					m_eResult = EResult.k_EResultOK,
					m_nGameID = (ulong)gameID
				};
				OnUserStatsReceivedCore(args2);
			}
			else
			{
				onUserStatsReceived.OnErrorResume(new FailedOnUserStatsStoredCore("StoreStats - failed. code is " + args.m_eResult, args.m_eResult));
			}
		}
	}

	private void OnAchievementStoredCore(UserAchievementStored_t args)
	{
		if ((ulong)gameID == args.m_nGameID)
		{
			_ = args.m_nMaxProgress;
			onAchievementStored.OnNext(args);
		}
	}

	public void Dispose()
	{
		userStatsReceived?.Dispose();
		userStatsStored?.Dispose();
		userAchievementStored?.Dispose();
		onUserStatsReceived?.Dispose();
		onUserStatsStored?.Dispose();
		onAchievementStored?.Dispose();
	}
}
