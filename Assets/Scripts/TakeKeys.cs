using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeKeys : MonoBehaviour
{
    public int numberOfKeys =0;
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Coin"))
        {
            numberOfKeys++;
            Destroy(collision.gameObject);
        }

    }
}
