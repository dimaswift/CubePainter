using UnityEngine;


namespace CubePainter
{
    public class DecalManager : MonoBehaviour
    {
        static DecalManager _instance;
        public static DecalManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<DecalManager>();
                    _instance.Awake();
                }
                return _instance;
            }
        }

        bool loaded;

        public DecalPool[] pools;

        public void ClearAll()
        {
            for (int i = 0; i < pools.Length; i++)
            {
                pools[i].Clear();
            }
        }

        public DecalPool GetPool(int index)
        {
            return pools[index];
        }

        public void Awake()
        {
            if (!loaded)
            {
                for (int i = 0; i < pools.Length; i++)
                {
                    pools[i].Init();
                }
                _instance = this;
                loaded = true;
            }
        }
    }
}

