using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Walls : MonoBehaviour
{
    Vector2 Idx;
    Vector2 TagetIdx;

    public BlockColors MyColor;
    public Renderer renderer;
    public TextMeshProUGUI Multiplier;
    int myValue;

    public bool isValueMatch=>GridMaker.Instance.GetCell(TagetIdx).Color==MyColor&& GridMaker.Instance.GetCell(TagetIdx).IsOccupied;
    public void SetWalls(Vector2 idx,BlockColors myColor,Vector2 tagetIdx, int _myValue, bool isText)
    {
        Idx= idx;
        TagetIdx= tagetIdx;
        MyColor= myColor;
        updateValueAndTextUI(_myValue);
        renderer.sharedMaterial= BlockPuzzleManager.Instance.GetMaterialFromBlockColor(myColor);
        Multiplier.gameObject.SetActive(isText);

    }
 
    public void updateValueAndTextUI(int value)
    {
        myValue = value;
        Multiplier.text = $"x{myValue}";
        
    }
    
}
