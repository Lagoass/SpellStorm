using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    Animator animator;
    Enemy enemy;
    SpriteRenderer spriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        enemy = GetComponent<Enemy>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (enemy.MoveDirection.x > 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (enemy.MoveDirection.x < 0)
        {
            spriteRenderer.flipX = false;
        }
    }

    public void PlayHurtAnimation()
    {
        animator.SetTrigger("Hurt");
    }

    //Add a blue color over the sprite for 0.2 seconds
    public void PlayFreezeAnimation()
    {
        StartCoroutine(FreezeEffect());
    }
    
    private System.Collections.IEnumerator FreezeEffect()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.cyan;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = originalColor;
    }
}
