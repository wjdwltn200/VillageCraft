using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    private Image bgimg;
    private Image joystickImg;
    private Vector3 inputVector;

    void Start()
    {
        bgimg = GetComponent<Image>();
        joystickImg = transform.GetChild(0).GetComponent<Image>();
    }

    public virtual void OnDrag(PointerEventData ped)
    {
        //Debug.Log("Joystick >>> OnDrag()");
        Vector2 pos;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(bgimg.rectTransform, ped.position, ped.pressEventCamera, out pos))
        {
            //Debug.Log("pos.x : " + pos.x.ToString());
            //Debug.Log("pos.y : " + pos.y.ToString());

            pos.x = ((pos.x - (bgimg.rectTransform.sizeDelta.x / 2.0f)) / bgimg.rectTransform.sizeDelta.x);
            pos.y = ((pos.y + (bgimg.rectTransform.sizeDelta.x / 2.0f)) / bgimg.rectTransform.sizeDelta.y);

            inputVector = new Vector3(pos.x * 2 + 1, pos.y * 2 - 1, 0);
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

            // Move Joustick IMG
            joystickImg.rectTransform.anchoredPosition = new Vector3(inputVector.x * (bgimg.rectTransform.sizeDelta.x / 3), inputVector.y * (bgimg.rectTransform.sizeDelta.y / 3));
        }
    }

    public virtual void OnPointerDown(PointerEventData ped)
    {
        OnDrag(ped);
    }

    public virtual void OnPointerUp(PointerEventData ped)
    {
        inputVector = Vector3.zero;
        joystickImg.rectTransform.anchoredPosition = Vector3.zero;
    }

    public float GetHorizontalValue()
    {
        return inputVector.x;
    }

    public float GetVerticalValue()
    {
        return inputVector.y;
    }
}
