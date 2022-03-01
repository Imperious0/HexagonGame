using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMechanics : MonoBehaviour
{
    [Header("Game Setting")]
    [SerializeField]
    private GameSettings gSetting;

    [SerializeField]
    private GameObject gridSystem;


    [Header("Prefabs")]
    [SerializeField]
    private GameObject[] tiles;

    [SerializeField]
    private MotionCapture mCapture;


    private void Start()
    {
        mCapture = Camera.main.GetComponent<MotionCapture>();

        InitializeGrid();

    }
    private void Update()
    {
        selectGroup();
    }
    private void InitializeGrid()
    {
        for (int i = 0; i < gSetting.GridSize.x; i++)
        {
            for (int j = 0; j < gSetting.GridSize.y; j++)
            {
                int selectedTile = UnityEngine.Random.Range(0, tiles.Length);
                GameObject tmpObj = Instantiate(tiles[selectedTile], gridSystem.transform, false);
                tmpObj.transform.position = new Vector3(gSetting.GridBaseIncrement.x*j, gSetting.GridBaseIncrement.y * i + (j % 2 == 1 ? -0.5f : 0f), 0);
            }
        }
    }
    private void selectGroup() 
    {
        if (mCapture.CurrentMotion.Equals(Motions.Tap)) 
        {

            Ray ray = Camera.main.ScreenPointToRay(mCapture.CurrentClick);
            RaycastHit hit;
 
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Transform objectHit = hit.transform;

                objectHit.gameObject.GetComponent<SpriteRenderer>().color = gSetting.GameColors[UnityEngine.Random.Range(0, gSetting.GameColors.Length)];

                //TODO now find which will groupped for flipping.

                Debug.LogError(Vector2.Angle(hit.point - objectHit.position, Vector2.right));

                Debug.DrawRay(ray.origin, ray.direction * 20f, Color.green, 100f);
            }
            else
            {
                Debug.DrawRay(ray.origin, ray.direction * 20f, Color.red, 100f);
            }
        }
    }
}
