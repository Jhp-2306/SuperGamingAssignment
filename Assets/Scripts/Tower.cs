using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public float health=1000;
    public TextMeshProUGUI TowerHptxt;
    public void Reducerhp(float hp)
    {
        health-=hp;
    }
    public void Update()
    {
        if (BlockPuzzleManager.Instance.isGamePaused) return;
        TowerHptxt.text = health.ToString();
    }
    
}
