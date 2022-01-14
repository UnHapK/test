using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using BackEnd.Tcp;
using System.Linq;
using Protocol;
using System.Text;
using FreeDraw;

public class BackEndManager : MonoBehaviour
{
    private static BackEndManager instance = null;
    public static BackEndManager MyInstance { get => instance; set => instance = value; }
    public bool isConnectInGameServer { get; private set; }

    public bool isConnectMatchServer = false;
    private bool isJoinGameRoom = false;
    public bool isReconnectProcess { get; private set; } = false;

    public string inGameRoomToken = string.Empty;

    public bool isSandBoxGame { get; private set; } = false;

    public List<SessionId> sessionIdList { get; private set; }  // ��ġ�� �������� �������� ���� ���
    // public Dictionary<SessionId, int> teamInfo { get; private set; }    // ��ġ�� �������� �������� �� ���� (MatchModeType�� team�� ��쿡�� ���)
    public Dictionary<SessionId, MatchUserGameRecord> gameRecords { get; private set; } = null;  // ��ġ�� �������� �������� ��Ī ���

    public SessionId hostSession { get; private set; }  // ȣ��Ʈ ����

    // ���� �α�
    private string FAIL_ACCESS_INGAME = "�ΰ��� ���� ���� : {0} - {1}";
    private string SUCCESS_ACCESS_INGAME = "���� �ΰ��� ���� ���� : {0}";
    private string NUM_INGAME_SESSION = "�ΰ��� �� ���� ���� : {0}";

    // ����� �α�
    private string NOTCONNECT_MATCHSERVER = "��ġ ������ ����Ǿ� ���� �ʽ��ϴ�.";
    private string RECONNECT_MATCHSERVER = "��ġ ������ ������ �õ��մϴ�.";
    private string FAIL_CONNECT_MATCHSERVER = "��ġ ���� ���� ���� : {0}";
    private string SUCCESS_CONNECT_MATCHSERVER = "��ġ ���� ���� ����";
    private string SUCCESS_MATCHMAKE = "��Ī ���� : {0}";
    private string SUCCESS_REGIST_MATCHMAKE = "��Ī ��⿭�� ��ϵǾ����ϴ�.";
    private string FAIL_REGIST_MATCHMAKE = "��Ī ���� : {0}";
    private string CANCEL_MATCHMAKE = "��Ī ��û ��� : {0}";
    private string INVAILD_MATCHTYPE = "�߸��� ��ġ Ÿ���Դϴ�.";
    private string INVALID_MODETYPE = "�߸��� ��� Ÿ���Դϴ�.";
    private string INVALID_OPERATION = "�߸��� ��û�Դϴ�\n{0}";
    private string EXCEPTION_OCCUR = "���� �߻� : {0}\n�ٽ� ��Ī�� �õ��մϴ�.";

    public string MyNickname;

    public MatchType nowMatchType { get; private set; } = MatchType.None;
    public MatchModeType nowModeType { get; private set; } = MatchModeType.None;

    private int numOfClient = 2;

    public List<MatchInfo> matchInfos { get; private set; } = new List<MatchInfo>();

    // �ֿܼ��� ������ ��Ī ī�� ����
    public class MatchInfo
    {
        public string title;                // ��Ī ��
        public string inDate;               // ��Ī inDate (UUID)
        public MatchType matchType;         // ��ġ Ÿ��
        public MatchModeType matchModeType; // ��ġ ��� Ÿ��
        public string headCount;            // ��Ī �ο�
        public bool isSandBoxEnable;        // ����ڽ� ��� (AI��Ī)
    }

    #region Host
    private bool isHost = false;                    // ȣ��Ʈ ���� (�������� ������ SuperGamer ������ ������)
    private Queue<KeyMessage> localQueue = null;    // ȣ��Ʈ���� ���÷� ó���ϴ� ��Ŷ�� �׾Ƶδ� ť (����ó���ϴ� �����ʹ� ������ �߼� ����)
    #endregion

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public static BackEndManager GetInstance()
    {
        if (!instance)
        {
            //Debug.LogError("BackEndMatchManager �ν��Ͻ��� �������� �ʽ��ϴ�.");
            return null;
        }

        return instance;
    }

