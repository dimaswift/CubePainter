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
        }

        public DecalPool(Material mat, int poolSize, string propName = "_Decal")
        {
            this.poolSize = poolSize;
            material = mat;
            Init();
        }

        public void Clear()
        {
            for (int i = 0; i < _pool.Length; i++)
            {
                var p = _pool[i];
                if (p.isDirty)
                {
                    p.texture.SetPixels32(DecalManager.GetBlankColors(p.texture));
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
            Debug.LogWarning(string.Format(">>>>>>>(Allocation warning!) Pool {0} has ran out of clean textures. Creating new texture runtime...", _source.name)); 
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
