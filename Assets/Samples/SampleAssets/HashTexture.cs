using UnityEngine;
using Klak.Math;

public class HashTexture : MonoBehaviour
{
    [SerializeField] int size = 512;

    void Start()
    {
        var hash = new XXHash();
        var texture = new Texture2D(size, size);

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var c = hash.Value01(x + y * size);
                texture.SetPixel(x, y, new Color(c, c, c));
            }
        }

        texture.Apply();

        GetComponent<Renderer>().material.mainTexture = texture;
    }
}
