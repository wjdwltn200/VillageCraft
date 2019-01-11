using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileDecoSys : MonoBehaviour {
    private TileMapSetting tileMapSet;

    public int setTileX;
    public int setTileZ;

    private void Start()
    {
        tileMapSet = GameObject.Find("TileFloor").GetComponent<TileMapSetting>();
    }

    public void setTile(int x, int z)
    {
        setTileX = x;
        setTileZ = z;
    }

    public void OnDisable()
    {
        for (int x = -1; x < 1 + 1; x++)
        {
            for (int z = -1; z < 1 + 1; z++)
            {
                tileMapSet.listTileGo[(setTileZ + z) + (setTileX + x) * (tileMapSet.tileSizeXY / 2)].isBuilding = false;
                tileMapSet.listTileGo[(setTileZ + z) + (setTileX + x) * (tileMapSet.tileSizeXY / 2)].listGoType = eBuildingType.NULL;
            }
        }
    }
}
