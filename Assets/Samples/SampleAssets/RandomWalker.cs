using UnityEngine;
using System.Collections;

namespace Klak.Test
{
    public class RandomWalker : MonoBehaviour
    {
        [SerializeField] Vector3 _range = Vector3.one;
        [SerializeField] float _interval = 1;
        [SerializeField] bool _rotation = true;

        IEnumerator Start()
        {
            var p0 = transform.position;
            while (true)
            {
                var pr = new Vector3(Random.value, Random.value, Random.value);
                pr = pr * 2 - Vector3.one;
                transform.position = p0 + Vector3.Scale(pr, _range);
                if (_rotation) transform.rotation = Random.rotation;
                yield return new WaitForSeconds(_interval);
            }
        }
    }
}
