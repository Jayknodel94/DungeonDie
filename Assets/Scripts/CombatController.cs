using UnityEngine;

public class CombatController : MonoBehaviour
{
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(Controls.Melee))
        {
            animator.SetTrigger("melee");
        }
        else if (Input.GetMouseButtonDown(Controls.MeleeHeavy))
        {
            animator.SetTrigger("meleeHeavy");
        }
    }
}
