using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnMenuButton : MonoBehaviour
{
    public void ReturnMain()
    {
        SceneManager.LoadScene("CharacterSelect");
    }
}
