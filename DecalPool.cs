using UnityEngine;
using System.Collections;

namespace CubePainter
{
    [System.Serializable]
    public class DecalPool
    {
        public Material material;
        public string decalPropName = "_Decal";
        public int poolSize = 3;
        public int hash;
        Color32[] defaultColor;
        DecalTexture[] _pool;
        Texture2D _source;

        public void Init()
        {
            _source = material.GetTexture(decalPropName) as Texture2D;
            _pool = new DecalTexture[poolSize];
            for (int i = 0; i < _pool.Length; i++)
            {
                _pool[i] = new DecalTexture(_source, material, decalPropName);
            }
            defaultColor = _source.GetPixels32();
        }

        public void Clear()
        {
            for (int i = 0; i < _pool.Length; i++)
            {
                var p = _pool[i];
                if (p.isDirty)
                {
                    p.texture.SetPixels32(defaultColor);
                    p.texture.Apply();
                }
            }
        }

        public DecalTexture Pick()
        {
            for (int i = 0; i < _pool.Length; i++)
            {
                var t = _pool[i];
                if (!t.isDirty)
                {
                    t.isDirty = true;
                    return t;
                }
            }
            return new DecalTexture(_source, material, decalPropName);
        }

        public class DecalTexture
        {
            public Texture2D texture;
            public Material material;
            public bool isDirty;

            public DecalTexture(int h, int w, Material mat, string propName = "_Decal")
            {
                texture = new Texture2D(w, h, TextureFormat.RGB24, true);
                material = Object.Instantiate(mat);
                material.SetTexture(propName, texture);
                isDirty = false;
            }

            public DecalTexture(Texture2D source, Material mat, string propName = "_Decal")
            {
                material = Object.Instantiate(mat);
                texture = Object.Instantiate(source);
                material.SetTexture(propName, texture);
                isDirty = false;
            }
        }
    }

}
