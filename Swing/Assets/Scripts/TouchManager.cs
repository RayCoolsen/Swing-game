using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchManager : MonoBehaviour
{
    [SerializeField] private float dropTouchHeight = 7.5f;
    private int gridWidth;

    private ScaleGrid scaleGrid;

    private void Awake()
    {
        scaleGrid = GetComponent<ScaleGrid>();
        gridWidth = scaleGrid.GetGridWidth();

        if (Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer)
        {
            Destroy(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
            touchPosition.z = 0;
            if(touchPosition.y > dropTouchHeight)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    scaleGrid.ReleaseBall();
                }
            }
            else
            {
                int touchPlayerPos = (int)(touchPosition.x + 0.5f);
                touchPlayerPos = Mathf.Clamp(touchPlayerPos, 0, gridWidth - 1);
                scaleGrid.MovePlayer(touchPlayerPos);
            }

            
        }
    }
}
