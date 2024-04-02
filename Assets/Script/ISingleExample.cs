using UnityEngine;
using WI;

public class ISingleExample : MonoBehaviour, ISingle
{
   void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Instantiate(gameObject);
        }
    }
}
