namespace CubePainter
{
    using UnityEngine;
    using System.Collections;
    using UnityEditor;
    using HandyUtilities;

    public class CubeBuilder : EditorWindow
    {
        PreviewCube m_cube;

        [MenuItem("Tools/CubePainter/Builder")]
        static void Open()
        {
            var w = GetWindow<CubeBuilder>(false, "Cube Builder", true);
            w.Show();
            w.maxSize = new Vector2(500, 500);
            HandyEditor.CenterOnMainWin(w);
            if(!CubeBuilderData.HasInstance)
            {
                var instance = CubeBuilderData.Instance;
                instance.Init();
            }

            w.m_cube = new PreviewCube();
        }

        void OnGUI()
        {
            var rect = EditorGUILayout.GetControlRect();
         
        }

        static Mesh CreateCubeMesh()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var mesh = cube.GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(cube);
            return Instantiate(mesh);
        }

        class PreviewCube
        {
            public GameObject gameObject;
            public Mesh mesh;
            public Material mat;
            const string NAME = "CubeBuilder Preview";

            public PreviewCube()
            {
                if (GameObject.Find(NAME) != null)
                {
                    DestroyImmediate(GameObject.Find(NAME));
                }
                gameObject = new GameObject(NAME);
                var mf = gameObject.AddComponent<MeshFilter>();
                var mr = gameObject.AddComponent<MeshRenderer>();
                mat = new Material(Shader.Find("CubePainter/Decal Diffuse Lit"));
                mesh = CreateCubeMesh();
                mf.sharedMesh = mesh;
                mr.sharedMaterial = mat;
                gameObject.transform.localPosition = Vector3.zero;
            }
        }
    }
}

