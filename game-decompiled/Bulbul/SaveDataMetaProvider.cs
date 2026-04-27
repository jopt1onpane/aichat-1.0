using System;

namespace Bulbul;

public static class SaveDataMetaProvider<T>
{
	public static readonly ISaveDataMetaGenerator Generator;

	static SaveDataMetaProvider()
	{
		Type typeFromHandle = typeof(T);
		ISaveDataMetaGenerator generator = ((typeFromHandle == typeof(SettingData) && DevicePlatform.Steam.IsMobile()) ? HashAndSuffixPlatformMetaGenerator<T>.Default : ((typeFromHandle == typeof(AccountData)) ? HashSaveDataJsonMetaGenerator<T>.Default : ((typeFromHandle == typeof(FilesSnapshot)) ? HashSaveDataJsonMetaGenerator<T>.Default : ((!(typeFromHandle == typeof(LocalMusicSetting))) ? ((ISaveDataMetaGenerator)HashSaveDataMetaGenerator<T>.Default) : ((ISaveDataMetaGenerator)HashDatMetaGenerator<T>.Default)))));
		Generator = generator;
	}
}
