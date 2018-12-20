using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class PlayerData : MonoBehaviour {

    public int currGold = 0;
    public Text txtGold;
	
	// Update is called once per frame
	void Update () {
        txtGold.text = currGold.ToString();
    }
}
