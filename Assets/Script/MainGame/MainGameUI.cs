using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainGameUI : MonoBehaviour {

    private void Start()
    {
        inGameChange();
    }

    public void inGameChange ()
    {
        SceneManager.LoadScene("InGame");
    }
}
