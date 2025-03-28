using System.Collections;
using System.Collections.Generic;
using Util;
using UnityEngine;
using Unity.Burst.CompilerServices;

public class PBlockPuzzleManager :SingletonRef<PBlockPuzzleManager>
{
    RaycastHit hit;
    public LayerMask layerMask;
    //public GridMaker GridMaker;
    public GameObject SelectedGameObj;
    Vector3 MousePosStart;
    Vector3 MousePos;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //Started moving
            if (SelectedGameObj == null)
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, layerMask))
                    if (hit.collider.GetComponent<IBlock>() != null)
                        SelectedGameObj = hit.collider.gameObject;
                var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                pos = GridMaker.Instance.transform.InverseTransformPoint(pos);
                //pos = new Vector3(RoundWithDeadzone(pos.x), 0, RoundWithDeadzone(pos.z));
                Debug.Log("pos" + pos + GridMaker.Instance.processCoords(pos));
                GridMaker.Instance.UpdateCellOccupied(pos, false);
                MousePosStart = pos;
                MousePos = pos;
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            //Moving Ended
            if (SelectedGameObj != null) SelectedGameObj = null;
            GridMaker.Instance.UpdateCellOccupied(MousePosStart, true);
            MousePosStart = Vector3.zero;
            MousePos = Vector3.zero;
        }
        if (SelectedGameObj != null)
        {
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos = GridMaker.Instance.transform.InverseTransformPoint(pos);
            //pos = new Vector3(RoundWithDeadzone(pos.x), 0, RoundWithDeadzone(pos.z));
            MousePos = pos;
        }
        CheckDirection();

    }
    private void LateUpdate()
    {
        if (SelectedGameObj != null)
        {
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos = GridMaker.Instance.transform.InverseTransformPoint(pos);
            pos = new Vector3((int)pos.x, 0, (int)pos.z);
            if (GridMaker.Instance.processCoords(pos))
            {
                SelectedGameObj.GetComponent<IBlock>().Move(pos);
                MousePosStart = pos;
            }
        }
    }
    void CheckDirection()
    {
        Vector3 delta = MousePos - MousePosStart;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.z))
        {
            if (delta.x > 0)
                Debug.Log("Moving Right");
            else
                Debug.Log("Moving Left");
        }
        else if (Mathf.Abs(delta.x) < Mathf.Abs(delta.z))
        {
            if (delta.z > 0)
                Debug.Log("Moving Up");
            else
                Debug.Log("Moving Down");
        }
    }
}
