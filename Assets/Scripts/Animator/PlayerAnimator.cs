using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    Animator animator;
    Player player;
    SpriteRenderer spriteRenderer;

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GetComponent<Player>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player.MovementDirection.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (player.MovementDirection.x > 0)
        {
            spriteRenderer.flipX = false;
        }

        if (player.MovementDirection.x != 0 || player.MovementDirection.y != 0)
        {
            animator.SetBool("Walk", true);
        }
        else
        {
            animator.SetBool("Walk", false);
        }
    }
    
    public void PlayHurtAnimation()
    {
        animator.SetTrigger("Hurt");
    }
}
