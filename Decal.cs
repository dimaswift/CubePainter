using UnityEngine;
using System.Collections.Generic;
using HandyUtilities;

namespace CubePainter
{
    using UVUnwrapper;
    public class Decal : MonoBehaviour
    {
        Texture2D decalTex;
        MeshRenderer meshRenderer;
        Bounds bounds;
        Transform cachedTransform;

        public bool debug = false;
        const float MAX_PENETRATION_VALUE = .001f;

        Vector2[] uvs;
        Collider box;
        static Color32[] blank = new Color32[128 * 128];
        float hue;
        Color baseColor;
        Flow[] flows = new Flow[10];
        static float commonHue;
        public static Color currentGradient = Color.red;
        public Vector3 dir;
        bool isDirty = false;
        public bool isPlane;
        public bool selfDecal;
        public int decalIndex = 0;

        struct Flow
        {
            public Color color;
            int count;
            float rate;
            float t;

            public Flow(Vector3 startPoint, Vector3 faceDirection, Color color, int count, float rate)
            {
                this.count = count;
                this.rate = rate;
                this.color = color;
                t = 0;
            }

            public bool IsDone()
            {
                return count <= 0;
            }

            public bool IsReadyToPlaceNext()
            {
                if (count <= 0) return false;
                t += Time.deltaTime;
                if (t >= rate)
                {
                    t = 0;
                    count--;
                    return true;
                }
                return false;
            }

            public Vector3 GetNextPoint()
            {
                var next = new Vector3();
                return next;
            }
        }

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
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            for (int i = 0; i < blank.Length; i++)
            {
                blank[i] = Color.white;
            }
        }

        void Start()
        {
            box = GetComponent<Collider>();
            meshRenderer = GetComponentInChildren<MeshRenderer>();
            cachedTransform = transform;
            var m = meshRenderer.GetComponent<MeshFilter>().sharedMesh;
            bounds = m.bounds;
            uvs = m.uv;
            if (selfDecal)
                SetDirty();
            //  Clear();
        }

        void SetDirty()
        {
            if (!isDirty)
            {
                if (selfDecal)
                {
                    var decalSource = meshRenderer.sharedMaterial.GetTexture("_Decal");
                    if (decalSource == null)
                        decalSource = meshRenderer.sharedMaterial.GetTexture("_MainTex");
                    decalTex = Instantiate(decalSource) as Texture2D;
                    if(decalTex == null)
                        decalTex = Instantiate(decalSource) as Texture2D;
                    meshRenderer.material.SetTexture("_Decal", decalTex);
                    isDirty = true;
                    return;
                }
                var p = DecalManager.Instance.GetPool(decalIndex).Pick();
                decalTex = p.texture;
                meshRenderer.sharedMaterial = p.material;
                isDirty = true;
            }
        }

        public Vector2 GetUV(Vector3 worldPos, out float horizonFactor)
        {
            var local = cachedTransform.InverseTransformPoint(worldPos) - bounds.center;
            Vector3 faceDir;
            var uv = UVUnwrapData.GetUVPoint(bounds.extents, local, uvs, out faceDir, MAX_PENETRATION_VALUE);
            horizonFactor = Mathf.Abs(Vector3.Dot(Vector3.down, cachedTransform.TransformDirection(faceDir)));
            return uv;
        }
        public Vector2 GetPlaneUV(Vector3 worldPos)
        {
            var local = cachedTransform.InverseTransformPoint(worldPos);
            var x = Helper.Remap(local.x, bounds.extents.x, -bounds.extents.x, 0f, 1f);
            var y = Helper.Remap(local.z, bounds.extents.z, -bounds.extents.z, 0f, 1f);
            return new Vector2(x, y);
        }

        public Vector2 GetUV(Vector3 worldPos)
        {
            var local = cachedTransform.InverseTransformPoint(worldPos) - bounds.center;
            return UVUnwrapData.GetUVPoint(bounds.extents, local, uvs, MAX_PENETRATION_VALUE); ;
        }

