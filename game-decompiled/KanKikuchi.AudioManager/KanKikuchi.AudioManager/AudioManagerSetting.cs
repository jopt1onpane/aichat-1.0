using UnityEngine;
using UnityEngine.Audio;

namespace KanKikuchi.AudioManager;

public class AudioManagerSetting : ScriptableObject
{
	[SerializeField]
	private bool _isOverwriteSettings;

	[SerializeField]
	private string _overwriteTopDirectoryPath;

	[SerializeField]
	private string _overwriteExportDirectoryPath;

	[SerializeField]
	private string _overwriteNamespace;

	[SerializeField]
	private AudioLoadType _loadType;

	[SerializeField]
	private AudioMixerGroup bgmGroup;

	[SerializeField]
	private AudioMixerGroup seGroup;

	[SerializeField]
	private AudioMixerGroup musicGroup;

	[SerializeField]
	private AudioMixerGroup voiceGroup;

	[SerializeField]
	private AudioMixerGroup ambientBGMGroup;

	[SerializeField]
	private AudioMixerGroup ambientSEGroup;

	[SerializeField]
	private bool _isAutoUpdateAudioPath = true;

	[SerializeField]
	private int _bgmAudioPlayerNum = 3;

	[SerializeField]
	private int _seAudioPlayerNum = 10;

	[SerializeField]
	private int _musicAudioPlayerNum = 3;

	[SerializeField]
	private int _voiceAudioPlayerNum = 3;

	[SerializeField]
	private int _ambientBgmAudioPlayerNum = 10;

	[SerializeField]
	private int _ambientSeAudioPlayerNum = 10;

	[SerializeField]
	private float _bgmBaseVolume = 1f;

	[SerializeField]
	private float _seBaseVolume = 1f;

	[SerializeField]
	private float _musicBaseVolume = 1f;

	[SerializeField]
	private float _voiceBaseVolume = 1f;

	[SerializeField]
	private float _ambientBgmBaseVolume = 1f;

	[SerializeField]
	private float _ambientSeBaseVolume = 1f;

	[SerializeField]
	private bool _shouldAdjustSEVolumeRate = true;

	[SerializeField]
	private bool _isDestroyBGMManager;

	[SerializeField]
	private bool _isDestroySEManager;

	[SerializeField]
	private AudioCacheType _bgmCacheType = AudioCacheType.All;

	[SerializeField]
	private AudioCacheType _seCacheType = AudioCacheType.All;

	[SerializeField]
	private bool _isReleaseBGMCache;

	[SerializeField]
	private bool _isReleaseSECache;

	[SerializeField]
	private bool _isAutoUpdateBGMSetting = true;

	[SerializeField]
	private bool _isAutoUpdateSESetting = true;

	[SerializeField]
	private bool _forceToMonoForBGM = true;

	[SerializeField]
	private bool _forceToMonoForSE = true;

	[SerializeField]
	private bool _normalizeForBGM = true;

	[SerializeField]
	private bool _normalizeForSE = true;

	[SerializeField]
	private bool _ambisonicForBGM;

	[SerializeField]
	private bool _ambisonicForSE;

	[SerializeField]
	private bool _loadInBackgroundForBGM;

	[SerializeField]
	private bool _loadInBackgroundForSE;

	[SerializeField]
	private AudioClipLoadType _loadTypeForBGM = AudioClipLoadType.Streaming;

	[SerializeField]
	private AudioClipLoadType _loadTypeForSE = AudioClipLoadType.CompressedInMemory;

	[SerializeField]
	private float _qualityForBGM = 0.3f;

	[SerializeField]
	private float _qualityForSE = 0.3f;

	[SerializeField]
	private AudioCompressionFormat _compressionFormatForBGM = AudioCompressionFormat.Vorbis;

	[SerializeField]
	private AudioCompressionFormat _compressionFormatForSE = AudioCompressionFormat.Vorbis;

	public static AudioManagerSetting Entity { get; private set; }

	public bool IsOverwriteSettings => _isOverwriteSettings;

	public string OverwriteTopDirectoryPath => _overwriteTopDirectoryPath;

	public string OverwriteExportDirectoryPath => _overwriteExportDirectoryPath;

	public string OverwriteNeamespace => _overwriteNamespace;

	public AudioLoadType LoadType => _loadType;

	public AudioMixerGroup BGMGroup => bgmGroup;

	public AudioMixerGroup SEGroup => seGroup;

	public AudioMixerGroup MusicGroup => musicGroup;

	public AudioMixerGroup VoiceGroup => voiceGroup;

	public AudioMixerGroup AmbientBGMGroup => ambientBGMGroup;

	public AudioMixerGroup AmbientSEGroup => ambientSEGroup;

	public bool IsAutoUpdateAudioPath => _isAutoUpdateAudioPath;

	public int BGMAudioPlayerNum => _bgmAudioPlayerNum;

	public int SEAudioPlayerNum => _seAudioPlayerNum;

	public int MusicAudioPlayerNum => _musicAudioPlayerNum;

	public int VoiceAudioPlayerNum => _voiceAudioPlayerNum;

	public int AmbientBgmAudioPlayerNum => _ambientBgmAudioPlayerNum;

	public int AmbientSeAudioPlayerNum => _ambientSeAudioPlayerNum;

	public float BGMBaseVolume => _bgmBaseVolume;

	public float SEBaseVolume => _seBaseVolume;

	public float MusicBaseVolume => _musicBaseVolume;

	public float VoiceBaseVolume => _voiceBaseVolume;

	public float AmbientBGMBaseVolume => _ambientBgmBaseVolume;

	public float AmbientSEBaseVolume => _ambientSeBaseVolume;

	public bool ShouldAdjustSeVolumeRate => _shouldAdjustSEVolumeRate;

	public bool IsDestroyBGMManager => _isDestroyBGMManager;

	public bool IsDestroySEManager => _isDestroySEManager;

	public AudioCacheType BGMCacheType => _bgmCacheType;

	public AudioCacheType SECacheType => _seCacheType;

	public bool IsReleaseBGMCache => _isReleaseBGMCache;

	public bool IsReleaseSECache => _isReleaseSECache;

	public bool IsAutoUpdateBGMSetting => _isAutoUpdateBGMSetting;

	public bool IsAutoUpdateSESetting => _isAutoUpdateSESetting;

	public bool ForceToMonoForBGM => _forceToMonoForBGM;

	public bool ForceToMonoForSE => _forceToMonoForSE;

	public bool NormalizeForBGM => _normalizeForBGM;

	public bool NormalizeForSE => _normalizeForSE;

	public bool AmbisonicForBGM => _ambisonicForBGM;

	public bool AmbisonicForSE => _ambisonicForSE;

	public bool LoadInBackgroundForBGM => _loadInBackgroundForBGM;

	public bool LoadInBackgroundForSE => _loadInBackgroundForSE;

	public AudioClipLoadType LoadTypeForBGM => _loadTypeForBGM;

	public AudioClipLoadType LoadTypeForSE => _loadTypeForSE;

	public float QualityForBGM => _qualityForBGM;

	public float QualityForSE => _qualityForSE;

	public AudioCompressionFormat CompressionFormatForBGM => _compressionFormatForBGM;

	public AudioCompressionFormat CompressionFormatForSE => _compressionFormatForSE;

	public void Initialize()
	{
		Entity = this;
	}
}
