using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneXThreeBlock : MonoBehaviour,IBlock
{
    private Vector3 prevPos;
    public BlockColors Color;
    private void Start()
    {
        prevPos = transform.localPosition;
        UpdateOccupiedCells(prevPos, true);
        updatecolor(false);
    }

    public void Move(Vector3 pos)
    {
        Debug.Log("Moving in 1x3 grid");
        if ((prevPos.x == pos.x || prevPos.z == pos.z && Vector3.Distance(prevPos, pos) == 1))
        {
            Debug.Log("Moving in if 1x3 grid");
            transform.position = pos;
            prevPos = pos;
        }
    }

    public List<Vector3> GetBlockCells(Vector3 centerPos)
    {
        List<Vector3> cells = new List<Vector3>();
        cells.Add(centerPos);
        cells.Add(centerPos + new Vector3(0, 0, 1));
        cells.Add(centerPos + new Vector3(0, 0, -1));
        return cells;
    }

    private void UpdateOccupiedCells(Vector3 centerPos, bool isOccupied)
    {
        List<Vector3> cells = GetBlockCells(centerPos);
        foreach (var cell in cells)
        {
            GridMaker.Instance.UpdateCellOccupied(cell, isOccupied);
        }
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
