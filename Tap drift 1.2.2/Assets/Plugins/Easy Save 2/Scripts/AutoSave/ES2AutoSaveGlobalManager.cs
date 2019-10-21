#if !UNITY_4
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class ES2AutoSaveGlobalManager : MonoBehaviour
{
	public ES2AutoSave[] prefabArray;
	public HashSet<string> ids = new HashSet<string>();
}
#endif