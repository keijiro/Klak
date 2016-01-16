using UnityEngine;
using System.Collections;
using Klak.MaterialExtension;

namespace Klak.Test
{
    public class MaterialChanger : MonoBehaviour
    {
        MaterialPropertyBlock _materialProperties;

        void ChangeMaterial()
        {
            _materialProperties.
                Property("_Color", RandomColor()).
                Property("_Glossiness", Random.value).
                Property("_Metallic", Random.value);
        }

        Color RandomColor()
        {
            return new Color(Random.value, Random.value, Random.value);
        }

        IEnumerator Start()
        {
            _materialProperties = new MaterialPropertyBlock();
            while (true)
            {
                ChangeMaterial();
                GetComponent<Renderer>().SetPropertyBlock(_materialProperties);
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