    // Start is called before the first frame update
    void Start()
    {
        InitBackEnd();
        MatchMakingHandler();
        GameHandler();
        ExceptionHandler();
    }

    void OnApplicationQuit()
    {
        if (isConnectMatchServer)
        {
            LeaveMatchServer();
            Debug.Log("ApplicationQuit - LeaveMatchServer");
        }
    }

    [System.Obsolete]
    private void InitBackEnd()
    {
        Backend.Initialize(BRO =>
        {
            Debug.Log("�ڳ� �ʱ�ȭ ����" + BRO);

            if(BRO.IsSuccess())
            {
                Debug.Log(Backend.Utils.GetServerTime());
            }
            else
            {
                Debug.Log("�ڳ� �ʱ�ȭ ���Ф̤�");
            }
        });
    }

    //���� ���� Error
    public void ShowErrorUI(BackendReturnObject backendReturn)
    {
        int statusCode = int.Parse(backendReturn.GetStatusCode());

        switch(statusCode)
        {
            case 401:
                Debug.Log("ID or Password Ʋ�Ⱦ�~");
                break;
            case 403:
                Debug.Log(backendReturn.GetErrorCode());
                break;
            case 404:
                Debug.Log("game not found");
                break;
            case 408:
                Debug.Log(backendReturn.GetMessage());
                break;
            case 409:
                Debug.Log("ID �ߺ�~~");
                break;
            case 410:
                Debug.Log("bad refreshToken");
                break;
            case 429:
                Debug.Log(backendReturn.GetMessage());
                break;
            case 503:
                Debug.Log(backendReturn.GetMessage());
                break;
            case 504:
                Debug.Log(backendReturn.GetMessage());
                break;
        }
    }

    public bool IsHost()
    {
        return isHost;
    }

    public bool IsMySessionId(SessionId session)
    {
        return Backend.Match.GetMySessionId() == session;
    }

    public string GetNickNameBySessionId(SessionId session)
    {
        // return Backend.Match.GetNickNameBySessionId(session);
        return gameRecords[session].m_nickname;
    }

    public bool IsSessionListNull()
    {
        return sessionIdList == null || sessionIdList.Count == 0;
    }

    private bool SetHostSession()
    {
        // ȣ��Ʈ ���� ���ϱ�
        // �� Ŭ���̾�Ʈ�� ��� ���� (ȣ��Ʈ ���� ���ϴ� ������ ��� �����Ƿ� ������ Ŭ���̾�Ʈ�� ��� ������ ���������� ������� ����.)

        Debug.Log("ȣ��Ʈ ���� ���� ����");
        // ȣ��Ʈ ���� ���� (�� Ŭ���̾�Ʈ���� ���� ������ �ٸ� �� �ֱ� ������ ����)
        sessionIdList.Sort();
        isHost = false;
        // ���� ȣ��Ʈ ��������
        foreach (var record in gameRecords)
        {
            if (record.Value.m_isSuperGamer == true)
            {
                if (record.Value.m_sessionId.Equals(Backend.Match.GetMySessionId()))
                {
                    isHost = true;
                }
                hostSession = record.Value.m_sessionId;
                break;
            }
        }

        Debug.Log("ȣ��Ʈ ���� : " + isHost);

        // ȣ��Ʈ �����̸� ���ÿ��� ó���ϴ� ��Ŷ�� �����Ƿ� ���� ť�� �������ش�
        if (isHost)
        {
            localQueue = new Queue<KeyMessage>();
        }
        else
        {
            localQueue = null;
        }

        // ȣ��Ʈ �������� ������ ��ġ������ ���� ����
        LeaveMatchServer();
        return true;
    }

