using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CombatSystem : MonoBehaviour
{
    // Reference to enclosing controller
    protected ObjectController parent;

    //public CombatSystem(ObjectController p)
    //{
    //    parent = p;
    //}

    [SerializeField] public int health;

    public void TakeDamage(int amt)
    {
        health -= amt;
        if (health <= 0)
        {
            parent.Die();
        }
    }
}
