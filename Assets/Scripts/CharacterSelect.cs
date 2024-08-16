using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class CharacterSelect : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject TextObj;
    
    public void OnPointerDown(PointerEventData eventData)
    {
        SceneManager.LoadScene(gameObject.name);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TextObj.gameObject.SetActive(true);
    }   

    public void OnPointerExit(PointerEventData eventData)
    {
        TextObj.gameObject.SetActive(false);
    }
}