    private void SetSubHost(SessionId hostSessionId)
    {
        Debug.Log("���� ȣ��Ʈ ���� ���� ����");
        // ���� ���� ȣ��Ʈ �������� �������� ���� ������ Ȯ��
        // �������� ���� SuperGamer ������ GameRecords�� SuperGamer ���� ����
        foreach (var record in gameRecords)
        {
            if (record.Value.m_sessionId.Equals(hostSessionId))
            {
                record.Value.m_isSuperGamer = true;
            }
            else
            {
                record.Value.m_isSuperGamer = false;
            }
        }
        // ���� ȣ��Ʈ �������� Ȯ��
        if (hostSessionId.Equals(Backend.Match.GetMySessionId()))
        {
            isHost = true;
        }
        else
        {
            isHost = false;
        }

        hostSession = hostSessionId;

        Debug.Log("���� ȣ��Ʈ ���� : " + isHost);
        // ȣ��Ʈ �����̸� ���ÿ��� ó���ϴ� ��Ŷ�� �����Ƿ� ���� ť�� �������ش�
        if (isHost)
        {
            localQueue = new Queue<KeyMessage>();
        }
        else
        {
            localQueue = null;
        }

        Debug.Log("���� ȣ��Ʈ ���� �Ϸ�");
    }

    public void MatchMakingHandler()
    {
        Backend.Match.OnJoinMatchMakingServer += (args) =>
        {
            Debug.Log("OnJoinMatchMakingServer : " + args.ErrInfo);
            // ��Ī ������ �����ϸ� ȣ��
            ProcessAccessMatchMakingServer(args.ErrInfo);
        };
        Backend.Match.OnMatchMakingResponse += (args) =>
        {
            Debug.Log("OnMatchMakingResponse : " + args.ErrInfo + " : " + args.Reason);
            // ��Ī ��û ���� �۾��� ���� ȣ��
            ProcessMatchMakingResponse(args);
        };

        Backend.Match.OnLeaveMatchMakingServer += (args) =>
        {
            // ��Ī �������� ���� ������ �� ȣ��
            Debug.Log("OnLeaveMatchMakingServer : " + args.ErrInfo);
            isConnectMatchServer = false;

            if (args.ErrInfo.Category.Equals(ErrorCode.DisconnectFromRemote) || args.ErrInfo.Category.Equals(ErrorCode.Exception)
                || args.ErrInfo.Category.Equals(ErrorCode.NetworkTimeout))
            {
                // �������� ������ ���� ���
                Debug.Log("���� ����");
            }
        };

        // ��� �� ����/���� ����
        Backend.Match.OnMatchMakingRoomCreate += (args) =>
        {
            Debug.Log("OnMatchMakingRoomCreate : " + args.ErrInfo + " : " + args.Reason);
        };

        // ���濡 ���� ���� �޽���
        Backend.Match.OnMatchMakingRoomJoin += (args) =>
        {
            Debug.Log(string.Format("OnMatchMakingRoomJoin : {0} : {1}", args.ErrInfo, args.Reason));
            if (args.ErrInfo.Equals(ErrorCode.Success))
            {
                Debug.Log("user join in loom : " + args.UserInfo.m_nickName);
            }
        };

        // ���濡 ���� ������ �ִ� ���� ����Ʈ �޽���
        Backend.Match.OnMatchMakingRoomUserList += (args) =>
        {
            Debug.Log(string.Format("OnMatchMakingRoomUserList : {0} : {1}", args.ErrInfo, args.Reason));
            List<MatchMakingUserInfo> userList = null;
            if (args.ErrInfo.Equals(ErrorCode.Success))
            {
                userList = args.UserInfos;
                Debug.Log("ready room user count : " + userList.Count);
            }
        };

        // ���濡 ���� ���� �޽���
        Backend.Match.OnMatchMakingRoomLeave += (args) =>
        {
            Debug.Log(string.Format("OnMatchMakingRoomLeave : {0} : {1}", args.ErrInfo, args.Reason));
            if (args.ErrInfo.Equals(ErrorCode.Success) || args.ErrInfo.Equals(ErrorCode.Match_Making_KickedByOwner))
            {
                Debug.Log("user leave in loom : " + args.UserInfo.m_nickName);
                if (args.UserInfo.m_nickName.Equals(MyNickname))
                {
                    if (args.ErrInfo.Equals(ErrorCode.Match_Making_KickedByOwner))
                    {
                        Debug.Log("������߽��ϴ�.");
                    }
                    Debug.Log("�ڱ��ڽ��� �濡�� �������ϴ�.");
                    return;
                }
            }
        };

        // ������ ���濡�� ���� ���� �ı� �� �޽���
        Backend.Match.OnMatchMakingRoomDestory += (args) =>
        {
            Debug.Log(string.Format("OnMatchMakingRoomDestory : {0} : {1}", args.ErrInfo, args.Reason));
        };

        // ���濡 ���� �ʴ� ����/���� ����. (������ �ʴ� ����/������ �ƴ�.)
        Backend.Match.OnMatchMakingRoomInvite += (args) =>
        {
            Debug.Log(string.Format("OnMatchMakingRoomInvite : {0} : {1}", args.ErrInfo, args.Reason));
        };

        // �ʴ��� ������ �ʴ� ����/���� ����.
        Backend.Match.OnMatchMakingRoomInviteResponse += (args) =>
        {
            Debug.Log(string.Format("OnMatchMakingRoomInviteResponse : {0} : {1}", args.ErrInfo, args.Reason));
        };

        // ���� ���� ��� �޽���
        Backend.Match.OnMatchMakingRoomKick += (args) =>
        {
            Debug.Log(string.Format("OnMatchMakingRoomKick : {0} : {1}", args.ErrInfo, args.Reason));
            if (args.ErrInfo.Equals(ErrorCode.Success) == false)
            {
                Debug.Log("Error : " + args.Reason);
            }
        };

        // ������ ���� �ʴ������� ���ϵ�
        Backend.Match.OnMatchMakingRoomSomeoneInvited += (args) =>
        {
            Debug.Log(string.Format("OnMatchMakingRoomSomeoneInvited : {0} : {1}", args.ErrInfo, args.Reason));
            var roomId = args.RoomId;
            var roomToken = args.RoomToken;
            Debug.Log(string.Format("room id : {0} / token : {1}", roomId, roomToken));
            MatchMakingUserInfo userInfo = args.InviteUserInfo;
        };
    }

