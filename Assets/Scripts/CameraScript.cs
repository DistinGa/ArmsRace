using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class CameraScript : MonoBehaviour
{
    const float UnitsPerPixel = 0.01f;
    float fieldWidth, fieldHeight;      //размеры карты
    float cameraWidth, cameraHeight;    //половинные размеры камеры

    Vector3 StartMovingPoint = Vector3.zero;
    Vector3 delta;

    bool _overMenu; //признак того, что курсор находится над меню

    public GameObject BackGround;
    // Use this for initialization
    void Awake()
    {
        //Настройка камеры под разрешение монитора
        Camera Camera = GetComponent<Camera>();
        Camera.orthographicSize = Screen.height * 0.5f * UnitsPerPixel;
        cameraHeight = Camera.orthographicSize;
        cameraWidth = Screen.width * 0.5f * UnitsPerPixel;
        Camera.aspect = cameraWidth / cameraHeight;

        Sprite Spr = BackGround.GetComponent<SpriteRenderer>().sprite;

        //Ширину и длину поля считаем в юнитах
        fieldWidth = Spr.rect.width * UnitsPerPixel;
        fieldHeight = Spr.rect.height * UnitsPerPixel;
    }

    void LateUpdate()
    {

        if (StartMovingPoint != Vector3.zero)
        {
            delta = (Input.mousePosition - StartMovingPoint) * UnitsPerPixel;
            delta.z = 0;

            SetNewPosition(transform.position - delta);

            StartMovingPoint = Input.mousePosition;
        }

        if (Input.GetMouseButtonDown(1) && !_overMenu)
        {
            StartMovingPoint = Input.mousePosition;
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
        //Проверка границ карты по Y
        if (NewCamPosition.y + cameraHeight > BackGround.transform.position.y)
            NewCamPosition.y = BackGround.transform.position.y - cameraHeight;
        if (NewCamPosition.y - cameraHeight < BackGround.transform.position.y - fieldHeight)
            NewCamPosition.y = BackGround.transform.position.y - fieldHeight + cameraHeight;

        //Проверка границ карты по Х
        if (NewCamPosition.x + cameraWidth > BackGround.transform.position.x + fieldWidth)
            NewCamPosition.x = BackGround.transform.position.x + fieldWidth - cameraWidth;
        if (NewCamPosition.x - cameraWidth < BackGround.transform.position.x)
            NewCamPosition.x = BackGround.transform.position.x + cameraWidth;

        transform.position = NewCamPosition;
    }

    public void SetNewPosition(Transform NewTransform)
    {
        SetNewPosition(new Vector3(NewTransform.position.x, transform.position.y, NewTransform.position.z));
        //GameManagerScript.GM.SnapToCountry(NewTransform.position);
    }
}
