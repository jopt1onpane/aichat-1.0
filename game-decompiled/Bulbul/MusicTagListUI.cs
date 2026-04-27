using System.Collections.Generic;
using System.Linq;
using FastEnumUtility;
using NestopiSystem.DIContainers;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class MusicTagListUI : MonoBehaviour
{
	[SerializeField]
	private MusicTagListButton[] buttons;

	[SerializeField]
	private PulldownListUI _pulldown;

	private MusicService musicService;

	private LocalizationMasterWrapper _localizationMaster;

	private LanguageSupplier _languageSupplier;

	private static readonly Dictionary<AudioTag, string> AudioTagLocalizationMap = new Dictionary<AudioTag, string>
	{
		{
			AudioTag.All,
			"ui_music_tag_all"
		},
		{
			AudioTag.Original,
			"ui_music_tag_original"
		},
		{
			AudioTag.Special,
			"ui_music_tag_special"
		},
		{
			AudioTag.Other,
			"ui_music_tag_other"
		},
		{
			AudioTag.Local,
			"ui_music_tag_local"
		},
		{
			AudioTag.Favorite,
			"ui_music_tag_favorite"
		}
	};

	[Inject]
	public void Construct(MusicService musicService)
	{
		this.musicService = musicService;
	}

	public void Setup()
	{
		_localizationMaster = ProjectLifetimeScope.Resolve<LocalizationMasterWrapper>();
		_languageSupplier = ProjectLifetimeScope.Resolve<LanguageSupplier>();
		_pulldown.Setup();
		buttons.Select((MusicTagListButton b) => b.OnClick).Merge().Subscribe(delegate(MusicTagListButton button)
		{
			AudioTag flag = button.Tag;
			AudioTag value = SaveDataManager.Instance.MusicSetting.CurrentAudioTag.Value;
			bool flag2 = value.HasFlagFast(flag);
			if (flag2)
			{
				SaveDataManager.Instance.MusicSetting.CurrentAudioTag.Value = value.RemoveFlag(flag);
			}
			else
			{
				SaveDataManager.Instance.MusicSetting.CurrentAudioTag.Value = value.AddFlag(flag);
			}
			button.SetCheck(!flag2);
			SetTitle();
			SaveDataManager.Instance.SaveMusicSetting();
		})
			.AddTo(this);
		ButtonsSetCheck(SaveDataManager.Instance.MusicSetting.CurrentAudioTag.Value);
		ObservableSubscribeExtensions.Subscribe(_languageSupplier.Language, delegate
		{
			SetTitle();
		}).AddTo(this);
	}

	public void ButtonsSetCheck(AudioTag? setTag)
	{
		AudioTag value = ((!setTag.HasValue) ? SaveDataManager.Instance.MusicSetting.CurrentAudioTag.Value : setTag.Value);
		MusicTagListButton[] array = buttons;
		foreach (MusicTagListButton musicTagListButton in array)
		{
			musicTagListButton.SetCheck(value.HasFlagFast(musicTagListButton.Tag));
		}
		SetTitle(setTag);
	}

	private void SetTitle(AudioTag? setTag = null)
	{
		string text = string.Empty;
		AudioTag currentTag = ((!setTag.HasValue) ? SaveDataManager.Instance.MusicSetting.CurrentAudioTag.Value : setTag.Value);
		bool flag = currentTag.HasFlagFast(AudioTag.Favorite);
		AudioTag tagsWithoutFavorite = currentTag.RemoveFlag(AudioTag.Favorite);
		if (currentTag == AudioTag.Favorite)
		{
			text = GetLocalizeText(AudioTagLocalizationMap[AudioTag.Favorite]);
		}
		else if (flag && tagsWithoutFavorite == AudioTag.All.RemoveFlag(AudioTag.Favorite))
		{
			text = GetLocalizeText(AudioTagLocalizationMap[AudioTag.All]);
		}
		else if (flag)
		{
			List<AudioTag> list = (from t in FastEnum.GetValues<AudioTag>()
				where t != AudioTag.All && t != AudioTag.Favorite
				where tagsWithoutFavorite.HasFlagFast(t)
				select t).ToList();
			if (list.Count == 1)
			{
				string localizeText = GetLocalizeText(AudioTagLocalizationMap[list.First()]);
				string localizeText2 = GetLocalizeText(AudioTagLocalizationMap[AudioTag.Favorite]);
				text = localizeText + " & " + localizeText2;
			}
			else if (list.Count > 1)
			{
				string[] value = (from tag in list
					where AudioTagLocalizationMap.ContainsKey(tag)
					select AudioTagLocalizationMap[tag] into id
					select GetLocalizeText(id)).ToArray();
				string text2 = string.Join(" & ", value);
				string localizeText3 = GetLocalizeText(AudioTagLocalizationMap[AudioTag.Favorite]);
				text = text2 + " & " + localizeText3;
			}
		}
		else if (tagsWithoutFavorite == AudioTag.All.RemoveFlag(AudioTag.Favorite))
		{
			text = GetLocalizeText(AudioTagLocalizationMap[AudioTag.All]);
		}
		else
		{
			List<AudioTag> list2 = (from t in FastEnum.GetValues<AudioTag>()
				where t != AudioTag.All && t != AudioTag.Favorite
				where currentTag.HasFlagFast(t)
				select t).ToList();
			if (list2.Count == 0)
			{
				text = GetLocalizeText("ui_music_tag_select_tag");
			}
			else if (list2.Count == 1)
			{
				text = GetLocalizeText(AudioTagLocalizationMap[list2.First()]);
			}
			else
			{
				string[] value2 = (from tag in list2
					where AudioTagLocalizationMap.ContainsKey(tag)
					select AudioTagLocalizationMap[tag] into id
					select GetLocalizeText(id)).ToArray();
				text = string.Join(" & ", value2);
			}
		}
		_pulldown.ChangeSelectContentText(text);
	}

	public void ToggleMusicTagList()
	{
		_pulldown.TogglePullDown();
	}

	public void OpenMusicTagList()
	{
		_pulldown.OpenPullDown();
	}

	public void CloseMusicTagList(bool immediate = false)
	{
		_pulldown.ClosePullDown(immediate);
	}

	public string GetLocalizeText(string localizationId)
	{
		string result = string.Empty;
		if (_localizationMaster.TryGet(localizationId, out var result2))
		{
			result = result2;
		}
		else
		{
			Debug.LogError("プレイリストのローカライズタグ" + result2 + "が見つかりませんでした。");
		}
		return result;
	}
}
