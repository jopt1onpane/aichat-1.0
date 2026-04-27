using UnityEngine;

namespace Bulbul;

[CreateAssetMenu(fileName = "AmbientSeVolumeMaster", menuName = "ScriptableObject/AmbientSeVolumeMaster")]
public class AmbientSeVolumeMaster : ScriptableObject
{
	[SerializeField]
	[Header("キーボードタイピング")]
	[Range(0f, 1f)]
	private float KeyboardTypingVolume = 1f;

	[SerializeField]
	[Header("メカニカルキーボードタイピング")]
	[Range(0f, 1f)]
	private float MechanicalKeyboardTypingVolume = 1f;

	[SerializeField]
	[Header("パンタグラフキーボードタイピング")]
	[Range(0f, 1f)]
	private float PantographKeyboardTypingVolume = 1f;

	[SerializeField]
	[Header("キーボード移動")]
	[Range(0f, 1f)]
	private float KeyboardMoveVolume = 1f;

	[SerializeField]
	[Header("キーボードを置く")]
	[Range(0f, 1f)]
	private float KeyboardPutVolume = 1f;

	[SerializeField]
	[Header("椅子が軋む")]
	[Range(0f, 1f)]
	private float ChairCreakingVolume = 1f;

	[SerializeField]
	[Header("椅子を動かす")]
	[Range(0f, 1f)]
	private float ChairCasterVolume = 1f;

	[SerializeField]
	[Header("ソファに座る")]
	[Range(0f, 1f)]
	private float SitSofa_1_Volume = 1f;

	[SerializeField]
	[Header("ノート移動")]
	[Range(0f, 1f)]
	private float NoteMoveVolume = 1f;

	[SerializeField]
	[Header("ノートを置く")]
	[Range(0f, 1f)]
	private float NotePutVolume = 1f;

	[SerializeField]
	[Header("ペンで書く音")]
	[Range(0f, 1f)]
	private float PenWriteVolume = 1f;

	[SerializeField]
	[Header("ペンをノートに突き立てる音")]
	[Range(0f, 1f)]
	private float PenStickIntoNoteVolume = 1f;

	[SerializeField]
	[Header("ペンを落とす音")]
	[Range(0f, 1f)]
	private float PenDropVolume = 1f;

	[SerializeField]
	[Header("コップを机に置く音")]
	[Range(0f, 1f)]
	private float CupPlaceVolume = 1f;

	[SerializeField]
	[Header("コップをソーサラーに置く音")]
	[Range(0f, 1f)]
	private float CupPlaceOnSaucerVolume = 1f;

	[SerializeField]
	[Header("衣擦れ1")]
	[Range(0f, 1f)]
	private float RubbingClothes_1_Volume = 1f;

	[SerializeField]
	[Header("衣擦れ2")]
	[Range(0f, 1f)]
	private float RubbingClothes_2_Volume = 1f;

	[SerializeField]
	[Header("衣擦れ3")]
	[Range(0f, 1f)]
	private float RubbingClothes_3_Volume = 1f;

	[SerializeField]
	[Header("衣擦れ4")]
	[Range(0f, 1f)]
	private float RubbingClothes_4_Volume = 1f;

	[SerializeField]
	[Header("歩く音")]
	[Range(0f, 1f)]
	private float Walk_1_Volume = 1f;

	[SerializeField]
	[Header("教科書を捲る")]
	[Range(0f, 1f)]
	private float PageFlipSoundTextbookVolume = 1f;

	[SerializeField]
	[Header("小説を捲る")]
	[Range(0f, 1f)]
	private float PageFlipSoundNovelVolume = 1f;

	[SerializeField]
	[Header("手を叩く")]
	[Range(0f, 1f)]
	private float HandClapVolume = 1f;

	[SerializeField]
	[Header("窓を開ける")]
	[Range(0f, 1f)]
	private float OpenWindowVolume = 1f;

	[SerializeField]
	[Header("窓を閉める")]
	[Range(0f, 1f)]
	private float CloseWindowVolume = 1f;

	[SerializeField]
	[Header("コーヒーメーカー")]
	[Range(0f, 1f)]
	private float CoffeeMakerAllVolume = 1f;

	[SerializeField]
	[Header("キッチン\n水音")]
	[Range(0f, 1f)]
	private float KitchenWaterOnly_1_Volume = 1f;

	[SerializeField]
	[Header("コップを洗う")]
	[Range(0f, 1f)]
	private float KitchenWashingCup_1_Volume = 1f;

