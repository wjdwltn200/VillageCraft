using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class MousePoint : MonoBehaviour {

    private List<sTileInfo> tileList;
    private int tileSizeXY;

    private BuildingCraftingUI CraftingUI;

    public GameObject _centerBase;

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

    private float screenWidth = Screen.width;
    private float screenHeight = Screen.height;
    public UnityEngine.UI.Text Wid;
    public UnityEngine.UI.Text Hei;

    private bool isBase;

    private CameraCtrl camGO;
    private Vector2?[] touchPrevPos = { null, null };
    private Vector2 touchPrevVector;
    private float touchPrevDist;

    public AudioSource buildingSound;
    public AudioClip buildingSoundClip;

    private void Awake()
    {
        isBase = false;

        camGO = GameObject.Find("CamObj").GetComponent<CameraCtrl>();
        playerData = GameObject.Find("PlayerMgr").GetComponent<PlayerData>();
        CraftingUI = GameObject.Find("UIMgr").GetComponent<BuildingCraftingUI>();
        isBuildingTileMgr = GameObject.Find("isBuildingTileMgr");
        isBuildingTile = new List<GameObject>();

        buildingSound = this.gameObject.AddComponent<AudioSource>();
        buildingSound.clip = buildingSoundClip;
        buildingSound.volume = 0.6f;
    }

    private void Start()
    {
        Wid.text = screenWidth.ToString();
        Hei.text = screenHeight.ToString();

        tileList = TileFloorGO.GetComponent<TileMapSetting>().listTileGo;
        tileSizeXY = TileFloorGO.GetComponent<TileMapSetting>().tileSizeXY;
    }

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject(0) == true) return;

        if (!isBase && (tileList.Count > 0)) firstBase();
        touchSys();

    }

    private void LateUpdate()
    {
        if (Input.touchCount == 0)
        {
            touchPrevPos[0] = null;
            touchPrevPos[1] = null;
        }
    }

    public void firstBase()
    {
        // 최초 생성
        tileMapSize = tileSizeXY / 4 - 1;

        pointX = 0 + tileMapSize;
        pointZ = 0 + tileMapSize;

        tmepListGo = tileList[(pointZ + pointX * (tileSizeXY / 2))].listGo;
        _centerBase.SetActive(true);

        tempBuilding = _centerBase;
        buildingData = tempBuilding.GetComponent<BuildingData>();
        tempBuildSizeX = buildingData.sizeX;
        tempBuildSizeZ = buildingData.sizeZ;
        tempBuilding.transform.position = tmepListGo.transform.position;

        if (isBuildingCheck(tempBuildSizeX, tempBuildSizeZ, buildingData.ebuildingType))
        {
            tempBuilding.GetComponent<BuildingData>().setTileXZ(pointX, pointZ);
            buildingType = tempBuilding.GetComponent<BuildingData>().ebuildingType;

            playerData.currGold -= (int)buildingData._buyPrice;
            tmepListGo.transform.rotation = tempBuilding.transform.rotation;

            GameObject tempBuildingInst = Instantiate(tempBuilding, tmepListGo.transform.position, tmepListGo.transform.rotation, transform);
            _centerBase.SetActive(false);

            tempBuildingInst.GetComponent<BuildingSys>().isOrigin = false;

            setBuildingCheck(tempBuildSizeX, tempBuildSizeZ);
        }
        else
        {
            Debug.Log("무언가 있습니다.");
            isBase = true;
        }

        isBase = true;
    }

    void touchSys()
    {
        if (Input.touchCount == 0) return;

        tempTouchs = Input.GetTouch(0);
        if (tempTouchs.phase == TouchPhase.Began || tempTouchs.phase == TouchPhase.Moved)
        {
            touchedPos = Camera.main.ScreenToWorldPoint(tempTouchs.position);
            touchOn = true;
            tileInfo(touchedPos);
        }

        if (!CraftingUI.isBuildingListSelect && tempTouchs.phase == TouchPhase.Moved)
        {
            touchedPos = Camera.main.ScreenToWorldPoint(tempTouchs.position);
            //screenMove(touchedPos);
        }

        if (tempTouchs.phase == TouchPhase.Ended)
        {
            touchOn = false;
            tileInfo(touchedPos);
        }
    }

    void screenTouchsMove()
    {
        Vector2 touchNewPos = Input.GetTouch(0).position;
        camGO.transform.position += camGO.transform.TransformDirection((Vector3)((touchPrevPos[0] - touchNewPos) * Camera.main.orthographicSize / Camera.main.pixelHeight * 2f));
        
        

        //MoveLimit();
        touchPrevPos[0] = touchNewPos;
    }

    void MoveLimit()
    {
        Vector3 temp;
        temp.x = Mathf.Clamp(transform.position.x, 10, 10);
        temp.y = Mathf.Clamp(transform.position.y, 0, 10);
        temp.z = Mathf.Clamp(transform.position.z, 10, 12);

        camGO.transform.position = temp;
    }

    void screenMove(Vector3 touchPos)
    {

        Vector3 screenPoint = Camera.main.WorldToScreenPoint(touchPos);

        if (screenPoint.x > (screenWidth - screenWidth / 3.0f) && screenPoint.x < screenWidth)
        {
            camGO.moveToCam(0);
        }
        else if (screenPoint.x > 0.0f && screenPoint.x < screenWidth / 3.0f)
        {
            camGO.moveToCam(1);
        }

        if (screenPoint.y > (screenHeight- screenHeight / 3.0f) && screenPoint.y < screenHeight)
        {
            camGO.moveToCam(2);
        }
        else if (screenPoint.y > 0.0f && screenPoint.y < screenHeight / 3.0f)
        {
            camGO.moveToCam(3);
        }
    }

    void tileInfo(Vector3 touchPos)
    {
        if (!CraftingUI.isBuildingListSelect) return;

        if (touchOn &&
            tileList.Count > 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (touchOn)
                ray = Camera.main.ScreenPointToRay(tempTouchs.position);

            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 100.0f, 1 << 0))
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

        if (!touchOn && isButton)
        {
            if ((buildingData._buyPrice <= playerData.currGold))
            {
                if (isBuildingCheck(tempBuildSizeX, tempBuildSizeZ, buildingData.ebuildingType))
                {
                    buildingSound.Play();
                    tempBuilding.GetComponent<BuildingData>().setTileXZ(pointX, pointZ);
                    buildingType = tempBuilding.GetComponent<BuildingData>().ebuildingType;

                    playerData.currGold -= (int)buildingData._buyPrice;
                    tmepListGo.transform.rotation = tempBuilding.transform.rotation;

                    GameObject tempBuildingInst = Instantiate(tempBuilding, tmepListGo.transform.position, tmepListGo.transform.rotation, transform);
                    tempBuildingInst.GetComponent<BuildingSys>().isOrigin = false;
                    tempBuildingInst.GetComponent<BuildingData>().setTileXZ(pointX, pointZ);

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
        return;
    }

    bool mapSizeCheck(int sizeX, int sizeZ)
    {
        if (sizeX <= -1 || sizeX >= tileSizeXY / 2) return false;
        if (sizeZ <= -1 || sizeZ >= tileSizeXY / 2) return false;

        return true;
    }
}
