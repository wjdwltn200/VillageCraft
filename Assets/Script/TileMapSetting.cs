using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sTileInfo
{
    public GameObject listGo;
    public bool isBuilding = false;
    public bool isFarmland = false;
    public eBuildingType listGoType;
}

[System.Serializable]
public class TileMapDeco
{
    public GameObject[] decoList;
    public GameObject[] notBuilding;
    public GameObject farmGO;
}

public class TileMapSetting : MonoBehaviour {
    public int tileSizeXY;
    public GameObject tempTileGo;
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
                tempTileGo.transform.position = new Vector3(x - (tileSizeXY / 4), 0, z - (tileSizeXY / 4));
                sTile = new sTileInfo();
                sTile.listGo = Instantiate(tempTileGo, transform);
                listTileGo.Add(sTile);
            }
        }


    }

    // 타일 장식 설치
    public void tileDecoCreate()
    {
        for (int x = 3; x < tileSizeXY / 2 - 3; x++)
        {
            for (int z = 3; z < tileSizeXY / 2 - 3; z++)
            {
                if (Random.Range(0, 100) < 5)
                {
                    if (Random.Range(0, 100) < 5)
                    {
                        bool isFarm = false;
                        for (int farmZ = -1; farmZ < 2; farmZ++)
                        {
                            for (int farmX = -1; farmX < 2; farmX++)
                            {
                                if (listTileGo[(z + farmZ) + ((x + farmX) * tileSizeXY / 2)].isFarmland)
                                    isFarm = true;
                            }
                        }

                        if (!isFarm)
                        {
                            for (int farmZ = -1; farmZ < 2; farmZ++)
                            {
                                for (int farmX = -1; farmX < 2; farmX++)
                                {
                                    tempTileGo = listTileGo[(z + farmZ) + ((x + farmX) * tileSizeXY / 2)].listGo;
                                    GameObject tempObj = Instantiate(mapDeco.farmGO, tempTileGo.transform.position, tempTileGo.transform.rotation, transform);
                                    listTileGo[(z + farmZ) + ((x + farmX) * tileSizeXY / 2)].isFarmland = true;
                                }
                            }
                        }
                    }
                }
                else if (Random.Range(0, 100) < 10)
                {
                    if (listTileGo[(z) + ((x) * tileSizeXY / 2)].isFarmland ||
                        listTileGo[(z) + ((x) * tileSizeXY / 2)].isBuilding) continue;

                    GameObject tempGO;
                    tempTileGo = listTileGo[(z) + ((x) * tileSizeXY / 2)].listGo;

                    if (Random.Range(0, 100) <= 50)
                    {
                        tempGO = mapDeco.decoList[Random.Range(0, mapDeco.decoList.Length)];
                    }
                    else
                    {
                        tempGO = mapDeco.notBuilding[Random.Range(0, mapDeco.notBuilding.Length)];
                        listTileGo[(z) + ((x) * tileSizeXY / 2)].isBuilding = true;
                    }

                    tempGO.transform.Rotate(0, Random.Range(0.0f, 180.0f), 0, Space.World);
                    Instantiate(tempGO, tempTileGo.transform.position, tempGO.transform.rotation, transform);
                }
            }
        }

        Destroy(tempTileGo);
        tempTileGo = null;
    }
}
