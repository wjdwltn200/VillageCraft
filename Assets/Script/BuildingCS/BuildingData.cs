using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eBuildingType
{
    NULL,
    WALL,
    FARM,
    WINDMILL,
    WELL
}

public class BuildingData : MonoBehaviour {

    public int sizeX;
    public int sizeZ;
    public eBuildingType ebuildingType;
    public GameObject subObject;
}
