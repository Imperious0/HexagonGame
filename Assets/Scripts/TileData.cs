using System;
using UnityEngine;

[Serializable]
public class TileData
{
    private GameObject tile;
    private TileMechanics tileMechanics;
    [SerializeField]
    public Color32 tileColor;
    public int Score;
    private int bombCountdown;
    public bool isBubbled = false;

    public int BombCountdown { get => bombCountdown; set { tileMechanics.setBombSituat((value >= 0 ? true : false), value); bombCountdown = value; } }

    public GameObject Tile { get => tile; set { tile = value; if (value != null) { tileMechanics = value.GetComponent<TileMechanics>(); } else { tileMechanics = null; } } }

    public TileMechanics TileMechanics { get => tileMechanics; }

    internal void BubbleIt()
    {
        isBubbled = true;
        this.TileMechanics.deSelect();
        GameObject.DestroyImmediate(tile);

    }
  
}