    // �ΰ��� ���� ���� �̺�Ʈ �ڵ鷯
    public void GameHandler()
    {
        Backend.Match.OnSessionJoinInServer += (args) =>
        {
            Debug.Log("OnSessionJoinInServer : " + args.ErrInfo);
            // �ΰ��� ������ �����ϸ� ȣ��
            if (args.ErrInfo != ErrorInfo.Success)
            {
                if (isReconnectProcess)
                {
                    if (args.ErrInfo.Reason.Equals("Reconnect Success"))
                    {
                        //������ ����
                        //GameManager.GetInstance().ChangeState(GameManager.GameState.Reconnect);
                        Debug.Log("������ ����");
                    }
                    else if (args.ErrInfo.Reason.Equals("Fail To Reconnect"))
                    {
                        Debug.Log("������ ����");
                        JoinMatchServer();
                        isConnectInGameServer = false;
                    }
                }
                return;
            }
            if (isJoinGameRoom)
            {
                return;
            }
            if (inGameRoomToken == string.Empty)
            {
                Debug.LogError("�ΰ��� ���� ���� ���������� �� ��ū�� �����ϴ�.");
                return;
            }
            Debug.Log("�ΰ��� ���� ���� ����");
            isJoinGameRoom = true;
            Backend.Match.JoinGameRoom(inGameRoomToken);
            Manager.MInstance.StartGame();
        };

        Backend.Match.OnSessionListInServer += (args) =>
        {
            // ���� ����Ʈ ȣ�� �� ���� ä���� ȣ���
            // ���� ���� ����(��)�� �������� �÷��̾�� �� ������ ���� �� �濡 ���� �ִ� �÷��̾��� ���� ������ ����ִ�.
            // ������ �ʰ� ���� �÷��̾���� ������ OnMatchInGameAccess ���� ���ŵ�
            Debug.Log("OnSessionListInServer : " + args.ErrInfo);

            ProcessMatchInGameSessionList(args);
        };

        Backend.Match.OnMatchInGameAccess += (args) =>
        {
            Debug.Log("OnMatchInGameAccess : " + args.ErrInfo);
            // ������ �ΰ��� �뿡 ������ ������ ȣ�� (�� Ŭ���̾�Ʈ�� �ΰ��� �뿡 ������ ������ ȣ���)
            ProcessMatchInGameAccess(args);
        };

        Backend.Match.OnMatchInGameStart += () =>
        {
            // �������� ���� ���� ��Ŷ�� ������ ȣ��
            //GameSetup();
            Debug.Log("���� ���� ��Ŷ ����");
        };

        Backend.Match.OnMatchResult += (args) =>
        {
            Debug.Log("���� ����� ���ε� ��� : " + string.Format("{0} : {1}", args.ErrInfo, args.Reason));
            // �������� ���� ��� ��Ŷ�� ������ ȣ��
            // ����(Ŭ���̾�Ʈ��) ������ ���� ������� ���������� ������Ʈ �Ǿ����� Ȯ��

            if (args.ErrInfo == BackEnd.Tcp.ErrorCode.Success)
            {
                //InGameUiManager.instance.SetGameResult();
                //GameManager.GetInstance().ChangeState(GameManager.GameState.Result);
            }
            else if (args.ErrInfo == BackEnd.Tcp.ErrorCode.Match_InGame_Timeout)
            {
                Debug.Log("���� ���� ���� : " + args.ErrInfo);
                //LobbyUI.GetInstance().MatchCancelCallback();
            }
            else
            {
                //InGameUiManager.instance.SetGameResult("��� ���� ����\nȣ��Ʈ�� ������ ������ϴ�.");
                Debug.Log("���� ��� ���ε� ���� : " + args.ErrInfo);
            }
            // ���Ǹ���Ʈ �ʱ�ȭ
            sessionIdList = null;
        };

        Backend.Match.OnMatchRelay += (args) =>
        {
            // �� Ŭ���̾�Ʈ���� ������ ���� �ְ���� ��Ŷ��
            // ������ �ܼ� ��ε�ĳ���ø� ���� (�������� ��� ���굵 �������� ����)

            // ���� ���� ����
            //if (PrevGameMessage(args.BinaryUserData) == true)
            //{
            //    // ���� ���� ������ �����Ͽ����� �ٷ� ����
            //    return;
            //}

            //if (WorldManager.instance == null)
            //{
            //    // ���� �Ŵ����� �������� ������ �ٷ� ����
            //    return;
            //}

            //WorldManager.instance.OnRecieve(args);

            var strByte = Encoding.Default.GetString(args.BinaryUserData);
            Message msg = JsonUtility.FromJson<Message>(strByte);
            Debug.Log("OnMatchRelay_1");
            if (msg.type == "TextClass")
            {
                TextClass TextData = JsonUtility.FromJson<TextClass>(strByte);
                if(Manager.MInstance.Write_IF.text.ToString() != TextData.createText)
                    Manager.MInstance.Write_IF.text = TextData.createText;

                var bytes = TextData.createTexture.EncodeToPNG();

                Texture2D tex = new Texture2D(16, 16, TextureFormat.PVRTC_RGBA4, false);
                tex.LoadRawTextureData(bytes);
                tex.Apply();

                Manager.MInstance.DrawSprite = tex;

                Debug.Log("OnMatchRelay_2");
            }
        };

        Backend.Match.OnMatchChat += (args) =>
        {
            // ä�ñ���� Ʃ�丮�� �������� �ʾҽ��ϴ�.
        };

        Backend.Match.OnLeaveInGameServer += (args) =>
        {
            Debug.Log("OnLeaveInGameServer : " + args.ErrInfo + " : " + args.Reason);
            if (args.Reason.Equals("Fail To Reconnect"))
            {
                JoinMatchServer();
            }
            isConnectInGameServer = false;
        };

        Backend.Match.OnSessionOnline += (args) =>
        {
            // �ٸ� ������ ������ ���� �� ȣ��
            var nickName = Backend.Match.GetNickNameBySessionId(args.GameRecord.m_sessionId);
            Debug.Log(string.Format("[{0}] �¶��εǾ����ϴ�. - {1} : {2}", nickName, args.ErrInfo, args.Reason));
            //ProcessSessionOnline(args.GameRecord.m_sessionId, nickName);
        };

        Backend.Match.OnSessionOffline += (args) =>
        {
            // �ٸ� ���� Ȥ�� �ڱ��ڽ��� ������ �������� �� ȣ��
            Debug.Log(string.Format("[{0}] �������εǾ����ϴ�. - {1} : {2}", args.GameRecord.m_nickname, args.ErrInfo, args.Reason));
            // ���� ������ �ƴϸ� �������� ���μ��� ����
            if (args.ErrInfo != ErrorCode.AuthenticationFailed)
            {
               // ProcessSessionOffline(args.GameRecord.m_sessionId);
            }
            else
            {
                // �߸��� ������ �õ� �� ���������� �߻�
            }
        };

        Backend.Match.OnChangeSuperGamer += (args) =>
        {
            Debug.Log(string.Format("���� ���� : {0} / �� ���� : {1}", args.OldSuperUserRecord.m_nickname, args.NewSuperUserRecord.m_nickname));
            // ȣ��Ʈ �缳��
            //SetSubHost(args.NewSuperUserRecord.m_sessionId);
            //if (isHost)
            //{
            //    // ���� ����ȣ��Ʈ�� �����Ǹ� �ٸ� ��� Ŭ���̾�Ʈ�� ��ũ�޽��� ����
            //    Invoke("SendGameSyncMessage", 1.0f);
            //}
        };
    }

