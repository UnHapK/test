using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;

public class BackEndGameInfo : MonoBehaviour
{
    public GameObject InfoUI;

    [System.Obsolete]
    public void OnClickInsertData()
    {
        int charLevel = Random.Range(0, 99);
        int charExp = Random.Range(0, 9999);
        int charScore = Random.Range(0, 99999);

        Param param = new Param();

        param.Add("lv", charLevel);
        param.Add("exp", charExp);
        param.Add("score", charScore);

        Dictionary<string, int> equipment = new Dictionary<string, int>
        {
            {"weapon", 123 },
            {"armor", 111 },
            {"helmet", 1345 }
        };

        param.Add("equipItem", equipment);

        BackendReturnObject BRO = Backend.GameData.Insert("custom", param);

        if(BRO.IsSuccess())
        {
            Debug.Log("indate : " + BRO.GetInDate());
            InfoUI.SetActive(false);
        }
        else
        {
            switch(BRO.GetStatusCode())
            {
                case "404":
                    Debug.Log("table name ����");
                    break;
                case "412":
                    Debug.Log("table name ��Ȱ��ȭ");
                    break;
                case "413":
                    Debug.Log("�ϳ��� row�� 400KB �Ѿ�");
                    break;
                default:
                    Debug.Log("���� ���� ���� : " + BRO.GetMessage());
                    break;
            }
        }
    }
}
