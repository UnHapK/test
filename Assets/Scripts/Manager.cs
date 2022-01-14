using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using UnityEngine.UI;
using System;
using Protocol;
using System.Text;
using FreeDraw;

public class Message
{
    public string type;
    public Message(string _type)
    {
        type = _type;
    }
}
public class TextClass : Message
{
    public String createText;
    public Texture2D createTexture;
    public TextClass(String s, Texture2D b) : base("TextClass")
    {
        createText = s;
        createTexture = b;
    }
}

public class Manager : MonoBehaviour
{
    public static Manager MInstance = null;

    public static event Action InGame = delegate { };

    public InputField Write_IF;
    public GameObject DrawObj;

    public Texture2D DrawSprite;

    public bool InGameCheck = false;

    private IEnumerator InGameUpdateCoroutine;


    private void Awake()
    {
        MInstance = this;

        InGameUpdateCoroutine = InGameUpdate();
    }

    // Start is called before the first frame update
    void Start()
    {
        InGame += TextInput;
    }

    void TextInput()
    {
        Debug.Log("TextInput");

        TextClass TextData = new TextClass(Write_IF.text, DrawSprite);

        Debug.Log("PlayerCreate");
        var jsonData = JsonUtility.ToJson(TextData);
        var byte_ = Encoding.UTF8.GetBytes(jsonData);
  
        Backend.Match.SendDataToInGameRoom(byte_);   
    }

    // Update is called once per frame
    void Update()
    {
        Backend.Match.Poll();
    }

    public void RoomList()
    {

    }

    public void StartGame()
    {
        Write_IF.gameObject.SetActive(true);
        DrawObj.SetActive(true);
        StartCoroutine(InGameUpdate());
    }

    public void OpenRoomUI()
    {
        if (BackEndManager.MyInstance.CreateMatchRoom() == true)
        {
            Debug.Log("대기방 진입 성공!!");
        }
    }

    IEnumerator InGameUpdate()
    {
        while (true)
        {
            //if (!InGameCheck)
            //{
            //    StopCoroutine(InGameUpdateCoroutine);
            //    yield return null;
            //}
            InGame();
            yield return new WaitForSeconds(.1f); //1초 단위
        }
    }
}
