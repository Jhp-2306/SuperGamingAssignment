using System.Collections;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class GridMaker : MonoBehaviour
{
    public static GridMaker Instance;
    public static CustomGrid<GridCell> Layout;
    private int gridWxH = 8;
    private int cellSize = 1;
    public GameObject P_cellsbg;
    public GameObject P_Gridbg;
    public GameObject P_Walls;
    public bool isFromCenter=false;
    public LayerMask notWalkable;
    public Dictionary<Vector2,GameObject> Walls;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this.gameObject);
        }
        init();
    }


    public void init()
    {
        CreateGrid();
        createGridVis();
        UpdateIsOccupied();

    }
    void createGridVis()
    {
        Walls= new Dictionary<Vector2, GameObject>();
        if(Layout != null)
        {
            var bg=Instantiate(P_Gridbg, transform);
            bg.transform.localScale = Vector3.one * gridWxH;
            bg.transform.localPosition = new Vector3(0.5f,0,0.5f);
            bg.transform.eulerAngles = new Vector3(90, 0, 0);
            foreach (var t in Layout.GetAllCells())
            {
                if (!t.IsOccupied)
                {
                    var temp = Instantiate(P_cellsbg, t.GetCoordinates3D(), Quaternion.identity);
                    temp.transform.eulerAngles = new Vector3(90, 0, 0);
                    temp.transform.SetParent(this.transform);
                    temp.name = $"{t.GetIndex()}";
                }
                else {
                    //Create Walls
                    var temp = Instantiate(P_Walls, t.GetCoordinates3D(), Quaternion.identity);
                    temp.transform.position=new Vector3(temp.transform.position.x,0.5f,temp.transform.position.z);
                    temp.transform.SetParent(this.transform);
                    temp.name = $"{t.GetIndex()}";                    
                    Walls.Add(t.GetIndex(),temp);
                }
            }
        }
    }
    public void CreateGrid()
    {
        Layout = new CustomGrid<GridCell>(gridWxH, gridWxH);
        
        for (int x = isFromCenter?-(gridWxH/2):0; x < (isFromCenter?gridWxH/2:gridWxH); x++)
        {
            for (int z = isFromCenter ? -(gridWxH / 2) : 0; z < (isFromCenter ? gridWxH / 2 : gridWxH); z++)
            {
                var idxX = isFromCenter ? x + (gridWxH / 2) : x;
                var idxZ = isFromCenter ? z + (gridWxH / 2) : z;
                GridCell _tempcell = new GridCell(new Vector2(idxX, idxZ), 
                    new Vector2(((x * cellSize)+1f)+transform.position.x, ((z * cellSize)+1f)+transform.position.z), 
                    idxX==0&&idxZ>=0||idxX== gridWxH-1&&idxZ>=0|| idxZ == 0 && idxX >= 0 || idxZ == gridWxH - 1 && idxX >= 0);
                _tempcell.Color=_tempcell.IsOccupied?BlockColors.Black:BlockColors.none;
                Layout.EditIndex(isFromCenter ? x + (gridWxH / 2) : x, isFromCenter ? z + (gridWxH / 2) : z, _tempcell);
                
            }
        }
    }
    public void UpdateIsOccupied()
    {
        var templist= Layout.GetAllCells();
        foreach (var _tempcell in templist)
        {
            _tempcell.IsOccupied= (Physics.CheckBox(_tempcell.GetCoordinates3D(), Vector3.one*0.1f, Quaternion.identity, notWalkable));
            Layout.EditIndex((int)_tempcell.GetIndex().x, (int)_tempcell.GetIndex().y, _tempcell);
        }
    }
    public bool ProcessCoordsList(List<Vector3> coordsList)
    {
        foreach(var pos in coordsList)
        {
            if(!processCoords(pos))return false;
        }
        return true;
    }
    public bool processCoords(Vector3 pos)
    {
        var cell=GetGridFromPos(pos);
        if(cell==null) return false;
        if(cell.GetCoordinates3D() == pos&&!cell.IsOccupied) return true;
        return false;
    }
    public GridCell GetGridFromPos(Vector3 pos)
    {
        foreach(var t in Layout.GetAllCells())
        {
            if(t.GetCoordinates3D()==pos)return t;
        }
        return null;
    }

    public void UpdateCellOccupied(Vector3 pos,bool isOcc)
    {
       var t= GetGridFromPos(pos);
        t.IsOccupied = isOcc;
        Layout.EditIndex((int)t.GetIndex().x,(int) t.GetIndex().y,t);
    }
    public void UpdateCellColor(Vector3 pos, BlockColors color)
    {
        var t = GetGridFromPos(pos);
        t.Color = color;
        Layout.EditIndex((int)t.GetIndex().x, (int)t.GetIndex().y, t);
    }
    public void UpdateCellOccupiedList(List<Vector3> Coords, bool isOcc)
    {
       foreach (var pos in Coords)
        {
            UpdateCellOccupied(pos,isOcc);
        }
    }
    private void Update()
    {
        if (BlockPuzzleManager.Instance.isGamePaused) return;
        if (Input.GetKeyDown(KeyCode.P))
        {
            PrintAllCell();
        }
    }
    public void PrintAllCell()
    {
        var templist = Layout.GetAllCells();
        foreach (var _tempcell in templist)
        {
            Debug.Log($"idx:{_tempcell.GetIndex()}, pos{_tempcell.GetCoordinates3D()}, isOcc{_tempcell.IsOccupied}, color{_tempcell.Color}");
        }
    }
    public GridCell GetCell(Vector2 pos)
    {
        return Layout.GetAtIndex((int)pos.x,(int)pos.y);
    }

    public class CustomGrid<T>
    {
        int _width;
        int _height;
        T[,] _grid;

        public CustomGrid(int width, int height)
        {
            _width = width;
            _height = height;
            _grid = new T[_width, _height];
        }
        public void EditIndex(int x, int y, T val)
        {
            _grid[x, y] = val;
        }
        public int getWidth() => _width;
        public int getHeight() => _height;
        public T GetAtIndex(int x, int y) => _grid[x, y];

        public List<T> GetAllCells() {
            List<T> cells = new List<T>();
        foreach (var cell in _grid)
            {
                cells.Add(cell);
            }
        return cells;
        }

        
    }

    public class GridCell
    {
        Vector2 Index;
        Vector2 AnchorCoordinates;
        bool isOccupied;
        BlockColors color;
        public bool IsOccupied {  get { return isOccupied; } set { isOccupied = value; } }
        public BlockColors Color { get=>color; set { color = value; } }

        public GridCell(Vector2 index, Vector2 coordinates, bool isOccupied)
        {
            Index = index;
            AnchorCoordinates = coordinates;
            this.isOccupied = isOccupied;
        }
        //public bool GetIsOccupied() => isOccupied;
        public Vector2 GetIndex() => Index;
        public Vector2 GetCoordinates2D() => AnchorCoordinates;
        public Vector3 GetCoordinates3D() => new Vector3(AnchorCoordinates.x, 0, AnchorCoordinates.y);
    }
}
