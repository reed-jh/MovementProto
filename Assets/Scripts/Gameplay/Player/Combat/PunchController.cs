using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Punched " + other.name);

        // Check if punched enemy
        if (other.gameObject.GetComponent<EnemyController>() != null)
        {
            // Does nothing right now
            EnemyController enemy = other.gameObject.GetComponent<EnemyController>();
            //enemy.combat.TakeDamage(10); // make damage amount configurable

            float r = (float)Random.Range(0, 255) / 255f;
            float g = (float)Random.Range(0, 255) / 255f;
            float b = (float)Random.Range(0, 255) / 255f;

            other.gameObject.GetComponent<SpriteRenderer>().color = new Color(r,g,b);
            Debug.Log("Color: " + r + ", " + g + ", " + b);
        }
    }
}
