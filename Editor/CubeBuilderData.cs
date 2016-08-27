namespace CubePainter
{
    using UnityEngine;
    using HandyUtilities;
    using UnityEditor;

    public class CubeBuilderData : SerializedSingleton<CubeBuilderData>
    {
        const string PREVIEW_NAME = "Cube Builder Preview";
        public static bool HasInstance { get { return _instance != null; } }

        public Material mat;
        public Mesh mesh;
        public Texture2D texture;
        [SerializeField]
        [HideInInspector]
        bool m_initialized = false;
        GameObject m_preview;

        public GameObject preview
        {
            get
            {
                if(m_preview == null)
                {
                    m_preview = GameObject.Find(PREVIEW_NAME);
                    if(m_preview == null)
                    {
                        m_preview = new GameObject(PREVIEW_NAME);
                        var mf = m_preview.AddComponent<MeshFilter>();
                        var mr = m_preview.AddComponent<MeshRenderer>();
                        mf.sharedMesh = mesh;
                        mr.sharedMaterial = mat;
                        m_preview.transform.position = Vector3.zero;
                    }
                }
                return m_preview;
            }
        }

        public void Init()
        {
            if(!m_initialized)
            {
                mat = new Material(Shader.Find("CubePainter/Decal Diffuse Lit"));
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mesh = Instantiate(cube.GetComponent<MeshFilter>().sharedMesh);
                DestroyImmediate(cube);
                texture = UVUnwrapper.UVUnwrapData.GenerateTexture(128, 128, UVUnwrapper.UVUnwrapData.TextureType.Gradient, Color.black, Color.white);
               
                AssetDatabase.AddObjectToAsset(mesh, Instance);
                AssetDatabase.AddObjectToAsset(texture, Instance);
                AssetDatabase.AddObjectToAsset(mat, Instance);
                AssetDatabase.Refresh();
                mat.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GetAssetPath(texture));
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(Instance));
                m_initialized = true;
            }
        }
    }
}