	[SerializeField]
	[Header("冷蔵庫を開ける")]
	[Range(0f, 1f)]
	private float FridgeDoorOpenVolume = 1f;

	[SerializeField]
	[Header("冷蔵庫から物を取る")]
	[Range(0f, 1f)]
	private float PlayTakeOutFromFridge_1Volume = 1f;

	[SerializeField]
	[Header("冷蔵庫を閉める")]
	[Range(0f, 1f)]
	private float FridgeDoorCloseVolume = 1f;

	[SerializeField]
	[Header("カップケーキ\nカップを潰す")]
	[Range(0f, 1f)]
	private float EatenCupcakeCrushCupVolume = 1f;

	[SerializeField]
	[Header("手を払う")]
	[Range(0f, 1f)]
	private float EatenCupcakeRubHandsVolume = 1f;

	public float GetVolume(AmbientSeType ambientSeType)
	{
		switch (ambientSeType)
		{
		case AmbientSeType.KeyboardTyping:
			return KeyboardTypingVolume;
		case AmbientSeType.MechanicalKeyboard_01:
		case AmbientSeType.MechanicalKeyboard_02:
		case AmbientSeType.MechanicalKeyboard_03:
			return MechanicalKeyboardTypingVolume;
		case AmbientSeType.PantographKeyboard_01:
		case AmbientSeType.PantographKeyboard_02:
		case AmbientSeType.PantographKeyboard_03:
			return PantographKeyboardTypingVolume;
		case AmbientSeType.MoveKeyboard:
			return KeyboardMoveVolume;
		case AmbientSeType.PutKeyboard:
			return KeyboardPutVolume;
		case AmbientSeType.ChairCreaking:
			return ChairCreakingVolume;
		case AmbientSeType.ChairCaster:
			return ChairCasterVolume;
		case AmbientSeType.SitSofa_1:
			return SitSofa_1_Volume;
		case AmbientSeType.MoveNote:
			return NoteMoveVolume;
		case AmbientSeType.PutNote:
			return NotePutVolume;
		case AmbientSeType.WritePen:
			return PenWriteVolume;
		case AmbientSeType.StickPenIntoNote:
			return PenStickIntoNoteVolume;
		case AmbientSeType.DropPen:
			return PenDropVolume;
		case AmbientSeType.PlaceCup:
			return CupPlaceVolume;
		case AmbientSeType.PlaceCupOnSaucer:
			return CupPlaceOnSaucerVolume;
		case AmbientSeType.RubbingClothes_1:
			return RubbingClothes_1_Volume;
		case AmbientSeType.RubbingClothes_2:
			return RubbingClothes_2_Volume;
		case AmbientSeType.RubbingClothes_3:
			return RubbingClothes_3_Volume;
		case AmbientSeType.RubbingClothes_4:
			return RubbingClothes_4_Volume;
		case AmbientSeType.Walk_1:
			return Walk_1_Volume;
		case AmbientSeType.PageFlipSoundTextbook:
			return PageFlipSoundTextbookVolume;
		case AmbientSeType.PageFlipSoundNovel:
			return PageFlipSoundNovelVolume;
		case AmbientSeType.HandClap:
			return HandClapVolume;
		case AmbientSeType.OpenWindow:
			return OpenWindowVolume;
		case AmbientSeType.CloseWindow:
			return CloseWindowVolume;
		case AmbientSeType.CoffeeMakerAll:
			return CoffeeMakerAllVolume;
		case AmbientSeType.KitchenWaterOnly_1:
			return KitchenWaterOnly_1_Volume;
		case AmbientSeType.KitchenWashingCup_1:
			return KitchenWashingCup_1_Volume;
		case AmbientSeType.FridgeDoorOpen:
			return FridgeDoorOpenVolume;
		case AmbientSeType.PlayTakeOutFromFridge_1:
			return PlayTakeOutFromFridge_1Volume;
		case AmbientSeType.FridgeDoorClose:
			return FridgeDoorCloseVolume;
		case AmbientSeType.EatenCupcakeCrushCup:
			return EatenCupcakeCrushCupVolume;
		case AmbientSeType.EatenCupcakeRubHands:
			return EatenCupcakeRubHandsVolume;
		default:
			Debug.LogError(string.Format("{0}:{1}の音量は登録されていない為、1.0fを返します", "AmbientSeType", ambientSeType));
			return 1f;
		}
	}
}
