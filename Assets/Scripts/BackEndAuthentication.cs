using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BackEnd;
using BackEnd.Tcp;

public class BackEndAuthentication : MonoBehaviour
{
    public InputField idInput;
    public InputField paInput;

    public GameObject LoginUI;

    public void OnClickSignUp()
    {
        BackendReturnObject BRO = Backend.BMember.CustomSignUp(idInput.text, paInput.text, "�α��� ���·� ���Ե� ����");

        if(BRO.IsSuccess())
        {
            Debug.Log("ȸ�� ���� �Ϸ�");
        }
        else
        {
            BackEndManager.MyInstance.ShowErrorUI(BRO);
        }
    }

    public void OnClickLogin()
    {
        BackendReturnObject BRO = Backend.BMember.CustomLogin(idInput.text, paInput.text);

        if(BRO.IsSuccess())
        {
            Debug.Log("�α��� �Ϸ�");
            Backend.BMember.CreateNickname(idInput.text);
            BackEndManager.MyInstance.CreateMatchRoom();
            LoginUI.SetActive(false);
            //BackEndManager.MyInstance.MatchMakingHandler();
            //BackEndManager.MyInstance.GameHandler();
        }
        else
        {
            BackEndManager.MyInstance.ShowErrorUI(BRO);
        }
    }

    public void AutoLogin()
    {
        BackendReturnObject backendReturnObject = Backend.BMember.LoginWithTheBackendToken();
        
        if(backendReturnObject.IsSuccess())
        {
            Debug.Log("�ڵ� �α��� �Ϸ�");
            Backend.BMember.CreateNickname(idInput.text);
            BackEndManager.MyInstance.CreateMatchRoom();
            LoginUI.SetActive(false);
        }
        else
        {
            BackEndManager.MyInstance.ShowErrorUI(backendReturnObject);
        }
    }

}