    /*
   * ��Ī ��û�� ���� ���ϰ� (ȣ��Ǵ� ����)
   * ��Ī ��û �������� ��
   * ��Ī �������� ��
   * ��Ī ��û �������� ��
  */
    private void ProcessMatchMakingResponse(MatchMakingResponseEventArgs args)
    {
        string debugLog = string.Empty;
        bool isError = false;
        switch (args.ErrInfo)
        {
            case ErrorCode.Success:
                // ��Ī �������� ��
                debugLog = string.Format(SUCCESS_MATCHMAKE, args.Reason);
                //LobbyUI.GetInstance().MatchDoneCallback();
                ProcessMatchSuccess(args);
                break;
            case ErrorCode.Match_InProgress:
                // ��Ī ��û �������� �� or ��Ī ���� �� ��Ī ��û�� �õ����� ��

                // ��Ī ��û �������� ��
                if (args.Reason == string.Empty)
                {
                    debugLog = SUCCESS_REGIST_MATCHMAKE;

                    //LobbyUI.GetInstance().MatchRequestCallback(true);
                }
                break;
            case ErrorCode.Match_MatchMakingCanceled:
                // ��Ī ��û�� ��ҵǾ��� ��
                debugLog = string.Format(CANCEL_MATCHMAKE, args.Reason);

                //LobbyUI.GetInstance().MatchRequestCallback(false);
                break;
            case ErrorCode.Match_InvalidMatchType:
                isError = true;
                // ��ġ Ÿ���� �߸� �������� ��
                debugLog = string.Format(FAIL_REGIST_MATCHMAKE, INVAILD_MATCHTYPE);

                //LobbyUI.GetInstance().MatchRequestCallback(false);
                break;
            case ErrorCode.Match_InvalidModeType:
                isError = true;
                // ��ġ ��带 �߸� �������� ��
                debugLog = string.Format(FAIL_REGIST_MATCHMAKE, INVALID_MODETYPE);

                //LobbyUI.GetInstance().MatchRequestCallback(false);
                break;
            case ErrorCode.InvalidOperation:
                isError = true;
                // �߸��� ��û�� �������� ��
                debugLog = string.Format(INVALID_OPERATION, args.Reason);
                //LobbyUI.GetInstance().MatchRequestCallback(false);
                break;
            case ErrorCode.Match_Making_InvalidRoom:
                isError = true;
                // �߸��� ��û�� �������� ��
                debugLog = string.Format(INVALID_OPERATION, args.Reason);
                //LobbyUI.GetInstance().MatchRequestCallback(false);
                break;
            case ErrorCode.Exception:
                isError = true;
                // ��Ī �ǰ�, �������� �� ������ �� ���� �߻� �� exception�� ���ϵ�
                // �� ��� �ٽ� ��Ī ��û�ؾ� ��
                debugLog = string.Format(EXCEPTION_OCCUR, args.Reason);

                //LobbyUI.GetInstance().RequestMatch();
                break;
        }

        if (!debugLog.Equals(string.Empty))
        {
            Debug.Log(debugLog);
            if (isError == true)
            {
                //LobbyUI.GetInstance().SetErrorObject(debugLog);
            }
        }
    }

