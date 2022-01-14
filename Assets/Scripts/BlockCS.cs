using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockCS : MonoBehaviour
{
    public BlockManager.BlockState bState = BlockManager.BlockState.None;
    public BlockManager.ItemState IState = BlockManager.ItemState.None;

    public int BlockHP = 0;

    public void BlockStateSetting(int i)
    {
        bState = (BlockManager.BlockState)i;

        BlockManager.BInstance.BlockCount++;
        this.gameObject.GetComponent<Image>().color = BlockManager.BInstance.BlockColor[i];

        switch(bState)
        {
            case BlockManager.BlockState.None:
                BlockManager.BInstance.BlockCount--;
                ObjectPool.ReturnObject(this);
                break;
            case BlockManager.BlockState.ThreeHp:
                BlockHP = 3;
                break;
            case BlockManager.BlockState.FiveHp:
                BlockHP = 5;
                break;
            case BlockManager.BlockState.Wall:
                BlockManager.BInstance.BlockCount--;
                BlockHP = 1;
                break;
            case BlockManager.BlockState.Item:
                BlockManager.BInstance.ItemObj.Add(this.gameObject);
                BlockHP = 1;
                break;
            default:
                BlockHP = 1;
                break;
        }
    }
}
