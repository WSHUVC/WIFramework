using UnityEngine;

public struct UserInput
{
    public bool leftClick;
    public bool rightClick;
    public bool wheelClick;
    public bool onClick;
    public float mouseX;
    public float mouseY;
    public float mouseWheel;
    public float keyboardX;
    public float keyboardY;

    public void GetInput()
    {
        leftClick = Input.GetMouseButton(0);
        rightClick = Input.GetMouseButton(1);
        wheelClick = Input.GetMouseButton(2);

        onClick = leftClick || rightClick || wheelClick;

        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
        mouseWheel = Input.GetAxis("Mouse ScrollWheel");

        keyboardX = Input.GetAxis("Horizontal");
        keyboardY = Input.GetAxis("Vertical");
    }
}


