using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenEffMgrCS : MonoBehaviour {

    public GameObject[] _screenEffIndex;
    private GameObject _currEffIndex;

    private void Awake()
    {
        for (int i = 0; i < _screenEffIndex.Length; i++)
        {
            _screenEffIndex[i].SetActive(false);
        }
    }

    public void setScreenEff(int value)
    {
        _screenEffIndex[value].SetActive(true);
        _currEffIndex = _screenEffIndex[value];
        StartCoroutine(EffSys());
    }

    IEnumerator EffSys()
    {
        float time = 0.0f;
        float value = 0.0f;
        while (time <= 5.0f)
        {
            time += Time.deltaTime;
            //value += 0.1f;

            _currEffIndex.transform.GetChild(0).GetComponent<Image>().color =
                new Color(_currEffIndex.transform.GetChild(0).GetComponent<Image>().color.r,
                _currEffIndex.transform.GetChild(0).GetComponent<Image>().color.g,
                _currEffIndex.transform.GetChild(0).GetComponent<Image>().color.b,
                Mathf.Sin(Time.time * 5.0f) / 2 + 0.2f);

            //if (value >= 0.7f) value = 0.0f;
            yield return null;
        }

        _currEffIndex.SetActive(false);
        yield break;
    }
}
