using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemy_fly : MonoBehaviour
{
    public Transform objetive;

    public float speed;

    public bool mustPersue;

    public float distance;

    public float distanceAbsolut; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        distance = objetive.position.x - transform.position.x;

        distanceAbsolut = Mathf.Abs(distance);

        if (mustPersue == true)
        {
            transform.position = Vector2.MoveTowards(transform.position, objetive.position, speed * Time.deltaTime); 
        }

        if (distance > 0) 
        {
            transform.localScale = new Vector3(1, 1, 1); 
        }

        if (distance < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); 
        }

        if (distanceAbsolut < 5)
        {
            mustPersue = true; 
        }    
        else
        {
            mustPersue = false;
        }
    }
}
