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
    private UIController uiController;

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

    private TileData[,] gridTiles;

    private GameObject selectionGroup;
    private Vector2[] selectedTiles;
    private readonly Vector2[] neighbourTiles = new Vector2[] 
    { 
        new Vector2(-1, 1), 
        new Vector2(-1, 0), 
        new Vector2(-1, -1), 
        new Vector2(0, -1),
        new Vector2(1, 0),
        new Vector2(0, 1),
    };
    private readonly Vector2[] neighbourTiles2 = new Vector2[]
    {
        new Vector2(0, 1),
        new Vector2(-1, 0),
        new Vector2(0, -1),
        new Vector2(1, -1),
        new Vector2(1, 0),
        new Vector2(1, 1),
    };

    private float lastSelectionAngle = 0f;

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
        selectionGroup = new GameObject("SelectGroup");

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
        //Try to Center Grid.
        transform.position = new Vector3(-((gSetting.GridSize.y - 1) * gSetting.GridBaseIncrement.x) / 2, -((gSetting.GridSize.x - 1) * gSetting.GridBaseIncrement.y) / 2 + 0.5f);
        for (int i = 0; i < gSetting.GridSize.x; i++)
        {
            for (int j = 0; j < gSetting.GridSize.y; j++)
            {
                int selectedTile = UnityEngine.Random.Range(0, tiles.Length);
                GameObject tmpObj = Instantiate(tiles[selectedTile], gridSystem.transform, false);

                gridTiles[i, j] = new TileData();
                gridTiles[i, j].Tile = tmpObj;
                gridTiles[i, j].TileMechanics.deSelect();
                gridTiles[i, j].tileColor = gSetting.GameColors[UnityEngine.Random.Range(0, gSetting.GameColors.Length)];
                gridTiles[i, j].Tile.GetComponent<SpriteRenderer>().color = gridTiles[i, j].tileColor;
                gridTiles[i, j].TileMechanics.setInitialPosition(new Vector2(i, j));

                gridTiles[i, j].Score = gSetting.TileBasicScore * (selectedTile + 1);
            }
        }

        StartCoroutine(checkForBubbles());
    }
    private void checkMotions() 
    {
        if (isNeedRotation)
            return;

        currentMotion = mCapture.CurrentMotion;

        if(currentMotion == Motions.Deselect)
        {
            isNeedSelect = false;
            deSelectGroup();
            return;
        }
        if ((currentMotion == Motions.None))
            return;

        if (currentMotion == Motions.Tap)
        {
            isNeedSelect = true;
        }
        else
        {
            //Swipe Event

            if (selectionGroup != null && isNeedRotation == false)
            {
                if (selectionGroup.transform.position != Vector3.zero)
                {
                    isNeedRotation = true;

                    Vector3 mouseVectorRelative = (mCapture.CurrentEndClick + mCapture.CurrentClick);
                    Vector3 groupCenter = Camera.main.WorldToScreenPoint(selectionGroup.transform.position);
                    Vector3 relativeVector = (mouseVectorRelative - (groupCenter * 2f)).normalized;

                    //float relativeVectorAngle = Vector2.SignedAngle(relativeVector, Vector2.right);
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
                    //Debug.LogError("isClockwise : " + isClockwiseRotation);
                    StartCoroutine(rotateGroup());
                }
            }
        }
        
    }
    private void selectGroup() 
    {
        Ray ray = Camera.main.ScreenPointToRay(mCapture.CurrentClick);
        RaycastHit2D rayHit = Physics2D.GetRayIntersection(ray);

        if (rayHit.transform != null)
        {
            Transform objectHit = rayHit.transform;
            //TODO now find which will groupped for flipping.
            uiController.playSfx(SfxTypes.Select);


            float clickedAngle = -Vector2.SignedAngle((rayHit.point - objectHit.position * Vector2.one), Vector2.right);
            
            lastSelectionAngle = clickedAngle;


            deSelectGroup();
            

            selectionGroup.transform.position = Vector3.zero;
            selectionGroup.transform.rotation = Quaternion.identity;

            selectionGroup.transform.SetParent(gridSystem.transform, false);

            Vector2 firstPos = new Vector2(int.Parse(objectHit.gameObject.name.Split('x')[0]), int.Parse(objectHit.gameObject.name.Split('x')[1]));
            Vector2 secondPos = -Vector2.one;
            Vector2 thirdPos = -Vector2.one;


            int selectedIndex = calculateSelectionIndex(lastSelectionAngle);
            Vector3[] selCount = calculateGroup(firstPos, selectedIndex, true);

            secondPos = selCount[0];
            thirdPos = selCount[1];

            //Center the selected group
            Vector3 centerOfGroup = gridTiles[(int)firstPos.x, (int)firstPos.y].Tile.transform.localPosition + gridTiles[(int)secondPos.x, (int)secondPos.y].Tile.transform.localPosition + gridTiles[(int)thirdPos.x, (int)thirdPos.y].Tile.transform.localPosition;
            centerOfGroup /= 3;
            selectionGroup.transform.localPosition = centerOfGroup;

            //Select the tiles
            gridTiles[(int)firstPos.x, (int)firstPos.y].TileMechanics.Select();
            gridTiles[(int)secondPos.x, (int)secondPos.y].TileMechanics.Select();
            gridTiles[(int)thirdPos.x, (int)thirdPos.y].TileMechanics.Select();


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
            deSelectGroup();
            Debug.DrawRay(ray.origin, ray.direction * 20f, Color.red, 100f);
        }
    }
    private int calculateSelectionIndex(float angle)
    {
        int index = -1;

        if (angle < 30f && angle > -30f)
            index = 0;

        if (index == 0)
            return index;

        if (angle < 0f)
            angle += 360f;

        angle %= 360f;


        float approxAngle = 30f;
        float incrementAmount = 60f;
        for (int i = 1; i < 6; i++)
        {
            if(angle > approxAngle && angle < approxAngle + incrementAmount )
            {
                index = i;
                break;
            }
            approxAngle += incrementAmount;
        }

        return index;
    }
    private Vector3[] calculateGroup(Vector2 selectedPos, int index, bool isClockwise)
    {
        Vector3[] result;

        Vector2 secondPos = -Vector2.one;
        Vector2 thirdPos = -Vector2.one;
        
        Vector2[] currentLineup;

        if(selectedPos.y % 2 == 0)
        {
            currentLineup = neighbourTiles;      
        }
        else
        {
            currentLineup = neighbourTiles2;
        }
        int[] lookingIndex = new int[currentLineup.Length];
        if(isClockwise)
        {
            int tmpIndex = 0;
            for (int i = index; i > -1; i--)
            {
                lookingIndex[tmpIndex] = i;
                tmpIndex++;
            }
            for (int i = currentLineup.Length - 1; i > index; i--)
            {
                lookingIndex[tmpIndex] = i;
                tmpIndex++;
            }
        }
        else 
        {
            int tmpIndex = 0;
            for (int i = index; i < currentLineup.Length; i++)
            {
                lookingIndex[tmpIndex] = i;
                tmpIndex++;
            }
            for (int i = 0; i < index; i++)
            {
                lookingIndex[tmpIndex] = i;
                tmpIndex++;
            }
        }

        for (int i = 0; i < lookingIndex.Length; i++)
        {
            int secondIndex = lookingIndex[i] - 1;
            int thirdIndex = lookingIndex[i];
            if (secondIndex < 0)
                secondIndex = lookingIndex.Length - 1;

            if (selectedPos.x + currentLineup[secondIndex].x < 0 || selectedPos.x + currentLineup[secondIndex].x >= gridTiles.GetLength(0))
                continue;
            if (selectedPos.y + currentLineup[secondIndex].y < 0 || selectedPos.y + currentLineup[secondIndex].y >= gridTiles.GetLength(1))
                continue;
            if (selectedPos.x + currentLineup[thirdIndex].x < 0 || selectedPos.x + currentLineup[thirdIndex].x >= gridTiles.GetLength(0))
                continue;
            if (selectedPos.y + currentLineup[thirdIndex].y < 0 || selectedPos.y + currentLineup[thirdIndex].y >= gridTiles.GetLength(1))
                continue;

            secondPos = selectedPos + currentLineup[secondIndex];
            thirdPos = selectedPos + currentLineup[thirdIndex];
            break;
        }

        result = new Vector3[] { secondPos, thirdPos };
        return result;

    }
    private void deSelectGroup() 
    {
        if (selectionGroup.transform.position != Vector3.zero)
        {
            gridTiles[(int)selectedTiles[0].x, (int)selectedTiles[0].y].TileMechanics.deSelect();
            gridTiles[(int)selectedTiles[1].x, (int)selectedTiles[1].y].TileMechanics.deSelect();
            gridTiles[(int)selectedTiles[2].x, (int)selectedTiles[2].y].TileMechanics.deSelect();
            selectionGroup.transform.position = Vector3.zero;
        }
    }
    private IEnumerator rotateGroup()
    {
        if(isNeedRotation)
        {
            float angle = isClockwiseRotation ? 120f : -120f;
            for (int i = 1; i < 4; i++)
            {
                float currentAngle = angle * i;
                Quaternion prev = selectionGroup.transform.rotation;
                float currentFrame = 0f;
                float interpolation = 0f;
                do
                {
                    currentFrame += Time.deltaTime * gSetting.RotationSpeed;
                    interpolation = Mathf.Lerp(0, 1, currentFrame);

                    Quaternion qu = Quaternion.Lerp(prev, Quaternion.Euler(Vector3.forward * currentAngle), interpolation);

                    gridTiles[(int)selectedTiles[0].x, (int)selectedTiles[0].y].Tile.transform.RotateAround(
                        selectionGroup.transform.position, 
                        Vector3.forward ,
                        Quaternion.Angle(selectionGroup.transform.rotation, qu) * (isClockwiseRotation ? -1f : 1f)
                        );;
                    gridTiles[(int)selectedTiles[1].x, (int)selectedTiles[1].y].Tile.transform.RotateAround(
                        selectionGroup.transform.position, 
                        Vector3.forward,
                        Quaternion.Angle(selectionGroup.transform.rotation, qu) * (isClockwiseRotation ? -1f : 1f)
                        );
                    gridTiles[(int)selectedTiles[2].x, (int)selectedTiles[2].y].Tile.transform.RotateAround(
                        selectionGroup.transform.position,
                        Vector3.forward,
                        Quaternion.Angle(selectionGroup.transform.rotation, qu) * (isClockwiseRotation ? -1f : 1f)
                    );

                    selectionGroup.transform.rotation = qu;
                    yield return new WaitForEndOfFrame();


                } while (!(interpolation >= 1f));
                if (isClockwiseRotation)
                {
                    TileData tmp;
                    tmp = gridTiles[(int)selectedTiles[2].x, (int)selectedTiles[2].y];

                    gridTiles[(int)selectedTiles[2].x, (int)selectedTiles[2].y] = gridTiles[(int)selectedTiles[0].x, (int)selectedTiles[0].y];
                    gridTiles[(int)selectedTiles[0].x, (int)selectedTiles[0].y] = gridTiles[(int)selectedTiles[1].x, (int)selectedTiles[1].y];
                    gridTiles[(int)selectedTiles[1].x, (int)selectedTiles[1].y] = tmp;

                }
                else
                {
                    TileData tmp;
                    tmp = gridTiles[(int)selectedTiles[1].x, (int)selectedTiles[1].y];

                    gridTiles[(int)selectedTiles[1].x, (int)selectedTiles[1].y] = gridTiles[(int)selectedTiles[0].x, (int)selectedTiles[0].y];
                    gridTiles[(int)selectedTiles[0].x, (int)selectedTiles[0].y] = gridTiles[(int)selectedTiles[2].x, (int)selectedTiles[2].y];
                    gridTiles[(int)selectedTiles[2].x, (int)selectedTiles[2].y] = tmp;

                }
                gridTiles[(int)selectedTiles[0].x, (int)selectedTiles[0].y].TileMechanics.changeGridPosition(selectedTiles[0], true);
                gridTiles[(int)selectedTiles[1].x, (int)selectedTiles[1].y].TileMechanics.changeGridPosition(selectedTiles[1], true);
                gridTiles[(int)selectedTiles[2].x, (int)selectedTiles[2].y].TileMechanics.changeGridPosition(selectedTiles[2], true);

                yield return new WaitForSeconds(0.1f);

                yield return StartCoroutine(checkForBubbles());

                //If rotation catch a bubble so we do deselection and selectionGroup be nulled.
                if (this.selectionGroup.transform.position == Vector3.zero)
                    break;
            }

            //If rotation bubble something so the bombs need countdown and check for is bomb die.
            if (this.selectionGroup.transform.position == Vector3.zero)
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

        yield return new WaitForSeconds(0.1f);
        while (BubbleIt())
        {
            deSelectGroup();
            shiftGrid(true);
            for (int i = 0; i < gSetting.GridSize.y; i++)
            {
                for (int j = 0; j < gSetting.GridSize.x; j++)
                {
                    if(gridTiles[j, i].TileMechanics.IsNeedMovement())
                        yield return new WaitWhile(gridTiles[j, i].TileMechanics.IsNeedMovement);
                    
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private bool BubbleIt()
    {
        bool isNeedBubble = false;
        List<Vector2> bubbleList = new List<Vector2>();

        for (int i = 0; i < gSetting.GridSize.x; i++)
        {
            for (int j = 0; j < gSetting.GridSize.y; j++)
            {
                for (int k = 0; k < 6; k++)
                {
                    Vector3[] selections = calculateGroup(new Vector2(i, j), k, false);
                    if (gridTiles[i, j].tileColor.Equals(gridTiles[(int)selections[0].x, (int)selections[0].y].tileColor)
                    && gridTiles[(int)selections[0].x, (int)selections[0].y].tileColor.Equals(gridTiles[(int)selections[1].x, (int)selections[1].y].tileColor))
                    {
                        bubbleList.Add(new Vector2(i, j));
                        bubbleList.Add(selections[0]);
                        bubbleList.Add(selections[1]);
                    }
                }
                
            }
        }
        //Clear same coords
        bubbleList = bubbleList.Distinct().ToList();

        //Find Bomb Tiles
        List<Color32> bColors = new List<Color32>();
        foreach (var item in bubbleList)
        {
            if(gridTiles[(int)item.x, (int)item.y].BombCountdown > 0)
            {
                bColors.Add(gridTiles[(int)item.x, (int)item.y].tileColor);
            }
        }
        
        bColors = bColors.Distinct().ToList();

        //Add Bomb Color Tiles
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

        if (bubbleList.Count > 0)
            deSelectGroup();

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
            uiController.playSfx(SfxTypes.BubbleScs);

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
                tmp.Tile = tmpObj;
                tmp.TileMechanics.deSelect();
                tmp.BombCountdown = bombCountDown;
                tmp.tileColor = gSetting.GameColors[UnityEngine.Random.Range(0, gSetting.GameColors.Length)];
                tmp.Tile.GetComponent<SpriteRenderer>().color = tmp.tileColor;

                tmp.TileMechanics.setInitialPosition(new Vector2(-(startOffset - index), i));

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
                gridTiles[j, i].TileMechanics.changeGridPosition(new Vector2(j, i));
            }
        }
    }

    private void setGameOver() 
    {
        uiController.showGGMenu();
        isGameOver = true;
    }
}

public enum RotateDir { R, UR, UL, L, LD, RD }