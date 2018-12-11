using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sTileInfo
{
    public GameObject listGo;
    public bool isBuilding = false;
}


public class TileMapSetting : MonoBehaviour {
    public int tileSizeXY;
    public GameObject tileGo;
    public List<sTileInfo> listTileGo;
    public sTileInfo sTile;

    private void Awake()
    {
        listTileGo = new List<sTileInfo>();
    }

    public void tileCreate()
    {
        for (int x = 0; x < tileSizeXY / 2; x++)
        {
            for (int z = 0; z < tileSizeXY / 2; z++)
            {
                tileGo.transform.position = new Vector3(x - (tileSizeXY / 4), 0, z - (tileSizeXY / 4));

                sTile = new sTileInfo();
                sTile.listGo = Instantiate(tileGo, transform);

                listTileGo.Add(sTile);
            }
        }

        Destroy(tileGo);
        tileGo = null;
    }
}
