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
    public float maxHP;
    public float currHP;

    public int setTIleZ;
    public int setTIleX;

    private TileMapSetting tileMapSet;

    private bool isDes = false;

    private void Awake()
    {
        currHP = maxHP;
    }

    private void Start()
    {
        tileMapSet = GameObject.Find("TileFloor").GetComponent<TileMapSetting>();
    }

    public void Des()
    {
        if (currHP <= 0.0f)
        {
            isDes = true;
            tileMapSet.listTileGo[(setTIleZ + setTIleX * (tileMapSet.tileSizeXY / 2))].isBuilding = false;
            tileMapSet.listTileGo[(setTIleZ + setTIleX * (tileMapSet.tileSizeXY / 2))].listGoType = eBuildingType.NULL;

            Destroy(gameObject, 0.5f);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "SUBWALL" && isDes)
            Destroy(other.gameObject);
    }
}