    // ��Ī �������� ��
    // �ΰ��� ������ �����ؾ� �Ѵ�.
    private void ProcessMatchSuccess(MatchMakingResponseEventArgs args)
    {
        ErrorInfo errorInfo;
        if (sessionIdList != null)
        {
            Debug.Log("���� ���� ���� ����");
            sessionIdList.Clear();
        }

        if (!Backend.Match.JoinGameServer(args.RoomInfo.m_inGameServerEndPoint.m_address, args.RoomInfo.m_inGameServerEndPoint.m_port, false, out errorInfo))
        {
            var debugLog = string.Format(FAIL_ACCESS_INGAME, errorInfo.ToString(), string.Empty);
            Debug.Log(debugLog);
        }
        // ���ڰ����� �ΰ��� ����ū�� �����صξ�� �Ѵ�.
        // �ΰ��� �������� �뿡 ������ �� �ʿ�
        // 1�� ���� ��� ������ �뿡 �������� ������ �ش� ���� �ı�ȴ�.
        isConnectInGameServer = true;
        isJoinGameRoom = false;
        isReconnectProcess = false;
        inGameRoomToken = args.RoomInfo.m_inGameRoomToken;
        isSandBoxGame = args.RoomInfo.m_enableSandbox;
        var info = GetMatchInfo(args.MatchCardIndate);
        if (info == null)
        {
            Debug.LogError("��ġ ������ �ҷ����� �� �����߽��ϴ�.");
            return;
        }

        nowMatchType = info.matchType;
        nowModeType = info.matchModeType;
        numOfClient = int.Parse(info.headCount);
    }

