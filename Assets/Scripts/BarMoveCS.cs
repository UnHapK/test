using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarMoveCS : MonoBehaviour
{
    private float prePostion;


    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            prePostion = Input.mousePosition.x;
        }
#else
        if (Input.GetTouch(0).phase == TouchPhase.Began)
        {
            prePostion = Input.GetTouch(0).position.x;
        }
#endif

        if (BlockManager.BInstance.StartCheck)
        {
#if UNITY_EDITOR
            if (Input.GetMouseButton(0))
            {
                float _deltaPosX = Input.mousePosition.x - prePostion;
                transform.localPosition = new Vector2(transform.localPosition.x + (_deltaPosX * 1.5f), transform.localPosition.y);
                prePostion = Input.mousePosition.x;
            }
#else
            if (Input.touchCount > 0)
            {
                float _deltaPosX = Input.GetTouch(0).position.x - prePostion;
                transform.localPosition = new Vector2(transform.localPosition.x + (_deltaPosX * 1.5f), transform.localPosition.y);
                prePostion = Input.GetTouch(0).position.x;
            }
#endif

            transform.localPosition = new Vector3(Mathf.Clamp(transform.localPosition.x, -850, 850), transform.localPosition.y, transform.localPosition.z);
        }
    }

    public void ItemGetSoundPlay()
    {
        this.GetComponent<AudioSource>().Play();
    }
}
