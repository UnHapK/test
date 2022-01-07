using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using LitJson;
using UnityEngine.EventSystems;

public class BlockManager : MonoBehaviour
{
    #region public
    public static BlockManager BInstance = null;

    [Header("Commons")]
    public GameObject StartBtnObj;
    public bool StartCheck = false;

    public GameObject BallObj;
    public float BallPower = 5f;
    public int BallCount;
    public GameObject SubBallParent;

    public GameObject StickObj;
    public int StickRotateNum = 1;

    public GameObject BarObj;

    public float AddBallPower = 1f;

    public Text EndText;
    public Animation EndAnim;

    #region Option
    [Header("Option")]
    public GameObject OptionObj;

    public AudioMixer masterMixer;
    public Slider BGMSlider, EffectSlider;
    public Image BGMSoundBtn, EffectSoundBtn;

    public Sprite SoundOffImg, SoundOnImg;
    #endregion

    #region Block
    public enum BlockState
    {
        None = 0,
        Idle = 1,
        Fire = 2,
        ThreeHp = 3,
        FiveHp = 4,
        Item = 5,
        Speed = 6,
        Wall = 7
    }

    [Header("Block")]
    public Color[] BlockColor;

    public int BlockCount = 0;

    public Image BlockSpace;

    public TextAsset BlockDataList;
    public int BlockSpaceWidth = 0;
    public int BlockSpaceHeight = 0;
    #endregion

    #region Item
    public enum ItemState
    {
        None = 0,
        PlusBall = 1,
        BarSize = 2
    }

    [Header("Item")]
    public GameObject ItemParent;

    public Sprite[] ItemImageSet;

    public List<GameObject> ItemObj = new List<GameObject>();
    #endregion

    #endregion

    #region private
    Vector2 BallPos = Vector2.zero;
    Vector2 BarPos = Vector2.zero;

    private bool StickRotateCheck = false;

    JsonData BlockData;

    float BlockWidth, BlockHeight;
    #endregion

    #region Function

    private void Awake()
    {
        BInstance = this;

        Screen.SetResolution(1920, 1080, false);
    }

    // Start is called before the first frame update
    void Start()
    {
        BlockData = JsonMapper.ToObject(BlockDataList.text);
        BallPos = BallObj.transform.position;
        BarPos = BarObj.transform.position;

        BGMSoundBtn.sprite = SoundOnImg;
        EffectSoundBtn.sprite = SoundOnImg;

        if (PlayerPrefs.HasKey("BGM"))
        {
            float BGMvalue = PlayerPrefs.GetFloat("BGM");
            BGMSlider.value = BGMvalue;
            masterMixer.SetFloat("BGM", BGMvalue);

            if(BGMvalue <= -40f)
                BGMSoundBtn.sprite = SoundOffImg;
        }
        else
        {
            BGMSlider.value = 0;
            masterMixer.SetFloat("BGM", 0);
        }

        if (PlayerPrefs.HasKey("Effect"))
        {
            float Effectvalue = PlayerPrefs.GetFloat("Effect");
            EffectSlider.value = Effectvalue;
            masterMixer.SetFloat("Effect", Effectvalue);

            if (Effectvalue <= -40f)
                EffectSoundBtn.sprite = SoundOffImg;
        }
        else
        {
            EffectSlider.value = 0;
            masterMixer.SetFloat("Effect", 0);
        }

        CreateBlock();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            ResetGame();
        
    }

