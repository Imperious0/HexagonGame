using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TileMechanics : MonoBehaviour
{
    [SerializeField]
    private GameSettings gSetting;

    Vector2 gridPosition = -Vector2.one;
    Vector3 prevGridPosition = -Vector3.one;
    Vector3 nextGridPosition = -Vector3.one;

    private GameObject guiText;
    private bool isTileBomb = false;
    private int bombCountdown = 0;

    private bool isNeedMovement = false;
    private float movementOffset = 0f;

    public bool IsNeedMovement () { return isNeedMovement; }

    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.localScale = Vector3.one * gSetting.ScreenRatio;
    }

    private void FixedUpdate()
    {
        if (this.guiText != null)
        {
            if(!Camera.main.WorldToScreenPoint(this.transform.position).Equals(Camera.main.WorldToScreenPoint(this.guiText.transform.position)))
                this.guiText.transform.position = Camera.main.WorldToScreenPoint(this.transform.position);
        
        }
        if (!transform.position.Equals(nextGridPosition) && isNeedMovement)
        {
            movementOffset += Time.fixedDeltaTime;
            
            this.gameObject.transform.localPosition = Vector3.Lerp(prevGridPosition, nextGridPosition, movementOffset / gSetting.GameSpeed);
            if ((movementOffset / gSetting.GameSpeed) > 1f)
            {
                this.gameObject.transform.localPosition = nextGridPosition;
                movementOffset = 0f;
                isNeedMovement = false;
            }
        }
    }

    void OnDestroy()
    {
        if (GameObject.Find("bubbleEffect"))
        {
            GameObject go = Instantiate(GameObject.Find("bubbleEffect"), this.transform.parent, true);
            go.transform.position = this.transform.position;
            go.GetComponent<ParticleSystem>().Play();
        }


        GameObject.DestroyImmediate(this.guiText);
    }
    public void setBombSituat(bool isBomb, int bombCounter)
    {
        isTileBomb = isBomb;
        bombCountdown = bombCounter;

        if (isBomb)
        {
            if (guiText == null)
            {
                guiText = Instantiate(GameObject.Find("text_TMP"), GameObject.Find("Canvas").transform, false);
                guiText.name = this.gameObject.name + "_Text";
                guiText.GetComponent<TextMeshProUGUI>().text = "" + bombCounter;

            }
            else
            {
                guiText.GetComponent<TextMeshProUGUI>().text = "" + bombCounter;
            }
        }
    }
    public void setInitialPosition(Vector2 newPosition) 
    {
        nextGridPosition = new Vector3(gSetting.GridBaseIncrement.x * newPosition.y, gSetting.GridBaseIncrement.y * newPosition.x + (newPosition.y % 2 == 1 ? -0.425f * gSetting.ScreenRatio : 0f), 0f);
        this.gameObject.transform.localPosition = nextGridPosition;
        gridPosition = newPosition;
        this.gameObject.name = gridPosition.x + "x" + gridPosition.y;

    }
    public void changeGridPosition(Vector2 newPosition) 
    {
        prevGridPosition = this.transform.localPosition;
        nextGridPosition = new Vector3(gSetting.GridBaseIncrement.x * newPosition.y, gSetting.GridBaseIncrement.y * newPosition.x + (newPosition.y % 2 == 1 ? -0.425f * gSetting.ScreenRatio : 0f), 0f); 
        gridPosition = newPosition;
        this.gameObject.name = gridPosition.x + "x" + gridPosition.y;
        isNeedMovement = true;
    }
}
