using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePoint : MonoBehaviour {

    private List<sTileInfo> tileList;
    private int tileSizeXY;

    private BuildingCraftingUI CraftingUI;

    public GameObject TileFloorGO;
    public int clickLayer = 8;
    public int blockLayer = 9;
    public GameObject trueTileGO;
    public GameObject falseTileGO;
    public GameObject isBuildingTileMgr;
    public List<GameObject> isBuildingTile;

    private int tileMapSize;
    private int pointX;
    private int pointZ;
    private int tempBuildSizeX;
    private int tempBuildSizeZ;

    private GameObject tmepListGo;
    private GameObject tempBuilding;
    private BuildingData buildingData;
    private PlayerData playerData;

    Vector3 hitPosition;
    private bool isButton = false;
    private eBuildingType buildingType;
    public int selectBuildingValue;

    private Touch tempTouchs;
    private Vector3 touchedPos;
    private bool touchOn;

    private void Awake()
    {
        playerData = GameObject.Find("PlayerMgr").GetComponent<PlayerData>();
        CraftingUI = GameObject.Find("UIMgr").GetComponent<BuildingCraftingUI>();
        isBuildingTileMgr = GameObject.Find("isBuildingTileMgr");
        isBuildingTile = new List<GameObject>();
    }

    private void Start()
    {
        tileList = TileFloorGO.GetComponent<TileMapSetting>().listTileGo;
        tileSizeXY = TileFloorGO.GetComponent<TileMapSetting>().tileSizeXY;

    }

    void Update()
    {
        tileInfo();
        //touchSys();
    }

    void touchSys()
    {
        touchOn = false;

        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {

                tempTouchs = Input.GetTouch(i);
                if (tempTouchs.phase == TouchPhase.Began)
                {
                    touchedPos = Camera.main.ScreenToWorldPoint(tempTouchs.position);
                    touchOn = true;
                    playerData.currGold += (int)1000.0f;
                    break;
                }
            }
        }
    }

    void tileInfo()
    {
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(1)) return;

        if (Input.GetMouseButton(0) && tileList.Count > 0 && CraftingUI.isBuildingListSelect)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, 100.0f, 1 << 5)) Debug.Log("눌림");

            if (Physics.Raycast(ray, out hitInfo, 100.0f, 1 << 10))
            {
                isBuildingList();

                hitPosition = hitInfo.point;
                tileMapSize = tileSizeXY / 4 - 1;

                pointX = (((int)hitPosition.x)) + tileMapSize;
                pointZ = (((int)hitPosition.z)) + tileMapSize;
                if (hitPosition.z > 0.0f) pointZ++;
                if (hitPosition.x > 0.0f) pointX++;

                tmepListGo = tileList[(pointZ + pointX * (tileSizeXY / 2))].listGo;
                tempBuilding = GetComponent<BuildingSetting>().buildingGet(selectBuildingValue);
                buildingData = tempBuilding.GetComponent<BuildingData>();
                tempBuildSizeX = buildingData.sizeX;
                tempBuildSizeZ = buildingData.sizeZ;

                if (isBuildingCheck(tempBuildSizeX, tempBuildSizeZ, buildingData.ebuildingType)) { }
                tempBuilding.transform.position = tmepListGo.transform.position;
                isButton = true;
            }
        }

        if (Input.GetMouseButtonUp(0) && isButton)
        {
            if ((buildingData._buyPrice <= playerData.currGold))
            {
                playerData.currGold -= (int)buildingData._buyPrice;
                tempBuilding.GetComponent<BuildingData>().setTileXZ(pointX, pointZ);

                buildingType = tempBuilding.GetComponent<BuildingData>().ebuildingType;

                if (isBuildingCheck(tempBuildSizeX, tempBuildSizeZ, buildingData.ebuildingType))
                {
                    tmepListGo.transform.rotation = tempBuilding.transform.rotation;

                    GameObject tempBuildingInst = Instantiate(tempBuilding, tmepListGo.transform.position, tmepListGo.transform.rotation, transform);
                    tempBuildingInst.GetComponent<BuildingSys>().isOrigin = false;

                    setBuildingCheck(tempBuildSizeX, tempBuildSizeZ);
                }
            }
            else
            {
                Debug.Log("응 돈없어~");
            }

            isBuildingList();
            tempBuilding.SetActive(false);
            isButton = false;
        }
    }

    bool isBuildingCheck(int sizeX, int sizeZ, eBuildingType buildingType)
    {
        bool isTempChaeck = true;

        switch (buildingType)
        {
            case eBuildingType.WINDMILL:
                int tempFramPoint = 0;
                for (int x = -sizeX; x < sizeX + 1; x++)
                {
                    for (int z = -sizeZ; z < sizeZ + 1; z++)
                    {
                        if (!mapSizeCheck((pointX + x), (pointZ + z))) return false;

                        if (tileList[(pointZ + z) + (pointX + x) * (tileSizeXY / 2)].isBuilding)
                        {
                            isTempChaeck = false;
                            GameObject tempGO = Instantiate(falseTileGO, tileList[(pointZ + z) + (pointX + x) * (tileSizeXY / 2)].listGo.transform.position, falseTileGO.transform.rotation, isBuildingTileMgr.transform);
                            tempGO.transform.Translate(0, 0.01f, 0,Space.World);
                            isBuildingTile.Add(tempGO);
                        }
                        else
                        {
                            GameObject tempGO = Instantiate(trueTileGO, tileList[(pointZ + z) + (pointX + x) * (tileSizeXY / 2)].listGo.transform.position, falseTileGO.transform.rotation, isBuildingTileMgr.transform);
                            tempGO.transform.Translate(0, 0.01f, 0, Space.World);
                            isBuildingTile.Add(tempGO);
                            if (tileList[(pointZ + z) + (pointX + x) * (tileSizeXY / 2)].isFarmland) tempFramPoint++;
                        }
                    }
                }

                if (!isTempChaeck) return false;
                if (tempFramPoint < 3) return false;

                break;
            default:
                for (int x = -sizeX; x < sizeX + 1; x++)
                {
                    for (int z = -sizeZ; z < sizeZ + 1; z++)
                    {
                        if (!mapSizeCheck((pointX + x), (pointZ + z))) return false;

                        if (tileList[(pointZ + z) + (pointX + x) * (tileSizeXY / 2)].isBuilding)
                        {
                            isTempChaeck = false;
                            GameObject tempGO = Instantiate(falseTileGO, tileList[(pointZ + z) + (pointX + x) * (tileSizeXY / 2)].listGo.transform.position, falseTileGO.transform.rotation, isBuildingTileMgr.transform);
                            tempGO.transform.Translate(0, 0.01f, 0,Space.World);
                            isBuildingTile.Add(tempGO);
                        }
                        else
                        {
                            GameObject tempGO = Instantiate(trueTileGO, tileList[(pointZ + z) + (pointX + x) * (tileSizeXY / 2)].listGo.transform.position, falseTileGO.transform.rotation, isBuildingTileMgr.transform);
                            tempGO.transform.Translate(0, 0.01f, 0, Space.World);
                            isBuildingTile.Add(tempGO);
                        }
                    }
                }

                if (!isTempChaeck) return false;
                break;
        }
        return true;
    }

    void isBuildingList()
    {
        for (int i = 0; i < isBuildingTile.Count; i++)
        {
            Destroy(isBuildingTile[i]);
        }
        isBuildingTile.Clear();
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
