using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public GameObject player;
    public GameObject portalLinked;
    public int keysForThisPortal = 0;
    public int playerNumberOfKeys;

    TakeKeys takeKeys;

    private void Start()
    {
        takeKeys = player.GetComponent<TakeKeys>();
    }

    public void Update()
    {
        playerNumberOfKeys = takeKeys.numberOfKeys;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Debug.Log("Detecto jugador: " + player.name);

            if (portalLinked != null && keysForThisPortal == playerNumberOfKeys)
            {
                Debug.Log("Entro al if");
                player.transform.position = portalLinked.transform.position;
                Debug.Log("Transporto al jugador");
            }
            else if (portalLinked == null)
            {
                Debug.Log(portalLinked.name);
            }
            else
            {
                Debug.Log("Nada, absolutamente nada");
            }
        }
    }
}
