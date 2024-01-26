using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemy_move : MonoBehaviour
{
    public float speed;

    public bool isRight;

    public float counterT;

    public float timeforChange = 4f;

    public Rigidbody2D rbd; 

    // Start is called before the first frame update
    void Start()
    {
        counterT = timeforChange; 
    }

    // Update is called once per frame
    void Update()
    {
        if (isRight == true)
        {
            transform.position += Vector3.right * speed * Time.deltaTime; 
            transform.localScale = new Vector3 (-1, 1, 1);
        }
        if (isRight == false) 
        {
            transform.position += Vector3.left * speed * Time.deltaTime;
            transform.localScale = new Vector3(1, 1, 1);
        }

        counterT -= Time.deltaTime;

        if (counterT < 0)
        {
            counterT = timeforChange;
            isRight = !isRight; 
        }
    }
}
