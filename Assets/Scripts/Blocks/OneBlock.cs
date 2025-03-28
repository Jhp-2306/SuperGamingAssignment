using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OneBlock : MonoBehaviour, IBlock
{
    public BlockColors Color;
    Vector3 pevpos;
    private void Start()
    {
        pevpos = transform.localPosition;
        UpdateOccupiedCells(pevpos, true);
        updatecolor(false);
    }

    public void Move(Vector3 pos)
    {
        Debug.Log(Vector3.Distance(pevpos, pos));
        if (pevpos.x == pos.x||pevpos.z==pos.z&& Vector3.Distance(pevpos, pos)==1)
        {
            this.transform.position = pos;
            pevpos = pos;
        }
    }
    public List<Vector3> GetBlockCells(Vector3 centerPos)
    {
        var cells = new List<Vector3>();
        cells.Add(centerPos);
        return cells;
    }

    public BlockColors GetColor() => Color;

    public void updatecolor(bool isremove)
    {
        List<Vector3> cells = GetBlockCells(this.gameObject.transform.localPosition);
        foreach (var cell in cells)
        {
            GridMaker.Instance.UpdateCellColor(cell,isremove? BlockColors.none:Color);
        }
    }
    private void UpdateOccupiedCells(Vector3 centerPos, bool isOccupied)
    {
        List<Vector3> cells = GetBlockCells(centerPos);
        foreach (var cell in cells)
        {
            GridMaker.Instance.UpdateCellOccupied(cell, isOccupied);
        }
    }
}
public interface IBlock
{
    public void Move(Vector3 pos);
    public List<Vector3> GetBlockCells(Vector3 centerPos);
    public void updatecolor(bool isremove);
    public BlockColors GetColor();
}
