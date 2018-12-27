using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveCtrl : MonoBehaviour {
    public Joystick joystick;
    public float moveSpeed;

    private Vector3 _moveVector;
    private Transform _transform;

    public float h;
    public float v;

    public bool isAtk;
    public Space tSpace;

    private void Start()
    {
        _transform = transform;
        _moveVector = Vector3.zero;
    }

    void Update () {
        HandleInput();
    }

    private void FixedUpdate()
    {
        if (v != 0.0f|| h != 0.0f) Move();

    }

    public void HandleInput()
    {
        _moveVector = PoolInput();
    }

    public Vector3 PoolInput()
    {
        h = joystick.GetHorizontalValue();
        v = joystick.GetVerticalValue();
        Vector3 moveDir = new Vector3(h, 0, v).normalized;
        return moveDir;
    }

    public void Move()
    {
        transform.forward = _moveVector;
        _transform.Rotate(0, 45.0f, 0, Space.World);
        _transform.Translate(transform.forward * moveSpeed * Time.deltaTime, Space.World);
    }

    public void AtkInput()
    {
        if (!isAtk) isAtk = true;
    }

    //void moveInputKey()
    //{
    //    transform.Translate(Vector3.forward * v * moveSpeed * Time.deltaTime, tSpace);
    //    transform.Rotate(0, h * rotSpeed * Time.deltaTime, 0, tSpace);

    //    if (!isAtk && Input.GetKeyDown(KeyCode.Space))
    //    {
    //        isAtk = true;
    //    }

    //    //Rg.constraints = RigidbodyConstraints.None;
    //    //Rg.AddForce(Vector3.up * jumpPower, ForceMode.Force);
    //    //Rg.AddTorque(Vector3.right * 100.0f, ForceMode.Force);
    //}
}
