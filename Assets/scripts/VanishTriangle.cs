using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VanishTriangle : MonoBehaviour
{
    private SpriteRenderer sr;
    private Collider2D col;

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Vanish()
    {
        sr.enabled = false;
        col.enabled = false;
    }

    public void Appear()
    {
        sr.enabled = true;
        col.enabled = true;
    }
}
