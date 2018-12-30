using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGameDate : MonoBehaviour {

    public int tileMapSizeDate;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
