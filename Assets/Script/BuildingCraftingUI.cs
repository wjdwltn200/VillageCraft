using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingCraftingUI : MonoBehaviour {

    public bool isBuildingListSelect;

    public void UIon(GameObject ui)
    {
        if (!ui.activeSelf)
        {
            ui.SetActive(true);
        }
        else
        {
            ui.SetActive(false);

            // 자식 버튼 취소 처리
            isBuildingListSelect = false;
        }
    }
}
