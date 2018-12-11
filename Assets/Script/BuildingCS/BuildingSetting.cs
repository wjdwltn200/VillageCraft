using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSetting : MonoBehaviour {
    public GameObject[] buildingList;

    public GameObject buildingGet (int build)
    {
        buildingList[build].SetActive(true);

        return buildingList[build];
    }
}
