using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePoint : MonoBehaviour {

    private List<sTileInfo> tileList;
    private int tileSizeXY;

    public GameObject TileFloorGO;
    public int clickLayer = 8;
    public int blockLayer = 9;

    private int tileMapSize;
    private int pointX;
    private int pointZ;
    private GameObject tmepListGo;
    private GameObject tempBuilding;

    Vector3 hitPosition;
    private bool isButton = false;

    void Update()
    {
        tileInfo();
    }

    private void Start()
    {
        tileList = TileFloorGO.GetComponent<TileMapSetting>().listTileGo;
        tileSizeXY = TileFloorGO.GetComponent<TileMapSetting>().tileSizeXY;
    }

    void tileInfo()
    {
        if (Input.GetMouseButton(0) && tileList.Count > 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, 100.0f, 1 << 10))
            {
                hitPosition = hitInfo.point;
                tileMapSize = tileSizeXY / 4 - 1;

                pointX = (((int)hitPosition.x)) + tileMapSize;
                pointZ = (((int)hitPosition.z)) + tileMapSize;
                if (hitPosition.z > 0.0f) pointZ++;
                if (hitPosition.x > 0.0f) pointX++;

                tmepListGo = tileList[(pointZ + pointX * (tileSizeXY / 2))].listGo;
                tempBuilding = GetComponent<BuildingSetting>().buildingGet(4);
                tempBuilding.transform.position = tmepListGo.transform.position;
                isButton = true;
            }
        }

        if (Input.GetMouseButtonUp(0) && isButton)
        {
            int tempBuildSizeX = tempBuilding.GetComponent<BuildingData>().sizeX;
            int tempBuildSizeZ = tempBuilding.GetComponent<BuildingData>().sizeY;

            if (isBuildingCheck(tempBuildSizeX, tempBuildSizeZ))
            {
                Instantiate(tempBuilding, tmepListGo.transform.position, tmepListGo.transform.rotation, transform);
                setBuildingCheck(tempBuildSizeX, tempBuildSizeZ);
            }
            tempBuilding.SetActive(false);
            isButton = false;
        }
    }

    bool isBuildingCheck(int sizeX, int sizeZ)
    {
        for (int x = -sizeX / 2; x < sizeX / 2 + 1; x++)
        {
            for (int z = -sizeZ / 2; z < sizeX / 2 + 1; z++)
            {
                if (!mapSizeCheck((pointX + x), (pointZ + z))) return false;

                if (tileList[(pointZ + z) + (pointX + x) * (tileSizeXY / 2)].isBuilding)
                    return false;
            }
        }
        return true;
    }

    void setBuildingCheck(int sizeX, int sizeZ)
    {
        for (int x = -sizeX / 2; x < sizeX / 2 + 1; x++)
        {
            for (int z = -sizeZ / 2; z < sizeX / 2 + 1; z++)
            {
                if (!mapSizeCheck((pointX + x), (pointZ + z))) continue;

                tileList[(pointZ + z) + (pointX + x) * (tileSizeXY / 2)].isBuilding = true;
            }
        }
    }

    bool mapSizeCheck(int sizeX, int sizeZ)
    {
        Debug.Log("sizeX : " + sizeX);
        Debug.Log("sizeZ : " + sizeZ);

        if (sizeX <= -1 || sizeX >= tileSizeXY / 2) return false;
        if (sizeZ <= -1 || sizeZ >= tileSizeXY / 2) return false;

        return true;
    }
}
