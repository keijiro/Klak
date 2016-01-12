using UnityEngine;
using System.Collections;

namespace Klak.Test
{
    public class RandomWalker : MonoBehaviour
    {
        [SerializeField] float _radius = 5;
        [SerializeField] float _interval = 1;
        [SerializeField] bool _rotation = true;

        IEnumerator Start()
        {
            while (true)
            {
                transform.position = Random.insideUnitSphere * _radius;
                if (_rotation) transform.rotation = Random.rotation;
                yield return new WaitForSeconds(_interval);
            }
        }
    }
}
