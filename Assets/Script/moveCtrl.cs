using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveCtrl : MonoBehaviour {
    public float moveSpeed;
    public float rotSpeed;
    public float jumpPower;

    public float h;
    public float v;

    public bool isAtk;
    public Space tSpace;

    private Rigidbody Rg;

    private void Awake()
    {
        Rg = GetComponent<Rigidbody>();
    }

    void Update () {
        h = Input.GetAxis("MoveInput");
        v = Input.GetAxis("Vertical");
        moveInputKey();
    }

    void moveInputKey()
    {
        transform.Translate(Vector3.forward * v * moveSpeed * Time.deltaTime, tSpace);
        transform.Rotate(0, h * rotSpeed * Time.deltaTime, 0, tSpace);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isAtk = true;
        }

        //Rg.constraints = RigidbodyConstraints.None;
        //Rg.AddForce(Vector3.up * jumpPower, ForceMode.Force);
        //Rg.AddTorque(Vector3.right * 100.0f, ForceMode.Force);
    }
}
