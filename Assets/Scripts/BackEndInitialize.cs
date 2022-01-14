using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;

public class BackEndInitialize : MonoBehaviour
{
    [System.Obsolete]
    private void Awake()
    {
        Backend.Initialize(BRO =>
        {
            if (BRO.IsSuccess())
            {
                if (!Backend.Utils.GetGoogleHash().Equals(""))
                    Debug.Log(Backend.Utils.GetGoogleHash());
            }
            else
            {
                Debug.Log("초기화 실패ㅠㅠ");
            }
        });
    }
}