    void ResetGame()
    {
        StartCheck = false;
        BlockCount = 0;

        int tempItem = ItemParent.transform.childCount;

        if (tempItem != 0)
        {
            for (int i = 0; i < tempItem; i++)
            {
                ItemPool.ReturnObject(ItemParent.transform.GetChild(0).GetComponent<ItemCS>());
            }
        }

        ItemObj.Clear();

        int tempNum = BlockSpace.transform.childCount;

        if (tempNum != 0)
        {
            for (int i = 0; i < tempNum; i++)
            {
                if(BlockSpace.transform.GetChild(0).GetComponent<BlockCS>())
                    ObjectPool.ReturnObject(BlockSpace.transform.GetChild(0).GetComponent<BlockCS>());
            }
        }

        BarObj.transform.position = BarPos;
        BarObj.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(426, 74.5f);
        BarObj.GetComponent<CapsuleCollider2D>().size = new Vector2(350, 50);

        //Stick
        StickObj.transform.rotation = Quaternion.Euler(Vector2.zero);
        StickObj.transform.GetChild(0).gameObject.SetActive(true);
        StartBtnObj.SetActive(true);

        //Ball
        BallObj.transform.position = BallPos;
        BallObj.transform.rotation = Quaternion.Euler(Vector2.zero);
        AddBallPower = 1f;

        BallCount = 1;

        BallObj.SetActive(true);

        int tempBall = SubBallParent.transform.childCount;

        if (tempBall != 0)
        {
            for (int i = 0; i < tempBall; i++)
            {
                BallPool.ReturnObject(SubBallParent.transform.GetChild(0).GetComponent<BallCS>());
            }
        }

        CreateBlock();
    }

    /// <summary>
    /// ClearCheck = true : GameClear
    /// ClearCheck = false : GameOver
    /// </summary>
    /// <param name="ClearCheck"></param>
    public void GameEnd(bool ClearCheck)
    {
        StartCheck = false;

        if(ClearCheck)
        {
            EndText.text = "GAME CLEAR";
        }
        else
        {
            EndText.text = "GAME OVER";
        }

        EndAnim.Play();
        Invoke("ResetGame", 5f);
    }

    public void CreateBlock()
    {
        int r = Random.Range(0, BlockData.Count);
        BlockSpaceWidth = int.Parse(BlockData[r]["width"].ToString());
        BlockSpaceHeight = int.Parse(BlockData[r]["height"].ToString());

        BlockWidth = BlockSpace.rectTransform.sizeDelta.x / BlockSpaceWidth;
        BlockHeight = BlockSpace.rectTransform.sizeDelta.y / BlockSpaceHeight;

        float BlockPosX = (BlockWidth / 2) - (BlockSpace.rectTransform.sizeDelta.x / 2);
        float BlockPosY =  (BlockSpace.rectTransform.sizeDelta.y / 2) - (BlockHeight / 2);

        for (int i = 0; i < BlockSpaceHeight; i++)
        {
            char[] BlockNum = BlockData[r]["BlockData"][i].ToString().ToCharArray();
            for (int j = 0; j < BlockSpaceWidth; j++)
            {
                var Block = ObjectPool.GetObject();
                Block.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(BlockWidth, BlockHeight);
                Block.transform.SetParent(BlockSpace.transform);
                Block.transform.localPosition = new Vector2(BlockPosX + (BlockWidth * j), BlockPosY - (BlockHeight * i));
                Block.transform.localScale = new Vector3(1, 1, 1);
                Block.GetComponent<BoxCollider2D>().size = new Vector2(BlockWidth, BlockHeight);
                int tempNum = int.Parse(BlockNum[j].ToString());
                Block.GetComponent<BlockCS>().BlockStateSetting(tempNum);
            }
        }

        ItemObj = ShuffleList(ItemObj);

        int ListHalfNum = ItemObj.Count / 2;

        for(int i = 0; i < ItemObj.Count; i++)
        {
            if(i < ListHalfNum)
            {
                ItemObj[i].GetComponent<BlockCS>().IState = ItemState.PlusBall;
            }
            else
            {
                ItemObj[i].GetComponent<BlockCS>().IState = ItemState.BarSize;
            }
        }
    }

