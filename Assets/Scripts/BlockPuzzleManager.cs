using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public enum BlockColors
{
    none,
    Red,
    Blue,
    Purple,
    Yellow, Black
}

public class BlockPuzzleManager : MonoBehaviour
{

    public class DoorMatcher
    {
        List<Vector2> doorIdx;
        List<Vector2> doorTargetIdx;
        Dictionary<Vector2, Walls> doorcomp;
        public BlockColors myColor;
        int spawnValue = 0;

        public int SpawnValue { get => spawnValue; set { spawnValue = value; } }
        public DoorMatcher(List<Vector2> _doorIdx, List<Vector2> _doorTargetIdx, BlockColors _myColor)
        {
            this.doorIdx = _doorIdx;
            this.doorTargetIdx = _doorTargetIdx;
            myColor = _myColor;
            doorcomp = new Dictionary<Vector2, Walls>();
        }
        public void AssignMyDoors()
        {
            if (doorIdx.Count == doorTargetIdx.Count)
                for (int i = 0; i < doorIdx.Count; i++)
                {
                    Debug.Log("Assign the door" + doorIdx[i]);
                    //var temp = SetupWalls(doorIdx[i], doorTargetIdx[i], i == (int)doorIdx.Count / 2);
                    var temp = GridMaker.Instance.Walls[doorIdx[i]].GetComponent<Walls>();
                    temp.SetWalls(doorIdx[i], myColor, doorTargetIdx[i], spawnValue, i == (int)doorIdx.Count / 2);
                    

                    doorcomp.Add(doorIdx[i], temp);
                }
        }
        public void UpdateMyDoors()
        {
            foreach (var door in doorcomp.Values)
            {
                door.updateValueAndTextUI(spawnValue);
            }
        }
        public Walls SetupWalls(Vector2 doorIdx, Vector2 doorTargetIdx, bool ismid)
        {
            var temp = GridMaker.Instance.Walls[doorIdx].GetComponent<Walls>();
            temp.SetWalls(doorIdx, myColor, doorTargetIdx, spawnValue, ismid);
            return temp;
        }
        public bool isSpawnerActive()
        {
            foreach (var t in doorcomp.Values)
            {
                if (!t.isValueMatch) return false;
            }
            return true;
        }
    }
    public static BlockPuzzleManager Instance;
    RaycastHit hit;
    public LayerMask layerMask;
    public GameObject SelectedGameObj;
    Vector3 MousePosStart;
    Vector3 MousePos;
    public Material Red, Blue, Purple, Yellow, Black;
    public int SpawnRate;
    public GameObject Tower;
    public TextMeshProUGUI SpawnRatetxt;
    public TextMeshProUGUI Timertxt;
    public TextMeshProUGUI SwapTimertxt;
    public TextMeshProUGUI GamestateTxt;
    public GameObject GameStateDeclaration;
    float timerSec, maxTimerSec = 90, swapTimerSec, maxSwapTimerSec = 15;

    /* Door Coords
Y(2,7)(3,7)
T(2,6)(3,6)

R(5,0)(6,0)
T(5,1)(6,1)

P(0,1)(0,2)(0,3)
T(1,1)(1,2)(1,3)

R(0,6)
T(1,6)

B(7,5)
T(6,5)
     */
    public List<DoorMatcher> DoorMatcherList = new List<DoorMatcher>()
   {
       new DoorMatcher(
           new List<Vector2>()
       {
           new Vector2 (2,7),
           new Vector2 (3,7)
       },new List<Vector2>()
       {
           new Vector2 (2,6),
           new Vector2 (3,6)
       },BlockColors.Yellow),
       new DoorMatcher(
           new List<Vector2>()
       {
           new Vector2 (5,0),
           new Vector2 (6,0)
       },new List<Vector2>()
       {
           new Vector2 (5,1),
           new Vector2 (6,1)
       },BlockColors.Red),
       new DoorMatcher(
           new List<Vector2>()
       {
           new Vector2 (0,1),
           new Vector2 (0,2),
           new Vector2 (0,3)
       },new List<Vector2>()
       {
           new Vector2 (1,1),
           new Vector2 (1,2),
           new Vector2 (1,3)
       },BlockColors.Purple),
       new DoorMatcher(
           new List<Vector2>()
       {
           new Vector2 (0,6),

       },new List<Vector2>()
       {
           new Vector2 (1,6),
       },BlockColors.Red),
       new DoorMatcher(
           new List<Vector2>()
       {
           new Vector2 (7,5),

       },new List<Vector2>()
       {
           new Vector2 (6,5),
       },BlockColors.Blue),
   };