    public MatchInfo GetMatchInfo(string indate)
    {
        var result = matchInfos.FirstOrDefault(x => x.inDate == indate);
        if (result.Equals(default(MatchInfo)) == true)
        {
            return null;
        }
        return result;
    }


    private void ExceptionHandler()
    {
        // ���ܰ� �߻����� �� ȣ��
        Backend.Match.OnException += (e) =>
        {
            Debug.Log(e);
        };
    }

    private void ProcessMatchInGameSessionList(MatchInGameSessionListEventArgs args)
    {
        sessionIdList = new List<SessionId>();
        gameRecords = new Dictionary<SessionId, MatchUserGameRecord>();

        foreach (var record in args.GameRecords)
        {
            sessionIdList.Add(record.m_sessionId);
            gameRecords.Add(record.m_sessionId, record);
        }
        sessionIdList.Sort();
    }

    // Ŭ���̾�Ʈ ���� ���� �� ���ӿ� ���� ���ϰ�
    // Ŭ���̾�Ʈ�� ���� �뿡 ������ ������ ȣ���
    // ������ ���� ���� ���ŵ��� ����
    private void ProcessMatchInGameAccess(MatchInGameSessionEventArgs args)
    {
        if (isReconnectProcess)
        {
            // ������ ���μ��� �� ���
            // �� �޽����� ���ŵ��� �ʰ�, ���� ���ŵǾ ������
            Debug.Log("������ ���μ��� ������... ������ ���μ��������� ProcessMatchInGameAccess �޽����� ���ŵ��� �ʽ��ϴ�.\n" + args.ErrInfo);
            return;
        }

        Debug.Log(string.Format(SUCCESS_ACCESS_INGAME, args.ErrInfo));

        if (args.ErrInfo != ErrorCode.Success)
        {
            // ���� �� ���� ����
            var errorLog = string.Format(FAIL_ACCESS_INGAME, args.ErrInfo, args.Reason);
            Debug.Log(errorLog);
            isConnectInGameServer = false;
            Backend.Match.LeaveGameServer();
            return;
        }

        // ���� �� ���� ����
        // ���ڰ��� ��� ������ Ŭ���̾�Ʈ(����)�� ����ID�� ��Ī ����� ����ִ�.
        // ���� ������ �����Ǿ� ����ֱ� ������ �̹� ������ �����̸� �ǳʶڴ�.

        var record = args.GameRecord;
        Debug.Log(string.Format(string.Format("�ΰ��� ���� ���� ���� [{0}] : {1}", args.GameRecord.m_sessionId, args.GameRecord.m_nickname)));
        if (!sessionIdList.Contains(args.GameRecord.m_sessionId))
        {
            // ���� ����, ���� ��� ���� ����
            sessionIdList.Add(record.m_sessionId);
            gameRecords.Add(record.m_sessionId, record);

            Debug.Log(string.Format(NUM_INGAME_SESSION, sessionIdList.Count));
        }
    }

