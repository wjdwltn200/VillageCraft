using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eBuildingType
{
    NULL,
    WALL,
    FARM,
    WINDMILL,
    WELL,
    TOWER
}

public class BuildingData : MonoBehaviour {

    public int sizeX;
    public int sizeZ;
    public eBuildingType ebuildingType;
    public GameObject subObject;
    public GameObject mGO;

    private int setTIleZ;
    private int setTIleX;

    private TileMapSetting tileMapSet;
    public float _setTimer;
    private float _currTimer;
    public bool _isTimeclear;

    public string _name = "-";
    public float _maxHP;
    public float _currHP;
    public float _atkPoint = 0.0f;
    public float _atkDelay = 0.0f;
    public float _atkRange = 0.0f;
    public float _rangeRadius = 0.0f;

    public float _taxPoint = 0.0f;
    public float _taxDelay = 0.0f;

    public float _buyPrice = 0.0f;

    private bool isDes = false;
    private bool isShaking = false;
    private Material mat;


    public void setTileXZ (int setX, int setZ)
    {
        setTIleX = setX;
        setTIleZ = setZ;
    }

    private void Awake()
    {
        _currHP = _maxHP;
        tileMapSet = GameObject.Find("TileFloor").GetComponent<TileMapSetting>();
        mat = mGO.GetComponent<Renderer>().material;
    }

    private void Start()
    {
        typeSetting();
        StartCoroutine(setBuildingTimer());
    }

    public IEnumerator setBuildingTimer()
    {
        _currTimer = 0.0f;
        _isTimeclear = false;
        while (_currTimer <= 1.0f)
        {
            _currTimer += Time.deltaTime / _setTimer;
            mat.SetFloat("_DissolveAmount", 1.0f - _currTimer);
            yield return null;
        }
        Debug.Log("완성");
        _isTimeclear = true;
        yield return null;
    }

    private void LateUpdate()
    {
        Des();
    }

    private void Update()
    {
        if (isShaking)
        {
            mGO.transform.position = new Vector3(transform.position.x + Random.Range(-0.1f, +0.1f), transform.position.y, transform.position.z + Random.Range(-0.1f, +0.1f));
        }
    }

    public void Des()
    {
        if (_currHP <= 0.0f)
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

    private void typeSetting()
    {
        switch (ebuildingType)
        {
            case eBuildingType.NULL:
                break;
            case eBuildingType.WALL:
                break;
            case eBuildingType.FARM:
                break;
            case eBuildingType.WINDMILL:
                break;
            case eBuildingType.WELL:
                break;
            case eBuildingType.TOWER:
                GetComponent<SphereCollider>().radius = _atkRange;
                break;
            default:
                break;
        }
    }

    public IEnumerator SetShaking()
    {
        if (!isShaking)
        {
            isShaking = true;
            yield return new WaitForSeconds(0.3f);
            isShaking = false;
        }
    }
}
