using UnityEngine;
using System.Collections.Generic;
using HandyUtilities;

namespace CubePainter
{
    using UVUnwrapper;
    [RequireComponent(typeof(Collider))]
    public class Decal : MonoBehaviour
    {
        // Inspector settings
        [SerializeField]
        bool m_debug = false;
        [SerializeField]
        bool m_isPlane;
        [SerializeField]
        bool m_useDecalPool;
        [SerializeField]
        int m_decalIndex = 0;
        [SerializeField]
        string m_decalPropName = "_Decal";

        // Private fields
        Texture2D m_decalTex;
        MeshRenderer m_meshRenderer;
        Bounds m_bounds;
        Transform m_cachedTransform;
        Vector2[] m_uvs;
        Collider m_collider;
        float m_hue;
        Color m_baseColor;
        Color32[] m_blankColor;
        bool m_isDirty = false;
        int m_decalHeight, m_decalWidth;

        // Static values
        const float MAX_PENETRATION_VALUE = .001f;
        static float commonHue;
        public static Color currentGradient = Color.red;

        // Public properties
        public Texture2D decalTexture { get { return m_decalTex; } }
        public MeshRenderer meshRenderer { get { return m_meshRenderer; } }

        struct PixelPoint
        {
            public int x;
            public int y;
            int _originalX;
            int _originalY;
            public static PixelPoint left { get { return new PixelPoint(-1, 0); } }
            public static PixelPoint right { get { return new PixelPoint(1, 0); } }
            public static PixelPoint up { get { return new PixelPoint(0, 1); } }
            public static PixelPoint down { get { return new PixelPoint(0, -1); } }
            public static PixelPoint upLeft { get { return new PixelPoint(-1, 1); } }
            public static PixelPoint upRight { get { return new PixelPoint(1, 1); } }
            public static PixelPoint downLeft { get { return new PixelPoint(-1, -1); } }
            public static PixelPoint downRight { get { return new PixelPoint(1, -1); } }
            public static PixelPoint[] marginPoints { get { return _marginPoints; } }
            static readonly PixelPoint[] _marginPoints = new PixelPoint[] { downLeft, left, upLeft, up, upRight, right, downRight, down };

            public PixelPoint(int x, int y)
            {
                this.x = x;
                this.y = y;
                _originalX = x;
                _originalY = y;

            }
            public PixelPoint(float x, float y)
            {
                this.x = (int) x;
                this.y = (int) y;
                _originalX = this.x;
                _originalY = this.y;
            }
            public void Shift(int x, int y)
            {
                this.x = _originalX + x;
                this.y = _originalY + y;
            }

            public static PixelPoint operator + (PixelPoint p1, PixelPoint p2)
            {
                return new PixelPoint(p1.x + p2.x, p2.y + p2.y);
            }

            public static PixelPoint operator - (PixelPoint p1, PixelPoint p2)
            {
                return new PixelPoint(p1.x - p2.x, p2.y - p2.y);
            }
        }

        void Start()
        {
            m_collider = GetComponent<Collider>();
            m_meshRenderer = GetComponentInChildren<MeshRenderer>();
            m_cachedTransform = transform;
            var m = m_meshRenderer.GetComponent<MeshFilter>().sharedMesh;
            m_bounds = m.bounds;
            m_uvs = m.uv;
            if (!m_useDecalPool)
            {
                SetDirty();
            }
            else DecalManager.RegisterDecal(this);
            m_decalHeight = m_decalTex.height;
            m_decalWidth = m_decalTex.width;
            //  Clear();
        }

        void SetDirty()
        {
            if (!m_isDirty)
            {
                if (!m_useDecalPool)
                {
                    var decalSource = m_meshRenderer.sharedMaterial.GetTexture(m_decalPropName);
                    m_decalTex = Instantiate(decalSource) as Texture2D;
                    if(m_decalTex == null)
                        m_decalTex = Instantiate(decalSource) as Texture2D;
                    m_meshRenderer.material.SetTexture("_Decal", m_decalTex);
                    m_blankColor = new Color32[m_decalTex.width * m_decalTex.height];
                    m_isDirty = true;
                    return;
                }
                var p = DecalManager.GetPool(m_meshRenderer.sharedMaterial).Pick();
                m_decalTex = p.texture;
                m_meshRenderer.sharedMaterial = p.material;
                m_isDirty = true;
            }
        }


        public Vector2 GetPlaneUV(Vector3 worldPos)
        {
            var local = m_cachedTransform.InverseTransformPoint(worldPos);
            var x = Helper.Remap(local.x, m_bounds.extents.x, -m_bounds.extents.x, 0f, 1f);
            var y = Helper.Remap(local.z, m_bounds.extents.z, -m_bounds.extents.z, 0f, 1f);
            return new Vector2(x, y);
        }