    private void ProcessAccessMatchMakingServer(ErrorInfo errInfo)
    {
        if (errInfo != ErrorInfo.Success)
        {
            // ���� ����
            isConnectMatchServer = false;
        }

        if (!isConnectMatchServer)
        {
            // ���� ����
            Debug.Log(errInfo.ToString());
        }
        else
        {
            //���� ����
            Debug.Log("���� ����!!");
        }
    }

    public bool CreateMatchRoom()
    {
        // ��û ������ ����Ǿ� ���� ������ ��Ī ���� ����
        if (!isConnectMatchServer)
        {
            Debug.Log("���� ����");
            JoinMatchServer();
            return false;
        }
        Debug.Log("�� ���� ��û�� ������ ����");
        Backend.Match.CreateMatchRoom();
        return true;
    }

    public void RequestMatchMaking()
    {
        // ��û ������ ����Ǿ� ���� ������ ��Ī ���� ����
        if (!isConnectMatchServer)
        {
            Debug.Log("��Ī���� ���� X");
            JoinMatchServer();
            return;
        }
        // ���� �ʱ�ȭ
        isConnectInGameServer = false;

        Backend.Match.RequestMatchMaking(MatchType.Point, MatchModeType.OneOnOne, "2022-01-10T09:52:09.526Z");
        if (isConnectInGameServer)
        {
            Backend.Match.LeaveGameServer(); //�ΰ��� ���� ���ӵǾ� ���� ��츦 ����� �ΰ��� ���� ���� ȣ��
        }

        //nowMatchType = matchInfos[index].matchType;
        //nowModeType = matchInfos[index].matchModeType;
        //numOfClient = int.Parse(matchInfos[index].headCount);
    }

    public void JoinMatchServer()
    {
        if (isConnectMatchServer)
        {
            return;
        }
        ErrorInfo errorInfo;
        isConnectMatchServer = true;
        if (!Backend.Match.JoinMatchMakingServer(out errorInfo))
        {
            Debug.Log(errorInfo.ToString());
        }
    }

    // ��Ī ���� ��������
    public void LeaveMatchServer()
    {
        isConnectMatchServer = false;
        Backend.Match.LeaveMatchMakingServer();
    }

    public void AddMsgToLocalQueue(KeyMessage message)
    {
        // ���� ť�� �޽��� �߰�
        if (isHost == false || localQueue == null)
        {
            return;
        }

        localQueue.Enqueue(message);
    }

    // ������ ������ ��Ŷ ����
    // ���������� �� ��Ŷ�� �޾� ��� Ŭ���̾�Ʈ(��Ŷ ���� Ŭ���̾�Ʈ ����)�� ��ε�ĳ���� ���ش�.
    public void SendDataToInGame<T>(T msg)
    {
        var byteArray = DataParser.DataToJsonData<T>(msg);
        Backend.Match.SendDataToInGameRoom(byteArray);
    }


    public void SetHostSession(SessionId host)
    {
        hostSession = host;
    }

    // Update is called once per frame
    void Update()
    {
        if (isConnectInGameServer || isConnectMatchServer)
        {
            Backend.Match.Poll();

            // ȣ��Ʈ�� ��� ���� ť�� ����
            // ť�� �ִ� ��Ŷ�� ���ÿ��� ó��
            if (localQueue != null)
            {
                while (localQueue.Count > 0)
                {
                    var msg = localQueue.Dequeue();
                    WorldManager.instance.OnRecieveForLocal(msg);
                }
            }
        }
    }
}

public class ServerInfo
{
    public string host;
    public ushort port;
    public string roomToken;
}

public class MatchRecord
{
    public MatchType matchType;
    public MatchModeType modeType;
    public string matchTitle;
    public string score = "-";
    public int win = -1;
    public int numOfMatch = 0;
    public double winRate = 0;
}
