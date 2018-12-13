using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sTileInfo
{
    public GameObject listGo;
    public bool isBuilding = false;
    public eBuildingType listGoType;
}

[System.Serializable]
public class TileMapDeco
{
    public GameObject[] decoList;
}

public class TileMapSetting : MonoBehaviour {
    public int tileSizeXY;
    public GameObject tileGo;
    public List<sTileInfo> listTileGo;
    public sTileInfo sTile;
    public TileMapDeco mapDeco;

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

                // 타일 장식 설치
                if (Random.Range(0, 100) < 5)
                {
                    GameObject tempGO = mapDeco.decoList[Random.Range(0, mapDeco.decoList.Length)];
                    tempGO.transform.Rotate(0, Random.Range(0.0f, 180.0f), 0, Space.World);
                    Instantiate(tempGO, tileGo.transform.position, tempGO.transform.rotation, transform);
                }
            }
        }

        Destroy(tileGo);
        tileGo = null;
    }
}
