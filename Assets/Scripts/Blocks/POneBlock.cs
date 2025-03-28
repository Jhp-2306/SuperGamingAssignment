using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class POneBlock : MonoBehaviour, IBlock
{
    public GameObject LSide;
    public GameObject RSide;
    public GameObject TSide;
    public GameObject BSide;
    Vector3 pevpos;
    public BlockColors Color;
    private void Start()
    {
        //var pos = this.transform.localPosition;
        //BlockPuzzleManager.Instance.UpdateCells(pos);
        pevpos = transform.localPosition;
    }

    public void Move(Vector3 pos)
    {
        //Debug.Log(Vector3.Distance(pevpos, pos));
        //if (pevpos.x == pos.x||pevpos.z==pos.z)
        //{
        //    this.transform.position = pos;
        //    pevpos = pos;
        //}
    }

    public List<Vector3> GetBlockCells(Vector3 centerPos)
    {
        throw new System.NotImplementedException();
    }
    public BlockColors GetColor() => Color;
    public void updatecolor(bool isremove)
    {
        List<Vector3> cells = GetBlockCells(this.gameObject.transform.localPosition);
        foreach (var cell in cells)
        {
            GridMaker.Instance.UpdateCellColor(cell, isremove ? BlockColors.none : Color);
        }
    }
}

