using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Util;

public class SpawnerUIManager : SingletonRef<SpawnerUIManager>
{
    public TextMeshProUGUI text;
    public Canvas canvas;
    private void Start()
    {
        canvas.worldCamera = Camera.main;
    }
    public void updateSpawnRateTxt(float value)
    {
        text.text = $"{value}";
    }
}