        public Vector2 GetUV(Vector3 worldPos)
        {
            var local = m_cachedTransform.InverseTransformPoint(worldPos) - m_bounds.center;
            return UVUnwrapData.GetUVPoint(m_bounds.extents, local, m_uvs, MAX_PENETRATION_VALUE); ;
        }

        public void PlacePixel(Vector3 point, Color color, float lerp = 1f)
        {
            SetDirty();
            point = m_collider.ClosestPointOnBounds(point);
            Vector2 uv = m_isPlane ? GetPlaneUV(point) : GetUV(point);
            var pp = new PixelPoint(uv.x * m_decalWidth, uv.y * m_decalHeight);
            var oldPixel = m_decalTex.GetPixel(pp.x, pp.y);
            if (oldPixel == color) return;
            if (lerp < 1)
                color = Color.Lerp(oldPixel, color, lerp);
            m_decalTex.SetPixel(pp.x, pp.y, color);
            m_decalTex.Apply();
        }

        public static Color GetNextGradient()
        {
            commonHue += Time.deltaTime * .05f;
            if (commonHue > 1)
                commonHue = 0;
            currentGradient = Color.HSVToRGB(commonHue, 1, 1);
            return currentGradient;
        }

        public void PlaceSmudge(Vector3 point, Color color, float lerp = 1f)
        {
            SetDirty();
            Vector2 uv = m_isPlane ? GetPlaneUV(point) : GetUV(point);
            var pp = new PixelPoint(uv.x * m_decalWidth, uv.y * m_decalHeight);
            var oldPixel = m_decalTex.GetPixel(pp.x, pp.y);

            if (lerp < 1)
                color = Color.Lerp(oldPixel, color, lerp);

            float splashChance = .6f;
            m_decalTex.SetPixel(pp.x, pp.y, color);
            for (int i = 0; i < PixelPoint.marginPoints.Length; i++)
            {
                if (Random.value < splashChance)
                {
                    var m = PixelPoint.marginPoints[i];
                    if(pp.x > 0 && pp.x < m_decalWidth - 1 && pp.y > 0 && pp.y < m_decalHeight - 1)
                        m_decalTex.SetPixel(m.x + pp.x, m.y + pp.y, color);
                }
            }
            m_decalTex.Apply();
        }

        public void PlaceSmudge2(Vector3 point, Color color, float lerp = 1f)
        {
            SetDirty();
            float splashChance = .6f;
            for (int i = 0; i < PixelPoint.marginPoints.Length; i++)
            {
                if (Random.value < splashChance)
                {
                    var mp = PixelPoint.marginPoints[i];
                    Vector2 uv = m_isPlane ? GetPlaneUV(point) : GetUV(point);
                    var pp = new PixelPoint(uv.x * m_decalWidth, uv.y * m_decalHeight);
                    var oldPixel = m_decalTex.GetPixel(pp.x, pp.y);

                    if (lerp < 1)
                        color = Color.Lerp(oldPixel, color, lerp);


                    m_decalTex.SetPixel(pp.x, pp.y, color);

                }
            }

         
            m_decalTex.Apply();
        }

        void Clear()
        {
            m_decalTex.SetPixels32(m_useDecalPool ? DecalManager.GetBlankColors(m_decalTex) : m_blankColor);
            m_decalTex.Apply();
        }


        void PlacePixel(int x, int y, Color color)
        {
            var pixel = m_decalTex.GetPixel(x, y);
            if (pixel != color)
            {
                m_decalTex.SetPixel(x, y, color);
                m_decalTex.Apply();
            }
        }

        void PlaceNextPixel()
        {



        }

        void Update()
        {
            //dir = transform.TransformDirection(Vector3.forward);
            //for (int i = 0; i < flows.Length; i++)
            //{
            //    var f = flows[i];

            //    if(f.IsReadyToPlaceNext())
            //    {
            //        PlaceSmudge(f.GetNextPoint(), f.color);
            //    }
            //}
            //if(placingDecal)
            //{
            //    timer.Wait(PlaceNextPixel);
            //}

            //   uvs = GetComponent<MeshFilter>().sharedMesh.uv;
            if (Input.GetKeyDown(KeyCode.C))
            {
                Clear();
            }
            if (Input.GetMouseButton(0))
            {
                var ray = Helper.RaycastMouse();
                if (ray.transform == transform)
                {
                 
                    PlacePixel(ray.point, GetNextGradient());
                }
         
             
            }
        }
    }

}

