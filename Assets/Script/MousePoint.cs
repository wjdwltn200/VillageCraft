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
    private eBuildingType buildingType;
    public int selectBuildingValue;
    void Update()
    {
        selectBuilding();
        tileInfo();
    }

    private void Start()
    {
        tileList = TileFloorGO.GetComponent<TileMapSetting>().listTileGo;
        tileSizeXY = TileFloorGO.GetComponent<TileMapSetting>().tileSizeXY;
    }

    void selectBuilding()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) selectBuildingValue = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) selectBuildingValue = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) selectBuildingValue = 2;
        if (Input.GetKeyDown(KeyCode.Alpha4)) selectBuildingValue = 3;

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
                tempBuilding = GetComponent<BuildingSetting>().buildingGet(selectBuildingValue);
                tempBuilding.transform.position = tmepListGo.transform.position;

                isButton = true;
            }
        }

        if (Input.GetMouseButtonUp(0) && isButton)
        {
            int tempBuildSizeX = tempBuilding.GetComponent<BuildingData>().sizeX;
            int tempBuildSizeZ = tempBuilding.GetComponent<BuildingData>().sizeZ;
            tempBuilding.GetComponent<BuildingData>().setTIleZ = pointZ;
            tempBuilding.GetComponent<BuildingData>().setTIleX = pointX;

            buildingType = tempBuilding.GetComponent<BuildingData>().ebuildingType;

            if (isBuildingCheck(tempBuildSizeX, tempBuildSizeZ))
            {
                tmepListGo.transform.rotation = tempBuilding.transform.rotation;

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
        if (sizeX + sizeZ == 0)
        {
            tileList[(pointZ) + (pointX) * (tileSizeXY / 2)].isBuilding = true;
            tileList[(pointZ) + (pointX) * (tileSizeXY / 2)].listGoType = buildingType;
        }
        else
        {
            for (int x = -sizeX; x < sizeX + 1; x++)
            {
                for (int z = -sizeZ; z < sizeZ + 1; z++)
                {
                    tileList[(pointZ + z) + (pointX + x) * (tileSizeXY / 2)].isBuilding = true;
                    tileList[(pointZ + z) + (pointX + x) * (tileSizeXY / 2)].listGoType = buildingType;
                }
            }
        }

        switch (tileList[(pointZ) + (pointX) * (tileSizeXY / 2)].listGoType)
        {
            case eBuildingType.NULL:
                break;
            case eBuildingType.WALL:
                for (int z = -1; z < 2; z++)
                {
                    if (z == 0) continue;
                    if (tileList[(pointZ + z) + (pointX) * (tileSizeXY / 2)].listGoType == eBuildingType.WALL)
                    {
                        GameObject tempSubObj = Instantiate(tempBuilding.GetComponent<BuildingData>().subObject, transform.position, transform.rotation, transform);
                        tempSubObj.transform.position = tempBuilding.transform.position;
                        tempSubObj.transform.Rotate(-90.0f, 0.0f, 90.0f, Space.World);
                        tempSubObj.transform.Translate(0, 0, (float)z / 2, Space.World);
                    }
                }

                for (int x = -1; x < 2; x++)
                {
                    if (x == 0) continue;
                    if (tileList[(pointZ) + (pointX + x) * (tileSizeXY / 2)].listGoType == eBuildingType.WALL)
                    {
                        GameObject tempSubObj = Instantiate(tempBuilding.GetComponent<BuildingData>().subObject, transform.position, transform.rotation, transform);
                        tempSubObj.transform.position = tempBuilding.transform.position;
                        tempSubObj.transform.Rotate(-90.0f, -90.0f, 90.0f, Space.World);
                        tempSubObj.transform.Translate((float)x / 2, 0, 0, Space.World);
                    }
                }
                break;
            default:
                break;
        }

        return;
    }

    bool mapSizeCheck(int sizeX, int sizeZ)
    {
        if (sizeX <= -1 || sizeX >= tileSizeXY / 2) return false;
        if (sizeZ <= -1 || sizeZ >= tileSizeXY / 2) return false;

        return true;
    }
}
