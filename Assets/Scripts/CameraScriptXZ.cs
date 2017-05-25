using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class CameraScriptXZ : MonoBehaviour
{
    public Transform Frame;
    Camera Camera;
    public float Offset;    //Отступы по бокам для отображения атмосферы
    float fieldWidth, fieldHeight;      //размеры карты
    float cameraWidth, cameraHeight;    //половинные размеры камеры
    float UnitsPerPixel;

    Vector3 StartMovingPoint = Vector3.zero;
    Vector3 delta;

    bool _overMenu; //признак того, что курсор находится над меню

    void Awake()
    {
        Camera = FindObjectOfType<Camera>();
        cameraHeight = Camera.orthographicSize;
        cameraWidth = Camera.aspect * cameraHeight;

        fieldWidth = Frame.localScale.x;
        fieldHeight = Frame.localScale.y;

        UnitsPerPixel = 2* cameraHeight / Screen.height;
    }

    void LateUpdate()
    {
        if (GameManagerScript.GM.delayedStop)
            return;

        //Vector3 mousePos = Camera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mousePos = Input.mousePosition;
        if (StartMovingPoint != Vector3.zero)
        {
            //delta = (mousePos - StartMovingPoint);
            delta = (mousePos - StartMovingPoint) * UnitsPerPixel;

            //Переводим из экранных координат X-Y в координаты карты X-Z
            delta = new Vector3(delta.x, 0, delta.y);
            SetNewPosition(transform.position - delta);

            StartMovingPoint = mousePos;
        }

        if (Input.GetMouseButtonDown(1) && !_overMenu)
        {
            StartMovingPoint = mousePos;
        }

        if (Input.GetMouseButtonUp(1))
        {
            StartMovingPoint = Vector3.zero;
        }
    }

    public bool setOverMenu
    {
        set { _overMenu = value; }
        get { return _overMenu; }
    }

    public void SetNewPosition(Vector3 NewCamPosition)
    {
        //Проверка границ карты по Z
        if (NewCamPosition.z - cameraHeight < Frame.position.z - 0.5f * fieldHeight)
            NewCamPosition.z = Frame.position.z - 0.5f * fieldHeight + cameraHeight;
        if (NewCamPosition.z + cameraHeight > Frame.position.z + 0.5f * fieldHeight)
            NewCamPosition.z = Frame.position.z + 0.5f * fieldHeight - cameraHeight;

        //Проверка границ карты по Х
        if (NewCamPosition.x + cameraWidth > Frame.transform.position.x + 0.5f * fieldWidth)
            NewCamPosition.x = Frame.position.x + 0.5f * fieldWidth - cameraWidth;
        if (NewCamPosition.x - cameraWidth < Frame.position.x - 0.5f * fieldWidth)
            NewCamPosition.x = Frame.position.x - 0.5f * fieldWidth + cameraWidth;

        transform.position = NewCamPosition;
    }

    public void SetNewPosition(Transform NewTransform)
    {
        SetNewPosition(new Vector3(NewTransform.position.x, transform.position.y, NewTransform.position.z));
        //GameManagerScript.GM.SnapToCountry(NewTransform.position);
    } 
}
