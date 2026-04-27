using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class HeroineCupcake : MonoBehaviour
{
	public enum CupcakeAnimationType
	{
		Idle = 0,
		Comeback = 100,
		Show_Loop = 101,
		Eat_Start_1 = 102,
		Eat_Loop_1 = 103,
		Eat_Start_2 = 104,
		Eat_Loop_2 = 105,
		Eat_End = 106
	}

	public enum CupcakeKind
	{
		None,
		A,
		B,
		C,
		D,
		E,
		F,
		G,
		H,
		I
	}

	public enum CupcakeRarity
	{
		Normal,
		Rare
	}

	private const string MotionType = "MotionType";

	private const string CupcakeIdleAnimationName = "Cupcake_Idle";

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[SerializeField]
	[Header("1口目のオブジェクト")]
	private GameObject _cake1Object;

	[SerializeField]
	[Header("2口目のオブジェクト")]
	private GameObject _cake2Object;

	[SerializeField]
	[Header("カップオブジェクト")]
	private GameObject _cupObject;

	[SerializeField]
	[Header("種類変更用\nマスタ")]
	private CupcakeKindMaster _cupcakeKindMaster;

	[SerializeField]
	[Header("カップのメッシュ")]
	private MeshFilter _bakingCupMesh;

	[SerializeField]
	[Header("カップのRenderer")]
	private Renderer _bakingCupRenderer;

	[SerializeField]
	[Header("ケーキ01 メッシュ")]
	private MeshFilter _cake01Mesh;

	[SerializeField]
	[Header("ケーキ01 Renderer")]
	private Renderer _cake01Renderer;

	[SerializeField]
	[Header("ケーキ02 メッシュ")]
	private MeshFilter _cake02Mesh;

	[SerializeField]
	[Header("ケーキ02 Renderer")]
	private Renderer _cake02Renderer;

	private List<CupcakeKind> _normalCupcakeList = new List<CupcakeKind>();

	private List<CupcakeKind> _rareCupcakeList = new List<CupcakeKind>();

	private int _currentMotionType;

	public void Setup()
	{
		foreach (CupcakeKind value in Enum.GetValues(typeof(CupcakeKind)))
		{
			CupcakeKindRecord cupcakeKindRecord = _cupcakeKindMaster.GetCupcakeKindRecord(value);
			if (cupcakeKindRecord != null)
			{
				switch (cupcakeKindRecord.Rarity)
				{
				case CupcakeRarity.Normal:
					_normalCupcakeList.Add(value);
					break;
				case CupcakeRarity.Rare:
					_rareCupcakeList.Add(value);
					break;
				}
			}
		}
	}

	public void StoryReady()
	{
		DeactivateCake1();
		DeactivateCake2();
		DeactivateCup();
	}

	public void TakeCupcake()
	{
		ChangeKind();
		Activate();
	}

	private void Activate()
	{
		_cake1Object.gameObject.SetActive(value: true);
		_cupObject.gameObject.SetActive(value: true);
		_cake2Object.gameObject.SetActive(value: false);
	}

	public void ChangeKind()
	{
		float cupcakeRareProbability = _masterDataLoader.HeroineAIMasterData.LeaveChairData.CupcakeRareProbability;
		List<CupcakeKind> list = ((!(UnityEngine.Random.Range(0f, 100f) <= cupcakeRareProbability)) ? _normalCupcakeList : _rareCupcakeList);
		int index = UnityEngine.Random.Range(0, list.Count);
		ApplyKind(list[index]);
	}

	private void ApplyKind(CupcakeKind kind)
	{
		CupcakeKindRecord cupcakeKindRecord = _cupcakeKindMaster.GetCupcakeKindRecord(kind);
		_bakingCupMesh.sharedMesh = cupcakeKindRecord.BakingCupMesh;
		_bakingCupRenderer.sharedMaterial = cupcakeKindRecord.BakingCupMaterial;
		_cake01Mesh.sharedMesh = cupcakeKindRecord.Cake01Mesh;
		_cake01Renderer.sharedMaterial = cupcakeKindRecord.Cake01Material;
		_cake02Mesh.sharedMesh = cupcakeKindRecord.Cake02Mesh;
		_cake02Renderer.sharedMaterial = cupcakeKindRecord.Cake02Material;
	}

	public void EatCake1()
	{
		DeactivateCake1();
		ActivateCake2();
	}

	private void DeactivateCake1()
	{
		_cake1Object.gameObject.SetActive(value: false);
	}

	public void EatCake2()
	{
		DeactivateCake2();
	}

	private void ActivateCake2()
	{
		_cake2Object.gameObject.SetActive(value: true);
	}

	private void DeactivateCake2()
	{
		_cake2Object.gameObject.SetActive(value: false);
	}

	public void CrashCup()
	{
		DeactivateCup();
	}

	private void DeactivateCup()
	{
		_cupObject.gameObject.SetActive(value: false);
	}
}
