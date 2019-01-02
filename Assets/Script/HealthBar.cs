using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum eUiType
{
    Monster,
    player,
    building
}

public class HealthBar : MonoBehaviour {

    public eUiType eUiType;
    public GameObject addGo;
    public GameObject _hpBar;
    private Vector3 _hpScale;

    public float _maxHP;
    public float _currHP;

    public bool BillboardX = true;
    public bool BillboardY = true;
    public bool BillboardZ = true;
    public float OffsetToCamera;
    protected Vector3 localStartPosition;

    private void Awake()
    {
        if (addGo.GetComponent<BuildingSys>() && addGo.GetComponent<BuildingSys>().isOrigin) return;

        transform.position = new Vector3(transform.position.x, addGo.transform.position.y, transform.position.z);
    }

    // Use this for initialization
    void Start()
    {
        if (addGo.GetComponent<BuildingSys>() && addGo.GetComponent<BuildingSys>().isOrigin) return;

        switch (eUiType)
        {
            case eUiType.Monster:
                BuildingCraftingUI.OnMonsterUI += this.onUI;
                break;
            case eUiType.player:
                BuildingCraftingUI.OnPlayerUI += this.onUI;
                break;
            case eUiType.building:
                BuildingCraftingUI.OnBuildingUI += this.onUI;
                break;
            default:
                break;
        }

        GetComponent<Canvas>().enabled = true;

        localStartPosition = transform.localPosition;
        _currHP = _maxHP = addGo.GetComponent<damageSys>().getHpPoint();
        _hpScale = _hpBar.GetComponent<RectTransform>().localScale;
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        if (addGo.GetComponent<BuildingSys>() && addGo.GetComponent<BuildingSys>().isOrigin) return;

        LookCam();
        hpSys();
    }

    void hpSys()
    {
        // 체력바 표기
        _currHP = addGo.GetComponent<damageSys>().getHpPoint();
        _hpScale.x = (_currHP /_maxHP) * 1.0f;
        _hpBar.GetComponent<RectTransform>().localScale = _hpScale;
    }

    void LookCam()
    {
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                                               Camera.main.transform.rotation * Vector3.up);
        if (!BillboardX || !BillboardY || !BillboardZ)
            transform.rotation = Quaternion.Euler(BillboardX ? transform.rotation.eulerAngles.x : 0f, BillboardY ? transform.rotation.eulerAngles.y : 0f, BillboardZ ? transform.rotation.eulerAngles.z : 0f);
        transform.localPosition = localStartPosition;
        transform.position = transform.position + transform.rotation * Vector3.forward * OffsetToCamera;
    }

    private void OnDisable()
    {
        switch (eUiType)
        {
            case eUiType.Monster:
                BuildingCraftingUI.OnMonsterUI -= this.onUI;
                break;
            case eUiType.player:
                BuildingCraftingUI.OnPlayerUI -= this.onUI;
                break;
            case eUiType.building:
                BuildingCraftingUI.OnBuildingUI -= this.onUI;
                break;
            default:
                break;
        }
    }

    public void onUI(bool isOn)
    {
        transform.GetChild(0).GetComponent<Image>().enabled = isOn;
        transform.GetChild(1).GetComponent<Image>().enabled = isOn;
        transform.GetChild(2).GetComponent<Image>().enabled = isOn;
    }
}