    public bool isGamePaused;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this.gameObject);
        }
    }
    private void Start()
    {
        AssignWalls();
        timerSec = maxTimerSec;
        swapTimerSec = maxSwapTimerSec;
        isGamePaused = false;
    }
    private void Update()
    {
        if (isGamePaused) return;
        timerSec -= Time.deltaTime;
        swapTimerSec -= Time.deltaTime;
        Timertxt.text = $"{(int)(timerSec/60)}:{(timerSec % 60).ToString("f0")}";
        SwapTimertxt.text = $"door swaping in {(swapTimerSec / 60).ToString("f0")}:{(swapTimerSec % 60).ToString("f0")}";
        if (swapTimerSec < 0)
        {
            swapTimerSec = maxSwapTimerSec;
            SwapDoorValues();
        }
        checkwinningcondition();
        Physics.SyncTransforms();
        if (Input.GetMouseButtonDown(0))
        {
            //Started moving
            if (SelectedGameObj == null)
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, layerMask))
                    if (hit.collider.GetComponent<IBlock>() != null)
                    {
                        SelectedGameObj = hit.collider.gameObject;
                        var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        pos = GridMaker.Instance.transform.InverseTransformPoint(pos);
                        pos = new Vector3(RoundWithDeadzone(pos.x), 0, RoundWithDeadzone(pos.z));
                        SelectedGameObj.GetComponent<IBlock>().updatecolor(true);
                        GridMaker.Instance.UpdateCellOccupiedList(hit.collider.GetComponent<IBlock>().GetBlockCells(SelectedGameObj.transform.position), false);
                        MousePosStart = pos;
                    }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            //Moving Ended
            if (SelectedGameObj != null)
            {
                SelectedGameObj.GetComponent<IBlock>().updatecolor(false);
                GridMaker.Instance.UpdateIsOccupied();
                SelectedGameObj = null;
            }
            MousePosStart = Vector3.zero;
            MousePos = Vector3.zero;
        }
        if (SelectedGameObj != null)
        {
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos = GridMaker.Instance.transform.InverseTransformPoint(pos);
            pos = new Vector3(RoundWithDeadzone(pos.x), 0, RoundWithDeadzone(pos.z));
            MousePos = pos;
        }
        CheckSpawnRate();
    }
    private void LateUpdate()
    {
        if (isGamePaused) return;
        if (SelectedGameObj != null)
        {
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos = GridMaker.Instance.transform.InverseTransformPoint(pos);
            pos = new Vector3(RoundWithDeadzone(pos.x), 0, RoundWithDeadzone(pos.z));
            Debug.Log("pos" + pos + GridMaker.Instance.ProcessCoordsList(hit.collider.GetComponent<IBlock>().GetBlockCells(pos)));
            if (GridMaker.Instance.ProcessCoordsList(SelectedGameObj.GetComponent<IBlock>().GetBlockCells(pos)))
            {
                SelectedGameObj.GetComponent<IBlock>().Move(pos);
                MousePosStart = pos;
            }
        }

    }
    public void AssignWalls()
    {
        foreach (var t in DoorMatcherList)
        {
            t.SpawnValue = Random.Range(-10, 31);
            t.AssignMyDoors();
        }
    }
    public int RoundWithDeadzone(float value)
    {
        int baseValue = Mathf.FloorToInt(value);
        float decimalPart = value - baseValue;

        if (decimalPart < 0.2f)
        {
            return baseValue;
        }
        else if (decimalPart > 0.8)
        {
            return baseValue + 1;
        }
        else
        {
            return baseValue;
        }
    }
    public Material GetMaterialFromBlockColor(BlockColors blockColors)
    {
        switch (blockColors)
        {
            case BlockColors.Red: return Red;
            case BlockColors.Blue: return Blue;
            case BlockColors.Purple: return Purple;
            case BlockColors.Yellow: return Yellow;
            case BlockColors.Black: return Black;
            default: return Black;
        }

    }

    public void CheckSpawnRate()
    {
        SpawnRate = 0;
        foreach (var t in DoorMatcherList)
        {
            if (t.isSpawnerActive())
            {
                SpawnRate += t.SpawnValue;
            }
        }
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = new EntityQueryBuilder(Allocator.Temp).WithAll<Spawner>().Build(manager);

        var _spawnerarr = query.ToComponentDataArray<Spawner>(Allocator.Temp);
        var _spawneentityrarr = query.ToEntityArray(Allocator.Temp);
        for (int i = 0; i < _spawnerarr.Length; i++)
        {
            var spawner = _spawnerarr[i];
            spawner.SpawnRate = SpawnRate;
            manager.SetComponentData(_spawneentityrarr[i], spawner);
        }
        SpawnRatetxt.text = $"{SpawnRate} u/s";
    }
    public void ReduceTowerValue(float value)
    {
        Tower.GetComponent<Tower>().Reducerhp(value);
    }

    public void checkwinningcondition()
    {
        if (Tower.GetComponent<Tower>().health <= 0)
        {
            //you win;
            isGamePaused = true;
            GamestateTxt.text = "You Win";
            GameStateDeclaration.SetActive(true);
        }
        if (timerSec <= 0)
        {
            //you Lose
            isGamePaused = true;
            GamestateTxt.text = "You Lose";
            GameStateDeclaration.SetActive(true);
        }
    }

    public void SwapDoorValues()
    {
        foreach (var t in DoorMatcherList)
        {
            t.SpawnValue = Random.Range(-10, 31);
            t.UpdateMyDoors();
        }
    }

}

