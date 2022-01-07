using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BallCS : MonoBehaviour
{
    public Vector2 startPos;

    public AudioClip[] ballSound; //0 : 일반블럭 | 1 : 여러방 블럭 | 2 : 돌블럭 | 3 : 벽 

    AudioSource BallAS;

    GameObject tempObj = null;

    private int BallSoundNum = 0;

    private void Start()
    {
        BallAS = this.GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        startPos = transform.position;
    }

    private void FixedUpdate()
    {
        if (BlockManager.BInstance.StartCheck)
        {
            this.transform.transform.position += this.transform.up * (BlockManager.BInstance.BallPower * BlockManager.BInstance.AddBallPower) * Time.deltaTime;
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (BlockManager.BInstance.StartCheck)
        {
            BallSoundNum = 3;
            Vector2 hitPos = collision.contacts[0].point;

            Vector3 incomingVec = hitPos - startPos;
            Vector3 reflectVec = Vector3.Reflect(incomingVec, collision.contacts[0].normal);

            this.transform.up = reflectVec + new Vector3(0, Random.Range(-0.2f, 0.2f));
            startPos = transform.position;

            if (collision.gameObject.GetComponent<BlockCS>())
            {
                Debug.Log("부셔져라 블록블록");
                BlockCS BObj = collision.gameObject.GetComponent<BlockCS>();
                if (BObj.bState != BlockManager.BlockState.Wall)
                {
                    BObj.BlockHP--;
                    if (BObj.BlockHP <= 0)
                    {
                        BallSoundNum = 0;
                        switch (BObj.bState)
                        {
                            case BlockManager.BlockState.Fire:
                                BObj.bState = BlockManager.BlockState.None;
                                var fire = FireObjectPool.GetObject();
                                fire.GetComponent<Image>().rectTransform.sizeDelta = BObj.GetComponent<Image>().rectTransform.sizeDelta;
                                fire.transform.SetParent(BlockManager.BInstance.BlockSpace.transform);
                                fire.transform.localPosition = BObj.transform.localPosition;
                                fire.transform.localScale = new Vector3(1, 1, 1);
                                fire.GetComponent<BoxCollider2D>().size = BObj.GetComponent<BoxCollider2D>().size;
                                fire.GetComponent<FireCS>().FireBlock();
                                break;
                            case BlockManager.BlockState.Item:
                                var Item = ItemPool.GetObject();
                                Item.GetComponent<ItemCS>().IState = BObj.IState;
                                Item.GetComponent<Image>().sprite = BlockManager.BInstance.ItemImageSet[(int)BObj.IState - 1];
                                Item.transform.SetParent(BlockManager.BInstance.ItemParent.transform);
                                Item.transform.localPosition = BObj.transform.localPosition;
                                Item.transform.localScale = new Vector3(1, 1, 1);
                                break;
                            case BlockManager.BlockState.Speed:
                                BlockManager.BInstance.AddBallPower += 0.2f;
                                break;
                        }

                        if (BObj.gameObject.active)
                        {
                            ObjectPool.ReturnObject(BObj);
                            BlockManager.BInstance.BlockCount--;
                            if (BlockManager.BInstance.BlockCount <= 0)
                            {
                                BlockManager.BInstance.GameEnd(true);
                            }
                        }
                    }
                    else
                    {
                        BallSoundNum = 1;
                    }
                }
                else
                {
                    BallSoundNum = 2;
                }
            }
            else if (collision.gameObject.CompareTag("End"))
            {
                if(this.transform.parent == BlockManager.BInstance.SubBallParent)
                {
                    BallPool.ReturnObject(this.GetComponent<BallCS>());
                }
                else
                {
                    this.gameObject.SetActive(false);
                }

                BlockManager.BInstance.BallCount--;

                if (BlockManager.BInstance.BallCount <= 0)
                {
                    BlockManager.BInstance.GameEnd(false);
                }
            }

            BallAS.clip = ballSound[BallSoundNum];
            BallAS.Play();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(tempObj == collision.gameObject)
        {
            this.transform.up -= this.transform.up;
            Debug.Log("왜~ 안튕겨~~");
        }

        tempObj = collision.gameObject;
    }

}
