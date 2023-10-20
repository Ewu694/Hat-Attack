using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mole : MonoBehaviour
{
    [Header("Graphics")]
    [SerializeField] private Sprite hat;//regular hat
    [SerializeField] private Sprite hat2;//hat that can take 2 hits
    [SerializeField] private Sprite hat3;//hat that shows up rarely and has a short duration to hit
    [SerializeField] private Sprite cat;//cat that will cause player to lose score on hit
    [SerializeField] private Sprite hatHit;
    [SerializeField] private Sprite hat2Hit;
    [SerializeField] private Sprite hat3Hit;
    [SerializeField] private Sprite catHit;
    [SerializeField] private Sprite hat2Damaged;

    [SerializeField] private GameManager gameManager;


    [Header("Sprite Offset")]//offset to hide the sprite
    private Vector2 startPosition = new Vector2(0f, -2.56f);
    private Vector2 endPosition = Vector2.zero;

    [Header("Pup-up Duration")]//duration a hat shows up for
    public float showDuration = 0.5f;
    public float duration = 1f;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider2D;
    private Vector2 boxOffset;
    private Vector2 boxSize;
    private Vector2 boxOffsetHidden;
    private Vector2 boxSizeHidden;

    private bool hittable = true;
    public enum HatType { regular, tank, rare, cat };
    private HatType hatType;
    public float tankRate = 0.25f;//how often the tank hat will spawn
    private float rareRate = 0.125f;//how often the rare hat will spawn
    public float catRate = 0f;//how often the cat will spawn
    private int lives;//how much hits the tank hat can take
    private int hatIndex = 0;

    public void SetIndex(int index)
    {
        hatIndex = index;
    }

    public void StopGame()
    {
        hittable = false;
        StopAllCoroutines();
    }
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        boxCollider2D = GetComponent<BoxCollider2D>();
        boxOffset = boxCollider2D.offset;
        boxSize = boxCollider2D.size;
        boxOffsetHidden = new Vector2(boxOffset.x, -startPosition.y / 2f);
        boxSizeHidden = new Vector2(boxSize.x, 0f);
    }

    // Start is called before the first frame update
    public void Activate(int level)
    {
        SetLevel(level);
        CreateNext();
        StartCoroutine(ShowHide(startPosition, endPosition));
    }

    private void OnMouseDown()
    {
        if(hittable)
        {
            switch (hatType)
            {
                case HatType.regular:
                    spriteRenderer.sprite = hatHit;
                    gameManager.AddScore(hatIndex);
                    StopAllCoroutines();
                    StartCoroutine(QuickHide());
                    hittable = false;
                    break;
                case HatType.tank:
                    if (lives == 2)
                    {
                        spriteRenderer.sprite = hat2Damaged;
                        lives--;
                    }
                    else
                    {
                        spriteRenderer.sprite = hat2Hit;
                        StopAllCoroutines();
                        StartCoroutine(QuickHide());
                        hittable = false;
                    }
                    break;
                case HatType.rare:
                    spriteRenderer.sprite = hat3Hit;
                    gameManager.AddScore(50);
                    StopAllCoroutines();
                    StartCoroutine(QuickHide());
                    hittable = false;
                    break;
                case HatType.cat:
                    spriteRenderer.sprite = cat;
                    gameManager.timeRemaining -= 6;
                    StopAllCoroutines();
                    StartCoroutine(QuickHide());
                    hittable = false;
                    break;

                default:
                    break;
            }
        }
        StopAllCoroutines();
        StartCoroutine(QuickHide());
        hittable = false;
    }

    private IEnumerator ShowHide(Vector2 start, Vector2 end)
    {
        transform.localPosition = start;//make sure the hat starts in the right positoin
        float elapsed = 0f;
        while(elapsed < showDuration)
        {
            transform.localPosition = Vector2.Lerp(start, end, elapsed / showDuration);
            boxCollider2D.offset = Vector2.Lerp(boxOffsetHidden, boxOffset, elapsed / showDuration);
            boxCollider2D.size = Vector2.Lerp(boxSizeHidden, boxSize, elapsed / showDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = end;
        yield return new WaitForSeconds(duration);

        elapsed = 0f;
        while(elapsed < showDuration)//same loop for after the hat is hit and needs to be hidden again
        {
            transform.localPosition = Vector2.Lerp(end, start, elapsed / showDuration);
            boxCollider2D.offset = Vector2.Lerp(boxOffsetHidden, boxOffset, elapsed / showDuration);
            boxCollider2D.size = Vector2.Lerp(boxSizeHidden, boxSize, elapsed / showDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = start;//resets position
        boxCollider2D.offset = boxOffsetHidden;
        boxCollider2D.size = boxSizeHidden;

        if (hittable)
        {
            hittable = false;
            gameManager.Missed(hatIndex, hatType != HatType.cat);
        }
    }

    private IEnumerator QuickHide()
    {
        yield return new WaitForSeconds(0.25f);
        if(!hittable)
        {
            Hide();
        }
    }

    public void Hide()
    {
        transform.localPosition = startPosition;
        boxCollider2D.offset = boxOffsetHidden;
        boxCollider2D.size = boxSizeHidden;
    }

    private void CreateNext()
    {
        float random = Random.Range(01, 1f);
        if(random < tankRate)
        {
            hatType = HatType.tank;
            spriteRenderer.sprite = hat2;
            lives = 2;//2 can be changed for however many hits we wanna use for the mole
        }
        else if(random > rareRate)
        {
            hatType = HatType.rare;
            spriteRenderer.sprite = hat3;
            lives = 1;
        }
        else if(random > catRate)
        {
            hatType = HatType.cat;
            spriteRenderer.sprite = cat;
            lives = 1;
        }
        else
        {
            hatType = HatType.regular;
            spriteRenderer.sprite = hat;
            lives = 1;
        }
        hittable = true;
    }

    private void SetLevel(int level)
    {
        catRate = Mathf.Min(level * 0.03f, 0.3f);//as the score increases and level increases, cat spawn rate
        tankRate = Mathf.Min(level * 0.05f, 1f);//change 0.5 depending on how often the tanks show up

        float durationMin = Mathf.Clamp(1 - level * 0.1f, 0.01f, 1f);//can change the 0.01 to make the game easier/harder 
        float durationMax = Mathf.Clamp(2 - level * 0.1f, 0.01f, 2f);
        duration = Random.Range(durationMin, durationMax);//duration gets quicker as time goes
    }
}