        //void OnDrawGizmos()
        //{
        //    var m = GetComponent<MeshFilter>().sharedMesh;
        //    uvs = m.uv;
        //    vertices = m.vertices;
        //    Gizmos.color = Color.blue;
        //    DrawRay(UVUnwrapData.RotateSide(UVUnwrapData.GetSideIndices(UVUnwrapData.CubeSide.FRONT), Vector2.left));
        //    DrawRay(UVUnwrapData.RotateSide(UVUnwrapData.GetSideIndices(UVUnwrapData.CubeSide.BACK), Vector2.left));
        //    Gizmos.color = Color.red;
        //    DrawRay(UVUnwrapData.RotateSide(UVUnwrapData.GetSideIndices(UVUnwrapData.CubeSide.LEFT), Vector2.left));
        //    DrawRay(UVUnwrapData.RotateSide(UVUnwrapData.GetSideIndices(UVUnwrapData.CubeSide.RIGHT), Vector2.left));
        //    Gizmos.color = Color.green;
        //    DrawRay(UVUnwrapData.RotateSide(UVUnwrapData.GetSideIndices(UVUnwrapData.CubeSide.TOP), Vector2.left));
        //    DrawRay(UVUnwrapData.RotateSide(UVUnwrapData.GetSideIndices(UVUnwrapData.CubeSide.BOTTOM), Vector2.left));





        //}


        public void PlacePixel(Vector3 point, Color color, float lerp = 1f)
        {
            SetDirty();
            //color = GetNextGradient();
            point = box.ClosestPointOnBounds(point);
            Vector2 uv;
            if (!isPlane)
            {
                float horizonFactor;
                uv = GetUV(point, out horizonFactor);
            }
            else
            {
                uv = GetPlaneUV(point);
            }

            var pp = new PixelPoint(uv.x * decalTex.width, uv.y * decalTex.height);
            var oldPixel = decalTex.GetPixel(pp.x, pp.y);
            if (lerp < 1)
                color = Color.Lerp(oldPixel, color, lerp);
            bool pixelChanged = false;
            if (color != oldPixel)
            {
                decalTex.SetPixel(pp.x, pp.y, color);
                pixelChanged = true;
            }
            if (pixelChanged)
                decalTex.Apply();
        }

        void AddNewFlow(Vector3 point, Vector3 faceDir, Color color, int count, float rate)
        {
            for (int i = 0; i < flows.Length; i++)
            {
                var f = flows[i];
                if (f.IsDone())
                {
                    // flows[i] = new Flow(point, faceDir, color, pixelPerUnit, 1, count, rate);
                    return;
                }
            }
            System.Array.Resize(ref flows, flows.Length + 1);
            //  flows[flows.Length - 1] = new Flow(point, faceDir, color, pixelPerUnit, 1, count, rate);
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
            //   point = box.ClosestPointOnBounds(point);
            float horizonFactor;
            Vector2 uv;
            if (!isPlane)
            {
                uv = GetUV(point, out horizonFactor);
            }
            else
            {
                uv = GetPlaneUV(point);
            }
            //  color = GetNextGradient();
            var pp = new PixelPoint(uv.x * decalTex.width, uv.y * decalTex.height);
            var oldPixel = decalTex.GetPixel(pp.x, pp.y);
            if (lerp < 1)
                color = Color.Lerp(oldPixel, color, lerp);


            // float flowChance = .5f;
            float splashChance = .6f;

            decalTex.SetPixel(pp.x, pp.y, color);
            for (int i = 0; i < PixelPoint.marginPoints.Length; i++)
            {
                if (Random.value < splashChance)
                {
                    var mp = PixelPoint.marginPoints[i];
                    decalTex.SetPixel(pp.x + mp.x, pp.y + mp.y, color);
                    //if (isVertical && Random.value < flowChance)
                    //{
                    //    AddNewFlow(point, UVUnwrapData.GetFaceDirection(point, bounds.extents, MAX_PENETRATION_VALUE), color, 5, .3f);
                    //}
                }
            }
            decalTex.Apply();
        }
        void Clear()
        {
            decalTex.SetPixels32(blank);
            decalTex.Apply();
        }


        void PlacePixel(int x, int y, Color color)
        {
            var pixel = decalTex.GetPixel(x, y);
            if (pixel != color)
            {
                decalTex.SetPixel(x, y, color);
                decalTex.Apply();
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
            if (Input.GetMouseButton(1))
            {

                var ray = Helper.RaycastMouse();
                if (ray.collider && ray.collider.gameObject == gameObject)
                {
                    var decal = ray.collider.GetComponent<Decal>();
                    if (decal)
                    {
                        hue += Time.deltaTime * .05f;
                        if (hue > 1)
                            hue = 0;
                        PlaceSmudge(ray.point, Color.HSVToRGB(hue, 1, 1), 1f);
                    }
                }
            }
        }
    }

}

