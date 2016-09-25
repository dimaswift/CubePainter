using UnityEngine;
using System.Collections.Generic;

namespace CubePainter
{
    [System.Serializable]
    public class DecalPool : MonoBehaviour
    {
        static DecalPool m_instance;

        readonly Dictionary<int, Color32[]> m_blankCache = new Dictionary<int, Color32[]>();

        bool m_loaded;

        public Color32[] GetBlankColors(Texture2D tex)
        {
            var pixelCount = tex.width * tex.height;
            Color32[] result = null;
            m_blankCache.TryGetValue(pixelCount, out result);
            return result;
        }


        public static DecalPool instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = FindObjectOfType<DecalPool>();
                    if (m_instance == null)
                        m_instance = new GameObject("Decal Pool").AddComponent<DecalPool>();
                    DontDestroyOnLoad(m_instance.gameObject);
                    m_instance.Init();
                }
                return m_instance;
            }
        }


        Dictionary<DecalSize, Pool> m_pool;

        public void RegisterDecal(Decal canvas)
        {
            var size = new DecalSize(canvas.decalTexture.width, canvas.decalTexture.height);
            var pixelCount = size.width * size.height;
            if (!m_blankCache.ContainsKey(pixelCount))
            {
                var colorArray = new Color32[pixelCount];
                m_blankCache.Add(pixelCount, colorArray);
            }
            if (m_pool.ContainsKey(size))
            {
                var pool = m_pool[size];
                pool.AddMaterial();
            }
            else
            {
                var pool = new Pool(canvas.decalMaterial, 10);
                m_pool.Add(size, pool);
            }
        }

        public Material PickMaterial(Decal canvas, out bool isDirty)
        {
            var size = new DecalSize(canvas.decalTexture.width, canvas.decalTexture.height);
            Pool pool;
            if (m_pool.TryGetValue(size, out pool))
            {
                return pool.Pick(out isDirty);
            }
            isDirty = false;
            return null;
        }

        public void Init()
        {
            m_pool = new Dictionary<DecalSize, Pool>(100);

        }

        public struct DecalSize
        {
            public int width, height;
            public DecalSize(int width, int height)
            {
                this.width = width;
                this.height = height;
            }
        }

        public class Pool
        {
            class PooledMaterial
            {
                public Material material;
                public bool isDirty = false;
            }
            int order = 0;

            List<PooledMaterial> materials;
            Material source;

            public Pool(Material mat, int capacity)
            {
                source = mat;
                materials = new List<PooledMaterial>(capacity);
                AddMaterial();
            }

            public Material Pick(out bool isDirty)
            {
                if (order >= materials.Count)
                {
                    order = 0;
                }
                var m = materials[order++];
                isDirty = m.isDirty;
                m.isDirty = true;
                return m.material;
            }

            public void AddMaterial()
            {
                var newMat = Instantiate(source);
                newMat.name = "pooled material";
                var newDecal = Instantiate(newMat.GetTexture(Decal.DECAL_PROP_NAME)) as Texture2D;
                newDecal.SetPixels32(DecalPool.instance.GetBlankColors(newDecal));
                newDecal.Apply();
                newMat.SetTexture(Decal.DECAL_PROP_NAME, newDecal);
                materials.Add(new PooledMaterial() { material = newMat });
            }
        }

    }

}
