using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuicideWhenNoChildren : MonoBehaviour
{
    void Start()
    {
        InvokeRepeating("KillSelfIfNoChildren", 5f, 5f);
    }

    private void KillSelfIfNoChildren()
    {
        if(transform.childCount == 0)
        {
            Destroy(gameObject);
        }
    }
}
