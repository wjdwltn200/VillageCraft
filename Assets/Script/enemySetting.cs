using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class AddOption
{
    public Vector3 vPos;
    public Quaternion qPos;
}

public class enemySetting : MonoBehaviour {

    List<GameObject> enemyObj;
    AddOption addOp;

    public GameObject enemyNum1;

    public Text num;
    public Text num2;

    public int enemyMaxCount;
    float deltaTime = 0.0f;


    private void Awake()
    {
        enemyObj = new List<GameObject>();
        addOp = new AddOption();
    }

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {

        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;

        num.text = "생성된 개수 : " + enemyObj.Count.ToString();
        num2.text = "FPS : " + fps.ToString();

    }

    public void addMax()
    {
        addOp.vPos.z = 0.0f;
        addOp.vPos.x = 0.0f;

        for (int i = 0; i < enemyMaxCount; i++)
        {
            addOp.vPos.z += 1.0f;
            addOp.vPos.x = 0.0f;
            for (int j = 0; j < enemyMaxCount; j++)
            {
                addOp.vPos.x += 1.0f;
                enemyObj.Add(Instantiate(enemyNum1, addOp.vPos, addOp.qPos, transform));
            }
        }
    }

    public void addUnit()
    {
        addOp.vPos.x += 1.0f;
        enemyObj.Add(Instantiate(enemyNum1, addOp.vPos, addOp.qPos, transform));
    }




    //void OnGUI()
    //{
    //    int w = Screen.width, h = Screen.height;

    //    GUIStyle style = new GUIStyle();

    //    Rect rect = new Rect(0, 0, w, h * 2 / 100);
    //    style.alignment = TextAnchor.UpperLeft;
    //    style.fontSize = h * 2 / 100;
    //    style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);

    //    string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
    //    GUI.Label(rect, text, style);
    //}
}
