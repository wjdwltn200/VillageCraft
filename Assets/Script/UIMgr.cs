using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMgr : MonoBehaviour {
    private GameObject BuildingScroll;
    private ScrollRect sr;
    private BuildingSetting buildingSetting;
    public Button listButtonIcon;

    private void Awake()
    {
        GameObject tempList = GameObject.Find("buildingList");

        BuildingScroll = GameObject.Find("buildingContent");
        sr = tempList.GetComponent<ScrollRect>();
        buildingSetting = GameObject.Find("BuildingMgr").GetComponent<BuildingSetting>();

        // 스크롤뷰에 들어갈 버튼 생성과 동시에 데이터 넣어주기
        for (int i = 0; i < buildingSetting.buildingList.Length; i++)
        {
            Button temp = Instantiate(listButtonIcon, BuildingScroll.transform);
            temp.GetComponent<RectTransform>().position -= Vector3.down * (i * -temp.GetComponent<RectTransform>().rect.height + -(temp.GetComponent<RectTransform>().rect.height / 2.0f));
            temp.GetComponent<buildingListData>().setBuildingDate(buildingSetting.getDate(i));
            temp.GetComponent<buildingListData>().buildingArrIndex = i;
        }
        SetContentSize();

        tempList.SetActive(false);
    }

    // 지을 수 있는 건물 만큼 스크롤뷰 확장
    private void SetContentSize()
    {
        float width = BuildingScroll.GetComponent<RectTransform>().rect.width;
        float height = listButtonIcon.GetComponent<RectTransform>().rect.height * buildingSetting.buildingList.Length;

        sr.content.sizeDelta = new Vector2(width, height);
    }
} 
