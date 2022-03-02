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


    private MotionCapture mCapture;
    private Motions currentMotion;

    private TileData [,] gridTiles;

    private GameObject selectionGroup;
    private Vector2[] selectedTiles;

    private bool isNeedRotation = false;
    private bool isClockwiseRotation = false;

    private void Start()
    {
        mCapture = Camera.main.GetComponent<MotionCapture>();
        selectedTiles = new Vector2[3];
        InitializeGrid();

    }
    private void Update()
    {
        checkMotions();

    }
    private void InitializeGrid()
    {
        gridTiles = new TileData[(int)gSetting.GridSize.x, (int)gSetting.GridSize.y];
        for (int i = 0; i < gSetting.GridSize.x; i++)
        {
            for (int j = 0; j < gSetting.GridSize.y; j++)
            {
                int selectedTile = UnityEngine.Random.Range(0, tiles.Length);
                GameObject tmpObj = Instantiate(tiles[selectedTile], gridSystem.transform, false);
                tmpObj.name = i + "x" + j;
                tmpObj.transform.position = new Vector3(gSetting.GridBaseIncrement.x*j, gSetting.GridBaseIncrement.y * i + (j % 2 == 1 ? -0.5f : 0f), 0);
                gridTiles[i, j] = new TileData();
                gridTiles[i, j].tile = tmpObj;
                gridTiles[i, j].worldPosition = tmpObj.transform.position;
                gridTiles[i, j].gridPosition = new Vector2(i,j);
            }
        }
        //Try to Center Grid.
        transform.position = new Vector3(-((gSetting.GridSize.y - 1) * gSetting.GridBaseIncrement.x) / 2, -((gSetting.GridSize.x - 1) * gSetting.GridBaseIncrement.y) / 2 + 0.5f);
    }
    private void checkMotions() 
    {
        currentMotion = mCapture.CurrentMotion;

        Debug.LogError(currentMotion);

        if ((currentMotion == Motions.None) || isNeedRotation)
            return;

        if (currentMotion == Motions.Tap)
        {
            selectGroup();
        }
        else
        {
            //Swipe Event

            if (selectionGroup != null)
            {
                if (selectionGroup.transform.childCount == 3)
                {
                    Vector3 mouseVectorRelative = (mCapture.CurrentEndClick + mCapture.CurrentClick);
                    Vector3 groupCenter = Camera.main.WorldToScreenPoint(selectionGroup.transform.position);
                    Vector3 relativeVector = ((mouseVectorRelative) - groupCenter * 2f).normalized;

                    //Debug.LogError("Start Point :" + mCapture.CurrentClick + " " + " End Point: " + mCapture.CurrentEndClick);
                    float relativeVectorAngle = Vector2.SignedAngle(relativeVector, Vector2.right);
                    //Debug.LogError("Center of mPoint: " + ((mCapture.CurrentEndClick - mCapture.CurrentClick) / 2f));
                    //Debug.LogError("Vector of mPoint: " + (((mCapture.CurrentEndClick - mCapture.CurrentClick) / 2f) - Camera.main.WorldToScreenPoint(selectionGroup.transform.position)));
 
                    
                    Debug.LogError("Angle of relativeVector: " + relativeVectorAngle);
                    isNeedRotation = true;

                    Vector2 mouseVector = (mCapture.CurrentEndClick - mCapture.CurrentClick).normalized;
                    float relativeAngle = Vector2.SignedAngle(mouseVector, relativeVector);

                    Debug.LogError("Angle Of Mouse Vector around RelativeVector : " + relativeAngle);


                    if(relativeAngle < 0) 
                    {
                        //Left Side Sign
                        isClockwiseRotation = false;
                    }
                    else
                    {
                        //Right Side Sign
                        isClockwiseRotation = true;
                    }
                   
                    StartCoroutine(rotateGroup());
                }
            }
        }
        
    }
    private void selectGroup() 
    {
        Ray ray = Camera.main.ScreenPointToRay(mCapture.CurrentClick);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Transform objectHit = hit.transform;

            objectHit.gameObject.GetComponent<SpriteRenderer>().color = gSetting.GameColors[UnityEngine.Random.Range(0, gSetting.GameColors.Length)];

            //TODO now find which will groupped for flipping.

            float clickedAngle = Vector2.SignedAngle(hit.point - objectHit.position, Vector2.right);

            if (selectionGroup != null) 
            {
                foreach(Transform to in selectionGroup.transform.GetComponentsInChildren<Transform>())
                {
                    to.SetParent(gridSystem.transform, true);
                }
                GameObject.Destroy(selectionGroup);
            }
               

            selectionGroup = new GameObject("SelectionGroup");
            selectionGroup.transform.position = Vector3.zero;
            selectionGroup.transform.rotation = Quaternion.identity;

            selectionGroup.transform.SetParent(gridSystem.transform, false);

            Vector2 firstPos = new Vector2(int.Parse(objectHit.gameObject.name.Split('x')[0]), int.Parse(objectHit.gameObject.name.Split('x')[1]));
            Vector2 secondPos = -Vector2.one;
            Vector2 thirdPos = -Vector2.one;


            RotateDir r = RotateDir.R;
            if (clickedAngle <= 30 && clickedAngle >= -30)
            {
                r = RotateDir.R;
            }
            else if (clickedAngle < -30 && clickedAngle > -90)
            {
                r = RotateDir.UR;
            }
            else if (clickedAngle <= -90 && clickedAngle > -150)
            {
                r = RotateDir.UL;
            }
            else if (clickedAngle <= -150 || clickedAngle >= 150)
            {
                r = RotateDir.L;
            }
            else if (clickedAngle > 30 && clickedAngle < 90)
            {
                //Right DOWN Select
                r = RotateDir.RD;
            }
            else if (clickedAngle >= 90 && clickedAngle < 150)
            {
                //Left DOWN Select
                r = RotateDir.LD;
            }

            Vector3[] clCount = calculateGroup(firstPos, r, true);
            Vector3[] cClCount = calculateGroup(firstPos, r, false);

            if (clCount[0].z == 0)
            {
                secondPos = clCount[0];
                thirdPos = clCount[1];
            }
            else if (clCount[0].z < cClCount[0].z)
            {
                secondPos = clCount[0];
                thirdPos = clCount[1];
            }
            else if (cClCount[0].z < clCount[0].z)
            {
                secondPos = cClCount[0];
                thirdPos = cClCount[1];
            }
            else
            {
                secondPos = clCount[0];
                thirdPos = clCount[1];
            }

            gridTiles[(int)secondPos.x, (int)secondPos.y].tile.GetComponent<SpriteRenderer>().color = gSetting.GameColors[UnityEngine.Random.Range(0, gSetting.GameColors.Length)];
            gridTiles[(int)thirdPos.x, (int)thirdPos.y].tile.GetComponent<SpriteRenderer>().color = gSetting.GameColors[UnityEngine.Random.Range(0, gSetting.GameColors.Length)];

            Vector3 centerOfGroup = gridTiles[(int)firstPos.x, (int)firstPos.y].tile.transform.position + gridTiles[(int)secondPos.x, (int)secondPos.y].tile.transform.position + gridTiles[(int)thirdPos.x, (int)thirdPos.y].tile.transform.position;
            centerOfGroup /= 3;

            selectionGroup.transform.position = centerOfGroup;

            gridTiles[(int)firstPos.x, (int)firstPos.y].tile.transform.SetParent(selectionGroup.transform, true);
            gridTiles[(int)secondPos.x, (int)secondPos.y].tile.transform.SetParent(selectionGroup.transform, true);
            gridTiles[(int)thirdPos.x, (int)thirdPos.y].tile.transform.SetParent(selectionGroup.transform, true);

            selectedTiles[0] = firstPos;
            selectedTiles[1] = secondPos;
            selectedTiles[2] = thirdPos;

            Debug.DrawRay(ray.origin, ray.direction * 20f, Color.green, 100f);
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * 20f, Color.red, 100f);
        }
    }
    private Vector3[] calculateGroup(Vector2 selectedPos, RotateDir dir, bool isClockwise) 
    {
        Vector2 secondPos = -Vector2.one;
        Vector2 thirdPos = -Vector2.one;
        Vector3[] result = new Vector3[2];
        int cycleDone = 0;

        if(selectedPos.y % 2 == 0) 
        {
            secondPos = selectedPos;
            thirdPos = selectedPos;

            switch (dir)
            {
                case RotateDir.R:
                    secondPos.y += 1;

                    thirdPos.y += 1;
                    thirdPos.x -= 1;
                    break;
                case RotateDir.UR:
                    secondPos.y += 1;
                    secondPos.x -= 1;

                    thirdPos.x -= 1;
                    break;
                case RotateDir.UL:
                    secondPos.x -= 1;

                    thirdPos.x -= 1;
                    thirdPos.y -= 1;

                    break;
                case RotateDir.L:
                    secondPos.x -= 1;
                    secondPos.y -= 1;

                    thirdPos.y -= 1;
                    break;
                case RotateDir.LD:
                    secondPos.y -= 1;

                    thirdPos.x += 1;
                    break;
                case RotateDir.RD:
                    secondPos.x += 1;

                    thirdPos.y += 1;
                    break;
            }
        }
        else 
        {
            secondPos = selectedPos;
            thirdPos = selectedPos;

            switch (dir)
            {
                case RotateDir.R:
                    secondPos.x += 1;
                    secondPos.y += 1;

                    thirdPos.y += 1;

                    break;
                case RotateDir.UR:
                    secondPos.y += 1;

                    thirdPos.x -= 1;

                    break;
                case RotateDir.UL:
                    secondPos.x -= 1;

                    thirdPos.y -= 1;
                    break;
                case RotateDir.L:
                    secondPos.y -= 1;

                    thirdPos.x += 1;
                    thirdPos.y -= 1;

                    break;
                case RotateDir.LD:
                    secondPos.x += 1;
                    secondPos.y -= 1;

                    thirdPos.x += 1;
                    break;
                case RotateDir.RD:
                    secondPos.x += 1;

                    thirdPos.x += 1;
                    thirdPos.y += 1;
                    break;
            }
        }

        try
        {
            TileData t = gridTiles[(int)secondPos.x, (int)secondPos.y];
            t = gridTiles[(int)thirdPos.x, (int)thirdPos.y];
            result[0] = secondPos;
            result[1] = thirdPos;
        }
        catch (Exception e)
        {
            secondPos = -Vector2.one;
            thirdPos = -Vector2.one;
        }


        if(secondPos == -Vector2.one || thirdPos == -Vector2.one) 
        {
            //Cant Find Yet

            if (isClockwise)
            {
                if (dir == RotateDir.R)
                    dir = RotateDir.RD;
                else if (dir == RotateDir.RD)
                    dir = RotateDir.LD;
                else if (dir == RotateDir.LD)
                    dir = RotateDir.L;
                else if (dir == RotateDir.L)
                    dir = RotateDir.UL;
                else if (dir == RotateDir.UL)
                    dir = RotateDir.UR;
                else if (dir == RotateDir.UR)
                    dir = RotateDir.R;
            }
            else
            {
                if (dir == RotateDir.R)
                    dir = RotateDir.UR;
                else if (dir == RotateDir.UR)
                    dir = RotateDir.UL;
                else if (dir == RotateDir.UL)
                    dir = RotateDir.L;
                else if (dir == RotateDir.L)
                    dir = RotateDir.LD;
                else if (dir == RotateDir.LD)
                    dir = RotateDir.RD;
                else if (dir == RotateDir.RD)
                    dir = RotateDir.R;
            }
            cycleDone++;
            result = calculateGroup(selectedPos, dir, isClockwise);
            result[0].z += cycleDone;
            result[1].z += cycleDone;
        }
        else 
        {
            cycleDone++;
            result[0].z += cycleDone;
            result[1].z += cycleDone;
        }

        return result;
    }

    private IEnumerator rotateGroup()
    {
        if(isNeedRotation)
        {

            do {
                if(isClockwiseRotation)
                    selectionGroup.transform.Rotate(Vector3.forward, -gSetting.GameSpeed);
                else
                    selectionGroup.transform.Rotate(Vector3.forward, gSetting.GameSpeed);

                yield return new WaitForFixedUpdate();
                if( Mathf.Abs(this.selectionGroup.transform.rotation.eulerAngles.z % 120) < 0.01f)
                {
                    TileData tmp;
                    tmp = gridTiles[(int)selectedTiles[1].x, (int)selectedTiles[1].y];

                    gridTiles[(int)selectedTiles[1].x, (int)selectedTiles[1].y] = gridTiles[(int)selectedTiles[0].x, (int)selectedTiles[0].y];
                    gridTiles[(int)selectedTiles[0].x, (int)selectedTiles[0].y] = gridTiles[(int)selectedTiles[2].x, (int)selectedTiles[2].y];
                    gridTiles[(int)selectedTiles[2].x, (int)selectedTiles[2].y] = tmp;

                    selectionGroup.transform.GetChild(0).name = selectedTiles[2].x + "x" + selectedTiles[2].y;
                    selectionGroup.transform.GetChild(1).name = selectedTiles[0].x + "x" + selectedTiles[0].y;
                    selectionGroup.transform.GetChild(2).name = selectedTiles[1].x + "x" + selectedTiles[1].y;

                    yield return new WaitForSeconds(0.1f);
                    if (checkForBubble()) 
                    {
                        
                        
                        break;
                    }
                }
            } while (!(Mathf.Abs( this.selectionGroup.transform.rotation.eulerAngles.z % 360) < 0.01f));
            selectionGroup.transform.rotation = Quaternion.identity;
            isNeedRotation = false;
            isClockwiseRotation = false;
        }
    }

    private bool checkForBubble()
    {

        return false;
    }
}

public enum RotateDir { R, UR, UL, L, LD, RD }

