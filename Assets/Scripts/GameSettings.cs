using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gSetting" , menuName = "Settings/gSettings")]
public class GameSettings : ScriptableObject
{
    [Header("Grid Settings")]
    [SerializeField]
    Vector2 _gridSize = Vector2.zero;
    [SerializeField]
    Vector2 _gridOffset = Vector2.zero;
    [SerializeField]
    Vector2 _gridBaseIncrement = new Vector2(1.5f, -1f);

    [Header("Game Settings")]
    [SerializeField]
    Color32[] _gameColors;

    const float maxVal = 10f;

    [SerializeField, Range(0.1f, maxVal)]
    float _gameSpeed = 1f;

    [SerializeField, Range(1, 20)]
    int _rotationSpeed = 1;

    [SerializeField, Range(1, 10)]
    int _tileRespawnOffset = 4;

    [SerializeField]
    int _bombCounter = 1000;

    [SerializeField, Range(1, 50)]
    int _bombMaxRange = 50;

    [Header("Score Settings")]
    [SerializeField]
    int _tileBasicScore = 5;

    public Vector2 GridSize { get => _gridSize; }
    public Vector2 GridOffset { get => _gridOffset;}
    public Vector2 GridBaseIncrement { get => _gridBaseIncrement * ScreenRatio;}

    public Color32[] GameColors { get => _gameColors; set => _gameColors = value; }
    public float GameSpeed { get => maxVal - _gameSpeed;}
    public int BombCounter { get => _bombCounter; }
    public int BombMaxRange { get => _bombMaxRange; }
    public int RotationSpeed { get => _rotationSpeed; }

    public int TileBasicScore { get => _tileBasicScore; }
    public int TileRespawnOffset { get => _tileRespawnOffset; }
    public float ScreenRatio { get => ((Screen.width / 1440f) / (Screen.height / 2560f)); }
}
