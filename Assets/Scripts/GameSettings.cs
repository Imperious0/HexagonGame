using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gSetting" , menuName = "Settings/gSettings")]
public class GameSettings : ScriptableObject
{
    [SerializeField]
    Vector2 _gridSize = Vector2.zero;
    [SerializeField]
    Vector2 _gridOffset = Vector2.zero;
    [SerializeField]
    Vector2 _gridBaseIncrement = new Vector2(1.5f, -1f);



    [SerializeField]
    Color32[] _gameColors;

    [SerializeField, Range(0.1f, 10f)]
    float _gameSpeed = 1f;

    public Vector2 GridSize { get => _gridSize; }
    public Vector2 GridOffset { get => _gridOffset;}
    public Vector2 GridBaseIncrement { get => _gridBaseIncrement;}
    public Color32[] GameColors { get => _gameColors; set => _gameColors = value; }
    public float GameSpeed { get => _gameSpeed;}
}
