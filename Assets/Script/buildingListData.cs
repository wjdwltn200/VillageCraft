using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class buildingListData : MonoBehaviour {

    BuildingData buildingDate;
    MousePoint mousePoint;
    GameObject plane;
    BuildingCraftingUI UIMgr;
    //ScrollRect sr;

    public Sprite _currImg;
    public int buildingArrIndex = -1;
    public Text buttonName;

    private void Awake()
    {
        //sr = GameObject.Find("buildingList").GetComponent<ScrollRect>();
        UIMgr = GameObject.Find("UIMgr").GetComponent<BuildingCraftingUI>();
        mousePoint = GameObject.Find("BuildingMgr").GetComponent<MousePoint>();
        plane = GameObject.Find("buildingInfoView");
    }

    private void Start()
    {
        _currImg = buildingDate._img;
        buttonName.text = buildingDate._name;
        this.GetComponent<Image>().sprite = _currImg;
    }

    public void setBuildingDate(BuildingData data) { buildingDate = data; }

    public void selectValue()
    {
        UIMgr.isBuildingListSelect = true;
        mousePoint.selectBuildingValue = buildingArrIndex;
        plane.GetComponent<buildingInfoView>().setBuilding(buildingDate);
    }
}
