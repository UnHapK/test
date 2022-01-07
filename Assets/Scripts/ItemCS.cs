using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCS : MonoBehaviour
{
    public BlockManager.ItemState IState = BlockManager.ItemState.None;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<BarMoveCS>())
        {
            collision.gameObject.GetComponent<BarMoveCS>().ItemGetSoundPlay();

            switch (IState)
            {
                case BlockManager.ItemState.BarSize:
                    BlockManager.BInstance.BarSizeUp();
                    break;
                case BlockManager.ItemState.PlusBall:
                    BlockManager.BInstance.CreateSubBall();
                    break;


#pragma warning disable CS0162 // ������ �� ���� �ڵ尡 �ֽ��ϴ�.
                    ItemPool.ReturnObject(this.GetComponent<ItemCS>());
#pragma warning restore CS0162 // ������ �� ���� �ڵ尡 �ֽ��ϴ�.
            }
        }
        else if (collision.gameObject.CompareTag("End"))
        {
            ItemPool.ReturnObject(this.GetComponent<ItemCS>());
        }
    }
}
