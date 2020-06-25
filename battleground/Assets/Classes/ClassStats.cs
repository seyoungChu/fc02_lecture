using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClassStats : ScriptableObject
{	
	public List<Sheet> sheets = new List<Sheet> ();

	[System.SerializableAttribute]
	public class Sheet
	{
		public string name = string.Empty;
		public List<Param> list = new List<Param>();
	}

	[System.SerializableAttribute]
	public class Param
	{
		
		public string ID;
		public float[] AimOffset;
		public int ChangeCoverChance;
		public string WeaponType;
		public int BulletDamage;
		public float ShotRateFactor;
		public float ShotErrorRate;
		public string Effect_MuzzleFlash;
		public string Effect_Shot;
		public string Effect_Sparks;
		public string Effect_BulletHole;
	}
}

