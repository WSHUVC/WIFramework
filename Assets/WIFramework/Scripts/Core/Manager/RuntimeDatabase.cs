using System.Collections.Generic;
using UnityEngine;

namespace WIFramework
{
    public class RuntimeDatabase
    {
        HashSet<MonoBehaviour> activeBehaviours = new HashSet<MonoBehaviour>();
        
        public bool AddBehaviour(MonoBehaviour mono)
        {
            bool result = activeBehaviours.Add(mono);

            if(result)
            {
                Cleaning();
            }
            
            return result;
        }

        void Cleaning()
        {
        }
    }
}