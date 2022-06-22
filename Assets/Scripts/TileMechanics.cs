using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TileMechanics : MonoBehaviour
{
    [SerializeField]
    private GameSettings gSetting;
    [SerializeField]
    private Collider2D bCollider;
    [SerializeField]
    private SpriteRenderer backMask;
    [SerializeField]
    private TextMeshPro guiText;
    [SerializeField]
    private ParticleSystem bubbleEffect;

    Vector2 gridPosition = -Vector2.one;
    Vector3 prevObjectPosition = -Vector3.one;
    Vector3 nextObjectPosition = -Vector3.one;

    Vector3 tileSize = Vector3.one;


    private bool isTileBomb = false;
    private int bombCountdown = 0;

    private bool isNeedMovement = false;
    private float movementOffset = 0f;

    public Vector3 TileSize { get => tileSize;  }

    public bool IsNeedMovement () { return isNeedMovement; }

    // Start is called before the first frame update
    void Awake()
    {
        gameObject.transform.localScale = Vector3.one * gSetting.ScreenRatio;
        tileSize = Vector3.Scale(bCollider.bounds.size, gameObject.transform.localScale);
    }

    private void Update()
    {

        if (Vector3.Distance(gameObject.transform.localPosition, nextObjectPosition) >= 0f && isNeedMovement)
        {
            movementOffset += Time.deltaTime * gSetting.GameSpeed;
            
            this.gameObject.transform.localPosition = Vector3.Lerp(prevObjectPosition, nextObjectPosition, movementOffset);
            if (movementOffset >= 1f)
            {
                this.gameObject.transform.localPosition = nextObjectPosition;
                movementOffset = 0f;
                isNeedMovement = false;
            }
        }
    }

    void OnDestroy()
    {
        if(bubbleEffect != null && this.gameObject.transform.parent != null)
        {
            bubbleEffect.transform.SetParent(this.gameObject.transform.parent, true);
            bubbleEffect.Play();
        }

    }
    public void setBombSituat(bool isBomb, int bombCounter)
    {
        isTileBomb = isBomb;
        bombCountdown = bombCounter;

        if (isBomb)
        {
            if (guiText != null)
            {
                guiText.text = "" + bombCounter;
            }
        }
    }
    public void setInitialPosition(Vector2 newPosition) 
    {
        gridPosition = newPosition;
        nextObjectPosition = new Vector3(tileSize.x * (3f/ 4f) * gridPosition.y, -tileSize.y * gridPosition.x + (gridPosition.y % 2 == 1 ? ( -tileSize.y / 2f) : 0f), 0f);
        gameObject.transform.localPosition = nextObjectPosition;

        gameObject.name = gridPosition.x + "x" + gridPosition.y;
    }
    public void changeGridPosition(Vector2 newPosition, bool isDirect = false) 
    {
        gridPosition = newPosition;
        gameObject.name = gridPosition.x + "x" + gridPosition.y;

        if (isDirect)
        {

            return;
        }

        prevObjectPosition = this.transform.localPosition;
        nextObjectPosition = new Vector3(tileSize.x * (3f / 4f) * gridPosition.y, -tileSize.y * gridPosition.x + (gridPosition.y % 2 == 1 ? (-tileSize.y / 2f) : 0f), 0f);

        isNeedMovement = true;
    }
    internal void Select()
    {
        backMask.color = new Color32(255, 255, 255, 255);
        gameObject.transform.position += Vector3.forward * -0.2f;
    }
    internal void deSelect()
    {
        transform.rotation = Quaternion.identity;
        backMask.color = new Color32(255, 255, 255, 0);
        gameObject.transform.position += Vector3.forward * 0.2f;
    }
}
