using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileData
{
    public GameObject tile;
    public Color32 tileColor;
    public int Score;
    private int bombCountdown;
    public bool isBubbled = false;

    public int BombCountdown { get => bombCountdown; set { this.tile.GetComponent<TileMechanics>().setBombSituat((value >= 0 ? true : false), value); bombCountdown = value; } }

    internal void BubbleIt()
    {
        isBubbled = true;
        this.deSelect();
        GameObject.Destroy(tile);

    }
    internal void Select() 
    {
        tile.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
    }
    internal void deSelect() 
    {
        this.tile.transform.rotation = Quaternion.identity;
        tile.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 0);  
    }
    internal void setInitialPosition(Vector2 position) 
    {
        this.tile.GetComponent<TileMechanics>().setInitialPosition(position);
    }
    internal void changeGridPosition(Vector2 position) 
    {
        this.tile.GetComponent<TileMechanics>().changeGridPosition(position);
    }
}
