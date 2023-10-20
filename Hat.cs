using System.Collections;
using UnityEngine;

public class Mol : MonoBehaviour {
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

    [Header("GameManager")]
  [SerializeField] private GameManager gameManager;

  // The offset of the sprite to hide it.
  private Vector2 startPosition = new Vector2(0f, -2.56f);
  private Vector2 endPosition = Vector2.zero;
  // How long it takes to show a mole.
  public float showDuration = 0.5f;//how long the hat stays out for
  public float duration = 1f;//how long you have before the hat goes away

  private SpriteRenderer spriteRenderer;
  private Animator animator;
  private BoxCollider2D boxCollider2D;
  private Vector2 boxOffset;
  private Vector2 boxSize;
  private Vector2 boxOffsetHidden;
  private Vector2 boxSizeHidden;

  private bool hittable = true;
  public enum HatType { regular, tank, rare, cat };
  private HatType hatType;
  public float tankRate = 0.25f;//how often the tank hat will spawn
  private float rareRate = 0.5f;//how often the rare hat will spawn
  public float catRate = 0.25f;//how often the cat will spawn
  private int lives;//how much hits the tank hat can take
  private int hatIndex = 0;

  private IEnumerator ShowHide(Vector2 start, Vector2 end) {
    // Make sure we start at the start.
    transform.localPosition = start;

    // Show hat
    float elapsed = 0f;
    while (elapsed < showDuration) {
      transform.localPosition = Vector2.Lerp(start, end, elapsed / showDuration);
      boxCollider2D.offset = Vector2.Lerp(boxOffsetHidden, boxOffset, elapsed / showDuration);
      boxCollider2D.size = Vector2.Lerp(boxSizeHidden, boxSize, elapsed / showDuration);
      // Update at max framerate.
      elapsed += Time.deltaTime;
      yield return null;
    }

    // Make sure we're exactly at the end.
    transform.localPosition = end;
    boxCollider2D.offset = boxOffset;
    boxCollider2D.size = boxSize;

    // Wait for duration to pass.
    yield return new WaitForSeconds(duration);

    // Hide hats
    elapsed = 0f;
    while (elapsed < showDuration) {
      transform.localPosition = Vector2.Lerp(end, start, elapsed / showDuration);
      boxCollider2D.offset = Vector2.Lerp(boxOffset, boxOffsetHidden, elapsed / showDuration);
      boxCollider2D.size = Vector2.Lerp(boxSize, boxSizeHidden, elapsed / showDuration);
      // Update at max framerate.
      elapsed += Time.deltaTime;
      yield return null;
    }
    // Make sure we're exactly back at the start position.
    transform.localPosition = start;
    boxCollider2D.offset = boxOffsetHidden;
    boxCollider2D.size = boxSizeHidden;

    // If we got to the end and it's still hittable then we missed it.
    if (hittable) {
      hittable = false;
      // We only give time penalty if it isn't a cat
      gameManager.Missed(hatIndex, hatType != HatType.cat);
    }
  }

  public void Hide() {
    // Set the appropriate hat parameters to hide it.
    transform.localPosition = startPosition;
    boxCollider2D.offset = boxOffsetHidden;
    boxCollider2D.size = boxSizeHidden;
  }

  private IEnumerator QuickHide() {
    yield return new WaitForSeconds(0.25f);
    // Whilst we were waiting we may have spawned again here, so just
    // check that hasn't happened before hiding it. This will stop it
    // flickering in that case.
    if (!hittable) {
      Hide();
    }
  }

  private void OnMouseDown() {
    if (hittable) {
      switch (hatType) {
        case HatType.regular:
          spriteRenderer.sprite = hatHit;
          gameManager.AddScore(hatIndex, 1);
          // Stop the animation
          StopAllCoroutines();
          StartCoroutine(QuickHide());
          // Turn off hittable so that we can't keep tapping for score.
          hittable = false;
          break;
        case HatType.tank:
          // If lives == 2 reduce, and change sprite.
          if (lives == 2) {
            spriteRenderer.sprite = hat2Damaged;
            lives--;
          }
           else {
            spriteRenderer.sprite = hat2Hit;
            gameManager.AddScore(hatIndex, 10);
            // Stop the animation
            StopAllCoroutines();
            StartCoroutine(QuickHide());
            // Turn off hittable so that we can't keep tapping for score.
            hittable = false;
          }
          break;
        case HatType.rare:
            spriteRenderer.sprite = hat3Hit;
            gameManager.AddScore(hatIndex, 100);
            StopAllCoroutines();
            StartCoroutine(QuickHide());
            hittable = false;
            break;
        case HatType.cat:
            spriteRenderer.sprite = catHit;
            gameManager.timeRemaining -= 6;
            StopAllCoroutines();
            StartCoroutine(QuickHide());
            hittable = false;
            break;
        default:
            break;
      }
    }
  }

   private void CreateNext()
    {
        float random = Random.Range(0f, 1f);
        if (random < tankRate)
        {
            hatType = HatType.tank;
            spriteRenderer.sprite = hat2;
            lives = 2;//2 can be changed for however many hits we wanna use for the hat
        }
        else if (random == rareRate)
        {
            hatType = HatType.rare;
            spriteRenderer.sprite = hat3;
            lives = 1;
        }
        else if (random < catRate)
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

    // As the level progresses the game gets harder.
    private void SetLevel(int level) {
    // As level increases increse the cat rate to 0.25 at level 10.
    catRate = Mathf.Min(level * 0.025f, 0.25f);

    // Increase the amounts of tanks until 100% at level 40.
    catRate = Mathf.Min(level * 0.025f, 1f);

    // Duration bounds get quicker as we progress. No cap on insanity.
    float durationMin = Mathf.Clamp(1 - level * 0.1f, 0.01f, 1f);
    float durationMax = Mathf.Clamp(2 - level * 0.1f, 0.01f, 2f);
    duration = Random.Range(durationMin, durationMax);
  }

  private void Awake() {
    // Get references to the components we'll need.
    spriteRenderer = GetComponent<SpriteRenderer>();
    animator = GetComponent<Animator>();
    boxCollider2D = GetComponent<BoxCollider2D>();
    // Work out collider values.
    boxOffset = boxCollider2D.offset;
    boxSize = boxCollider2D.size;
    boxOffsetHidden = new Vector2(boxOffset.x, -startPosition.y / 2f);
    boxSizeHidden = new Vector2(boxSize.x, 0f);
    }

  public void Activate(int level) {
    SetLevel(level);
    CreateNext();
    StartCoroutine(ShowHide(startPosition, endPosition));
  }

  // Used by the game manager to uniquely identify hats
  public void SetIndex(int index) {
    hatIndex = index;
  }

  // Used to freeze the game on finish.
  public void StopGame() {
    hittable = false;
    StopAllCoroutines();
  }
}
