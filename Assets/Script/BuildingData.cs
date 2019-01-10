using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum eBuildingType
{
    NULL,
    WALL,
    FARM,
    WINDMILL,
    WELL,
    TOWER,
    BASE
}

public class BuildingData : MonoBehaviour {

    public EffPoolMgr effPoolMgr;
    public ParticleSystem currParticle;

    public int sizeX;
    public int sizeZ;
    public eBuildingType ebuildingType;
    public GameObject subObject;
    public GameObject mGO;

    private int setTIleZ;
    private int setTIleX;

    private List<Image> _healthBar = new List<Image>();

    private TileMapSetting tileMapSet;
    public float _setTimer;
    private float _currTimer;
    public bool _isTimeclear;

    public GameObject _TimerGO;
    public Sprite _img;
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

    private bool isShaking = false;
    private Material mat;

    public bool isOrgim = false;

    public void setTileXZ (int setX, int setZ)
    {
        setTIleX = setX;
        setTIleZ = setZ;
        Debug.Log("setTIleX : " + setTIleX + ", " + "setTIleZ : " + setTIleZ);
    }

    private void Awake()
    {
        _currHP = _maxHP;
        tileMapSet = GameObject.Find("TileFloor").GetComponent<TileMapSetting>();
        mat = mGO.GetComponent<Renderer>().material;
    }

    private void Start()
    {
        _TimerGO = this.transform.GetChild(0).gameObject.transform.GetChild(4).gameObject;
        for (int i = 0; i < 3; i++)
        {
            _healthBar.Add(this.transform.GetChild(0).gameObject.transform.GetChild(i).gameObject.GetComponent<Image>());
            _healthBar[i].enabled = false;
        }

        typeSetting();

        if (!isOrgim) StartCoroutine(setBuildingTimer());
        if (!isOrgim) GetComponent<BoxCollider>().enabled = true;
        if (!isOrgim)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(0).gameObject.transform.Translate(0, +2.0f, 0);
        }
    }

    public IEnumerator setBuildingTimer()
    {
        if (eBuildingType.WINDMILL == ebuildingType)
        {
            transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.SetActive(false);
        }

        _currTimer = 0.0f;
        _isTimeclear = false;
        while (_currTimer <= 1.0f)
        {
            _currTimer += Time.deltaTime / _setTimer;
            mat.SetFloat("_DissolveAmount", 1.0f - _currTimer);
            _TimerGO.GetComponent<Image>().fillAmount = _currTimer;
            yield return null;
        }
        _isTimeclear = true;
        mat.SetFloat("_DissolveAmount", 0.0f);
        for (int i = 0; i < 3; i++)
        {
            _healthBar[i].enabled = true;
        }
        this.transform.GetChild(0).gameObject.transform.GetChild(3).gameObject.SetActive(false);
        _TimerGO.SetActive(false);

        if (eBuildingType.WINDMILL == ebuildingType)
        {
            transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.SetActive(true);
        }
        yield return null;
    }

    private void LateUpdate()
    {
        if (_currHP <= 0.0f) Des();
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
        for (int x = -sizeX; x < sizeX + 1; x++)
        {
            for (int z = -sizeZ; z < sizeZ + 1; z++)
            {
                Debug.Log("setTIleZ : " + setTIleZ);
                Debug.Log("setTIleX : " + setTIleX);
                Debug.Log("(setTIleZ + z) : " + (setTIleZ + z) + ", " + "(setTIleX + x) : " + (setTIleX + x));
                tileMapSet.listTileGo[(setTIleZ + z) + (setTIleX + x) * (tileMapSet.tileSizeXY / 2)].isBuilding = false;
                tileMapSet.listTileGo[(setTIleZ + z) + (setTIleX + x) * (tileMapSet.tileSizeXY / 2)].listGoType = eBuildingType.NULL;
                Debug.Log("isBuilding : " + tileMapSet.listTileGo[((setTIleZ + z) + (setTIleX + x) * (tileMapSet.tileSizeXY / 2))].isBuilding);
            }
        }

        effPoolMgr.addEff(currParticle, transform.position);
        Destroy(gameObject);
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
