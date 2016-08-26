using UnityEngine;
using System.Collections.Generic;

namespace CubePainter
{
    public static class DecalManager
    {
        static readonly Dictionary<int, Color32[]> m_blankCache = new Dictionary<int, Color32[]>();

        static bool m_loaded;

        static readonly Dictionary<Material, DecalPool> m_pools = new Dictionary<Material, DecalPool>();

        public static void RegisterDecal(Decal decal)
        {
            var pixelCount = decal.decalTexture.width * decal.decalTexture.height;
            if(!m_blankCache.ContainsKey(pixelCount))
            {
                var colorArray = new Color32[pixelCount];
                m_blankCache.Add(pixelCount, colorArray);
            }
            if (!m_pools.ContainsKey(decal.meshRenderer.sharedMaterial))
            {
                var pool = new DecalPool(decal.meshRenderer.sharedMaterial, 10);
                m_pools.Add(decal.meshRenderer.sharedMaterial, pool);
            }
        }

        public static Color32[] GetBlankColors(Texture2D tex)
        {
            var pixelCount = tex.width * tex.height;
            Color32[] result = null;
            m_blankCache.TryGetValue(pixelCount, out result);
            return result;
        }

        public static void ClearAll()
        {
            foreach (var item in m_pools)
            {
                item.Value.Clear();
            }
        }

        public static DecalPool GetPool(Material mat)
        {
            return m_pools[mat];
        }
    }
}

