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
        BackendReturnObject BRO = Backend.BMember.CustomSignUp(idInput.text, paInput.text, "로그인 강좌로 가입된 유저");

        if(BRO.IsSuccess())
        {
            Debug.Log("회원 가입 완료");
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
            Debug.Log("로그인 완료");
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
            Debug.Log("자동 로그인 완료");
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
