using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FireCS : MonoBehaviour
{
    public void FireBlock()
    {
        this.GetComponent<AudioSource>().Play();

        this.GetComponent<BoxCollider2D>().enabled = true;
        Collider2D[] colls;

        colls = Physics2D.OverlapBoxAll(transform.position, new Vector2(2, 2), 0);

        foreach (Collider2D coll in colls)
        {
            if (coll.gameObject != this.gameObject)
            {
                if (coll.gameObject.GetComponent<BlockCS>())
                {
                    BlockCS BObj = coll.gameObject.GetComponent<BlockCS>();
                    if (BObj.bState != BlockManager.BlockState.Wall)
                    {
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

#pragma warning disable CS0618 // 형식 또는 멤버는 사용되지 않습니다.
                        if (BObj.gameObject.active)
#pragma warning restore CS0618 // 형식 또는 멤버는 사용되지 않습니다.
                        {
                            ObjectPool.ReturnObject(BObj);
                            BlockManager.BInstance.BlockCount--;
                            if (BlockManager.BInstance.BlockCount <= 0)
                            {
                                BlockManager.BInstance.GameEnd(true);
                            }
                        }
                    }
                }
            }
        }

        StartCoroutine(FireObjectReturn());
    }

    IEnumerator FireObjectReturn()
    {
        this.GetComponent<BoxCollider2D>().enabled = false;
        yield return new WaitForSeconds(1f);
        FireObjectPool.ReturnObject(this.gameObject.GetComponent<FireCS>());
    }
}
