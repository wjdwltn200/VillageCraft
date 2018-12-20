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
        buttonName.text = buildingDate.ebuildingType.ToString();
    }

    public void setBuildingDate(BuildingData data) { buildingDate = data; }

    public void selectValue()
    {
        UIMgr.isBuildingListSelect = true;
        mousePoint.selectBuildingValue = buildingArrIndex;
        plane.GetComponent<buildingInfoView>().setBuilding(buildingDate);
    }

    //public override void OnDrag(PointerEventData eventData)
    //{
    //    sr.OnBeginDrag(eventData);
    //}

    //public override void OnBeginDrag(PointerEventData eventData)
    //{
    //    sr.OnBeginDrag(eventData);
    //}

    //public override void OnEndDrag(PointerEventData eventData)
    //{
    //    sr.OnEndDrag(eventData);
    //}

    //public override void OnPointerDown(PointerEventData eventData)
    //{
    //    Debug.Log("PPAP");
    //}
}
