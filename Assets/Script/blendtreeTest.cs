using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class blendtreeTest : MonoBehaviour
{
    public Animator animator;

    public float X, Y;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("HorizontalMove",X);
        animator.SetFloat("VerticalMove",Y);
    }
}
