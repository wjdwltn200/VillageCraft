using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveCtrl : MonoBehaviour {
    public float moveSpeed;
    public float rotSpeed;
    public float h;
    public float v;
    public bool isAtk = false;

    public bool isMove = false;


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        h = Input.GetAxis("MoveInput");
        v = Input.GetAxis("Vertical");
        moveInputKey();
        moveUp();
    }

    void moveInputKey()
    {
        if (Input.GetKeyDown(KeyCode.Space)) isAtk = true;

        transform.Translate(Vector3.forward * v * moveSpeed * Time.deltaTime);
        transform.Rotate(0, h * rotSpeed * Time.deltaTime, 0);
    }

    public void moveUp()
    {
        isMove = !isMove;

        if (isMove)
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }
}