    public void CreateSubBall()
    {
        var Ball = BallPool.GetObject();
        Ball.transform.SetParent(SubBallParent.transform);
        Ball.transform.localScale = new Vector3(1, 1, 1);
        Ball.transform.localPosition = BallObj.transform.localPosition;
        Ball.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0, 360));

        Ball.GetComponent<BallCS>().startPos = Ball.transform.position;
        BallCount++;
    }

    public void BarSizeUp()
    {
        BarObj.GetComponent<Image>().rectTransform.sizeDelta += new Vector2(150, 0);
        BarObj.GetComponent<CapsuleCollider2D>().size += new Vector2(130, 0);
    }

    private List<T> ShuffleList<T>(List<T> list)
    {
        int random1, random2;
        T temp;

        for (int i = 0; i < list.Count; ++i)
        {
            random1 = Random.Range(0, list.Count);
            random2 = Random.Range(0, list.Count);

            temp = list[random1];
            list[random1] = list[random2];
            list[random2] = temp;
        }

        return list;
    }

    #endregion

    #region Button Event

    public void OnStartBtnClick()
    {
        if (StickRotateCheck)
        {
            StickRotateCheck = false;
            StickObj.transform.GetChild(0).gameObject.SetActive(false);
            StartBtnObj.SetActive(false);
            StartCheck = true;

            BarObj.transform.position = BarPos;
        }
    }

    public void OnStartBtnDown()
    {
        StickRotateCheck = true;
        StartCoroutine(RotateStick());
    }

    /// <summary>
    /// b : true = BGM | false = Effect
    /// </summary>
    /// <param name="b"></param>
    public void AudioSlideControl(bool b)
    {
        float sound;
        string audioname;
        if (b)
        {
            sound = BGMSlider.value;
            audioname = "BGM";
        }
        else
        {
            sound = EffectSlider.value;
            audioname = "Effect";
        }

        if (sound == -40f)
        {
            masterMixer.SetFloat(audioname, -80);
            PlayerPrefs.SetFloat(audioname, -80);
            if (b)
                BGMSoundBtn.sprite = SoundOffImg;
            else
                EffectSoundBtn.sprite = SoundOffImg;
        }
        else
        {
            masterMixer.SetFloat(audioname, sound);
            PlayerPrefs.SetFloat(audioname, sound);
            if (b)
                BGMSoundBtn.sprite = SoundOnImg;
            else
                EffectSoundBtn.sprite = SoundOnImg;
        }
    }

    /// <summary>
    /// b : true = BGM | false = Effect
    /// </summary>
    /// <param name="b"></param>
    public void AudioBtnClick(bool b)
    {
        string audioname;
        if (b)
        {
            audioname = "BGM";
            if (BGMSlider.value != -40f)
            {
                masterMixer.SetFloat(audioname, -80);
                PlayerPrefs.SetFloat(audioname, -80);
                BGMSlider.value = -40f;
                BGMSoundBtn.sprite = SoundOffImg;
            }
            else
            {
                masterMixer.SetFloat(audioname, -20);
                PlayerPrefs.SetFloat(audioname, -20);
                BGMSlider.value = -20f;
                BGMSoundBtn.sprite = SoundOnImg;
            }
        }
        else
        {
            audioname = "Effect";
            if (EffectSlider.value != -40f)
            {
                masterMixer.SetFloat(audioname, -80);
                PlayerPrefs.SetFloat(audioname, -80);
                EffectSlider.value = -40f;
                EffectSoundBtn.sprite = SoundOffImg;
            }
            else
            {
                masterMixer.SetFloat(audioname, -20);
                PlayerPrefs.SetFloat(audioname, -20);
                EffectSlider.value = -20f;
                EffectSoundBtn.sprite = SoundOnImg;
            }
        }
    }

    [System.Obsolete]
    public void OptionBtnClick()
    {
        if(OptionObj.active)
        {
            OptionObj.SetActive(false);
            if(StartBtnObj.active)
                StartCheck = false;
            else
                StartCheck = true;
        }
        else
        {
            OptionObj.SetActive(true);
            StartCheck = false;
        }
    }

    public void SoundPlay()
    {
        EventSystem.current.currentSelectedGameObject.gameObject.GetComponent<AudioSource>().Play();
    }

    #endregion

    #region Coroutine

    IEnumerator RotateStick()
    {
        if(StickRotateCheck)
        {
            float timer = 1.5f;
            StickObj.GetComponent<AudioSource>().Play();
            while (StickRotateCheck)
            {
                timer -= Time.deltaTime;
                StickObj.transform.rotation = Quaternion.Slerp(StickObj.transform.rotation, Quaternion.Euler(0, 0, StickRotateNum), Time.deltaTime);
                yield return new WaitForEndOfFrame();

                if(timer <= 0)
                {
                    StickObj.GetComponent<AudioSource>().Play();
                    timer = 1.5f;
                    StickRotateNum *= -1;
                }
            }
        }
    }

    #endregion
}
