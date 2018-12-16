using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class PlayerData : MonoBehaviour {

    public int currGold;
    public Text txtGold;

    private void Awake()
    {
        currGold = 0;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        txtGold.text = "currGold : " + currGold.ToString();
    }
}
