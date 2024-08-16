using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class IncomingChat : MonoBehaviour
{
	public TextMeshProUGUI characterName;
	
	public TextMeshProUGUI message;

	public Coroutine TextCoroutine;
}
