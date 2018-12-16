using System;
using UnityEngine;

namespace RLCreature.Sample.Driving
{
    public class BackToCenterBehaviour: MonoBehaviour
    {
        private void Update()
        {
            if (Math.Abs(transform.position.x) > 4500 || Math.Abs(transform.position.z) > 4500)
            {
                transform.position = new Vector3(0, 0, 0);
            }
        }
    }
}