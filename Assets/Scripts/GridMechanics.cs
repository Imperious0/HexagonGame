using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GridMechanics : MonoBehaviour
{
    [Header("Game Setting")]
    [SerializeField]
    private GameSettings gSetting;

    [SerializeField]
    private GameObject gridSystem;

    [SerializeField]
    private TextMeshProUGUI scoreText;
    [SerializeField]
    private TextMeshProUGUI highScoreText;
    [SerializeField]
    private TextMeshProUGUI movesText;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject[] tiles;
    [SerializeField]
    private GameObject bombTile;

    private MotionCapture mCapture;
    private Motions currentMotion;

    private TileData [,] gridTiles;

    private GameObject selectionGroup;
    private Vector2[] selectedTiles;

    private bool isNeedSelect = false;
    private bool isNeedRotation = false;
    private bool isClockwiseRotation = false;

    private bool isNextTileBomb = false;

    private int _score = 0;
    private int _moves = 0;

    private bool isGameOver = false;

    public int Score { get => _score; set { _score = value; scoreText.text = "" +_score; if (PlayerPrefs.GetInt("Highscore", 0) < _score) { this.highScoreText.text = "Highscore: " + _score; PlayerPrefs.SetInt("Highscore", _score); PlayerPrefs.Save(); } } }

    public int Moves { get => _moves; set { _moves = value; movesText.text = "" + _moves; } }

    private void Start()
    {
        mCapture = Camera.main.GetComponent<MotionCapture>();
        selectedTiles = new Vector2[3];
        highScoreText.text = "Highscore: " + PlayerPrefs.GetInt("Highscore", 0);
        Resize(GameObject.Find("BG"));

        InitializeGrid();

    }
    private void Update()
    {
        if (isGameOver)
            return;

        checkMotions();

    }
    private void FixedUpdate()
    {
        if (isNeedSelect)
        {
            selectGroup();
            isNeedSelect = false;
        }
    }
    private void Resize(GameObject go)
    {
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        transform.localScale = new Vector3(1, 1, 1);

        float width = sr.sprite.bounds.size.x;
        float height = sr.sprite.bounds.size.y;


        float worldScreenHeight = Camera.main.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        Vector3 xWidth = go.transform.localScale;
        xWidth.x = worldScreenWidth / width;
        go.transform.localScale = xWidth;
        //transform.localScale.x = worldScreenWidth / width;
        Vector3 yHeight = go.transform.localScale;
        yHeight.y = worldScreenHeight / height;
        go.transform.localScale = yHeight;
        //transform.localScale.y = worldScreenHeight / height;

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

                gridTiles[i, j] = new TileData();
                gridTiles[i, j].tile = tmpObj;
                gridTiles[i, j].deSelect();
                gridTiles[i, j].tileColor = gSetting.GameColors[UnityEngine.Random.Range(0, gSetting.GameColors.Length)];
                gridTiles[i, j].tile.GetComponent<SpriteRenderer>().color = gridTiles[i, j].tileColor;
                gridTiles[i, j].setInitialPosition(new Vector2(i, j));

                gridTiles[i, j].Score = gSetting.TileBasicScore * (selectedTile + 1);
            }
        }
        //Try to Center Grid.
        transform.position = new Vector3(-((gSetting.GridSize.y - 1) * gSetting.GridBaseIncrement.x) / 2, -((gSetting.GridSize.x - 1) * gSetting.GridBaseIncrement.y) / 2 + 0.5f);
        StartCoroutine(checkForBubbles());
    }
    private void checkMotions() 
    {
        currentMotion = mCapture.CurrentMotion;

        if(currentMotion == Motions.Deselect)
        {
            isNeedSelect = false;
            deSelectGroup();
            return;
        }
        if ((currentMotion == Motions.None) || isNeedRotation)
            return;

        if (currentMotion == Motions.Tap)
        {
            isNeedSelect = true;
        }
        else
        {
            //Swipe Event

            if (selectionGroup != null)
            {
                if (selectionGroup.transform.childCount == 3)
                {
                    isNeedRotation = true;

                    Vector3 mouseVectorRelative = (mCapture.CurrentEndClick + mCapture.CurrentClick);
                    Vector3 groupCenter = Camera.main.WorldToScreenPoint(selectionGroup.transform.position);
                    Vector3 relativeVector = ((mouseVectorRelative) - groupCenter * 2f).normalized;

                    float relativeVectorAngle = Vector2.SignedAngle(relativeVector, Vector2.right);

                    //Debug.LogError("Angle of relativeVector: " + relativeVectorAngle);
                    
                    Vector2 mouseVector = (mCapture.CurrentEndClick - mCapture.CurrentClick).normalized;
                    float relativeAngle = Vector2.SignedAngle(mouseVector, relativeVector);

                    //Debug.LogError("Angle Of Mouse Vector around RelativeVector : " + relativeAngle);

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
            //TODO now find which will groupped for flipping.
            Camera.main.GetComponent<UIController>().playSfx(SfxTypes.Select);

            float clickedAngle = Vector2.SignedAngle(hit.point - objectHit.position, Vector2.right);

            deSelectGroup();
            
            selectionGroup = new GameObject("SelectGroup");
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

            //Center the selected group
            Vector3 centerOfGroup = gridTiles[(int)firstPos.x, (int)firstPos.y].tile.transform.position + gridTiles[(int)secondPos.x, (int)secondPos.y].tile.transform.position + gridTiles[(int)thirdPos.x, (int)thirdPos.y].tile.transform.position;
            centerOfGroup /= 3;
            selectionGroup.transform.position = centerOfGroup;

            //Select the tiles
            gridTiles[(int)firstPos.x, (int)firstPos.y].Select();
            gridTiles[(int)secondPos.x, (int)secondPos.y].Select();
            gridTiles[(int)thirdPos.x, (int)thirdPos.y].Select();

            //Push tiles to selected group
            gridTiles[(int)firstPos.x, (int)firstPos.y].tile.transform.SetParent(selectionGroup.transform, true);
            gridTiles[(int)secondPos.x, (int)secondPos.y].tile.transform.SetParent(selectionGroup.transform, true);
            gridTiles[(int)thirdPos.x, (int)thirdPos.y].tile.transform.SetParent(selectionGroup.transform, true);

            //Little bit zoom in to selected group
            selectionGroup.transform.position += Vector3.forward * -0.2f;

            //Push selected tiles grid coords to handler
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
    private void deSelectGroup() 
    {
        if (selectionGroup != null)
        {
            foreach (Transform to in selectionGroup.transform.GetComponentsInChildren<Transform>())
            {
                if (to.gameObject.CompareTag("Tile"))
                {
                    to.SetParent(gridSystem.transform, true);
                    to.position = new Vector3(to.position.x, to.position.y, 0);
                    to.rotation = Quaternion.identity;
                }

                if (to.gameObject.CompareTag("TileBackmask"))
                    to.gameObject.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 0);
            }
            GameObject.Destroy(selectionGroup);
        }
    }
    private IEnumerator rotateGroup()
    {
        if(isNeedRotation)
        {

            do {
                if (isClockwiseRotation)
                    selectionGroup.transform.Rotate(Vector3.forward, -gSetting.RotationSpeed);
                else
                    selectionGroup.transform.Rotate(Vector3.forward, gSetting.RotationSpeed);

                yield return new WaitForFixedUpdate();


                float absZAngle = Mathf.Abs(this.selectionGroup.transform.rotation.eulerAngles.z);

                if (isClockwiseRotation)
                    absZAngle = 360f - absZAngle;

                if((absZAngle % 120f) + gSetting.RotationSpeed + 1 >= 120f)
                {
                  
                    float newRotationSpeed = 120f - (absZAngle % 120f);

                    if (isClockwiseRotation)
                        selectionGroup.transform.Rotate(Vector3.forward, -newRotationSpeed);
                    else
                        selectionGroup.transform.Rotate(Vector3.forward, newRotationSpeed);

                    Camera.main.GetComponent<UIController>().playSfx(SfxTypes.Bubble);

                    if (isClockwiseRotation) 
                    {
                        TileData tmp;
                        tmp = gridTiles[(int)selectedTiles[2].x, (int)selectedTiles[2].y];

                        gridTiles[(int)selectedTiles[2].x, (int)selectedTiles[2].y] = gridTiles[(int)selectedTiles[0].x, (int)selectedTiles[0].y];
                        gridTiles[(int)selectedTiles[0].x, (int)selectedTiles[0].y] = gridTiles[(int)selectedTiles[1].x, (int)selectedTiles[1].y];
                        gridTiles[(int)selectedTiles[1].x, (int)selectedTiles[1].y] = tmp;

                        selectionGroup.transform.GetChild(0).name = selectedTiles[1].x + "x" + selectedTiles[1].y;
                        selectionGroup.transform.GetChild(1).name = selectedTiles[2].x + "x" + selectedTiles[2].y;
                        selectionGroup.transform.GetChild(2).name = selectedTiles[0].x + "x" + selectedTiles[0].y;
                       
                        Vector2 tmpVector = selectedTiles[2];
                        selectedTiles[2] = selectedTiles[0];
                        selectedTiles[0] = selectedTiles[1];
                        selectedTiles[1] = tmpVector;
                    }
                    else 
                    {
                        TileData tmp;
                        tmp = gridTiles[(int)selectedTiles[1].x, (int)selectedTiles[1].y];

                        gridTiles[(int)selectedTiles[1].x, (int)selectedTiles[1].y] = gridTiles[(int)selectedTiles[0].x, (int)selectedTiles[0].y];
                        gridTiles[(int)selectedTiles[0].x, (int)selectedTiles[0].y] = gridTiles[(int)selectedTiles[2].x, (int)selectedTiles[2].y];
                        gridTiles[(int)selectedTiles[2].x, (int)selectedTiles[2].y] = tmp;

                        selectionGroup.transform.GetChild(0).name = selectedTiles[2].x + "x" + selectedTiles[2].y;
                        selectionGroup.transform.GetChild(1).name = selectedTiles[0].x + "x" + selectedTiles[0].y;
                        selectionGroup.transform.GetChild(2).name = selectedTiles[1].x + "x" + selectedTiles[1].y;
                       
                        Vector2 tmpVector = selectedTiles[1];
                        selectedTiles[1] = selectedTiles[0];
                        selectedTiles[0] = selectedTiles[2];
                        selectedTiles[2] = tmpVector;
                    }
                    
                    yield return new WaitForSeconds(0.1f);

                    

                    yield return StartCoroutine(checkForBubbles());

                    //If rotation catch a bubble so we do deselection and selectionGroup be nulled.
                    if (this.selectionGroup == null)
                        break;
                }
            } while (!(Mathf.Abs( this.selectionGroup.transform.rotation.eulerAngles.z % 360f) < 0.1f));

            //If rotation bubble something so the bombs need countdown and check for is bomb die.
            if(this.selectionGroup == null)
            {
                this.Moves++;
                for (int i = 0; i < gSetting.GridSize.x; i++)
                {
                    for (int j = 0; j < gSetting.GridSize.y; j++)
                    {
                        if(gridTiles[i, j].BombCountdown > 0)
                        {
                            gridTiles[i, j].BombCountdown -= 1;
                            if(gridTiles[i, j].BombCountdown == 0)
                            {
                                setGameOver();
                            }
                        }
                    }
                }
            }
            else
            {
                selectionGroup.transform.rotation = Quaternion.identity;
            }
            isNeedRotation = false;
            isClockwiseRotation = false;
        }
    }
    private IEnumerator checkForBubbles() 
    {
        //while bubbling interactions disabled
        isNeedRotation = true;

        while (BubbleIt())
        {
            deSelectGroup();
            shiftGrid(true);
            for (int i = 0; i < gSetting.GridSize.y; i++)
            {
                for (int j = 0; j < gSetting.GridSize.x; j++)
                {
                    if(gridTiles[j, i].tile.GetComponent<TileMechanics>().IsNeedMovement())
                        yield return new WaitWhile(gridTiles[j, i].tile.GetComponent<TileMechanics>().IsNeedMovement);
                    
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
        isNeedRotation = false;
    }

    private bool BubbleIt()
    {
        bool isNeedBubble = false;
        List<Vector2> bubbleList = new List<Vector2>();

        for (int i = 0; i < gSetting.GridSize.x; i++)
        {
            for (int j = 0; j < gSetting.GridSize.y; j++)
            {
                Vector3[] selections = calculateGroup(new Vector2(i, j), RotateDir.R, true);
                Vector3[] rSelections = calculateGroup(new Vector2(i, j), RotateDir.L, true);

                if(gridTiles[i, j].tileColor.Equals(gridTiles[(int)selections[0].x, (int)selections[0].y].tileColor)
                    && gridTiles[(int)selections[0].x, (int)selections[0].y].tileColor.Equals(gridTiles[(int)selections[1].x, (int)selections[1].y].tileColor))
                {
                    bubbleList.Add(new Vector2(i, j));
                    bubbleList.Add(selections[0]);
                    bubbleList.Add(selections[1]);
                }
                if (gridTiles[i, j].tileColor.Equals(gridTiles[(int)rSelections[0].x, (int)rSelections[0].y].tileColor)
                    && gridTiles[(int)rSelections[0].x, (int)rSelections[0].y].tileColor.Equals(gridTiles[(int)rSelections[1].x, (int)rSelections[1].y].tileColor))
                {
                    bubbleList.Add(new Vector2(i, j));
                    bubbleList.Add(rSelections[0]);
                    bubbleList.Add(rSelections[1]);
                }
            }
        }
        //Clear same coords
        bubbleList = bubbleList.Distinct().ToList();
        List<Color32> bColors = new List<Color32>();
        foreach (var item in bubbleList)
        {
            if(gridTiles[(int)item.x, (int)item.y].BombCountdown > 0)
            {
                bColors.Add(gridTiles[(int)item.x, (int)item.y].tileColor);
            }
        }

        bColors = bColors.Distinct().ToList();

        for (int i = 0; i < gSetting.GridSize.x; i++)
        {
            for (int j = 0; j < gSetting.GridSize.y; j++)
            {
                for (int k = 0; k < bColors.Count; k++)
                {
                    if (gridTiles[i, j].tileColor.Equals(bColors[k]))
                        bubbleList.Add(new Vector2(i, j));

                }
            }
        }
        bubbleList = bubbleList.Distinct().ToList();

        for (int i = 0; i < bubbleList.Count; i++)
        {
            gridTiles[(int)bubbleList[i].x, (int)bubbleList[i].y].BubbleIt();
            if((this.Score % gSetting.BombCounter) + gridTiles[(int)bubbleList[i].x, (int)bubbleList[i].y].Score >= gSetting.BombCounter)
            {
                isNextTileBomb = true;
            }
            this.Score += gridTiles[(int)bubbleList[i].x, (int)bubbleList[i].y].Score;
        }
        if (bubbleList.Count > 0)
            isNeedBubble = true;
        else
            isNeedBubble = false;

        if(isNeedBubble)
            Camera.main.GetComponent<UIController>().playSfx(SfxTypes.BubbleScs);

        return isNeedBubble;
    }

    private void shiftGrid(bool isDownside) 
    {
        for (int i = 0; i < gSetting.GridSize.y; i++)
        {
            List<TileData> column = new List<TileData>();
            for (int j = 0; j < gSetting.GridSize.x; j++)
            {
                column.Add(gridTiles[j, i]);
            }
            column.RemoveAll(p => p.isBubbled == true);

            int startOffset = (int)gSetting.GridSize.x - column.Count;

            startOffset += gSetting.TileRespawnOffset;

            while (column.Count < (gSetting.GridSize.x)) 
            {
                int index = (int)gSetting.GridSize.x - column.Count;
                GameObject tmpObj;

                int selectedTile = -1;
                int bombCountDown = -1;
                if (isNextTileBomb)
                {
                    tmpObj = Instantiate(bombTile, gridSystem.transform, false);
                    bombCountDown = UnityEngine.Random.Range(10, gSetting.BombMaxRange);
                    
                }
                else
                {
                    selectedTile = UnityEngine.Random.Range(0, tiles.Length);
                    tmpObj = Instantiate(tiles[selectedTile], gridSystem.transform, false);
                }

                TileData tmp = new TileData();
                tmp.tile = tmpObj;
                tmp.deSelect();
                tmp.BombCountdown = bombCountDown;
                tmp.tileColor = gSetting.GameColors[UnityEngine.Random.Range(0, gSetting.GameColors.Length)];
                tmp.tile.GetComponent<SpriteRenderer>().color = tmp.tileColor;

                tmp.setInitialPosition(new Vector2(-(startOffset - index), i));

                tmp.Score = gSetting.TileBasicScore * (selectedTile + 1);
                
                //Wheter is bomb tile created set it false
                isNextTileBomb = false;

                if (isDownside)
                    column.Insert(0, tmp);
                else
                    column.Add(tmp);
            }

            for (int j = 0; j < gSetting.GridSize.x; j++)
            {
                gridTiles[j, i] = column[j];
                gridTiles[j, i].changeGridPosition(new Vector2(j, i));
            }
        }
    }

    private void setGameOver() 
    {
        Camera.main.GetComponent<UIController>().showGGMenu();
        isGameOver = true;
    }
}

public enum RotateDir { R, UR, UL, L, LD, RD }

