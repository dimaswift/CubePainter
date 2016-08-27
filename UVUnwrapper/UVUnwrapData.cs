using UnityEngine;
using System.Collections.Generic;
using HandyUtilities;

namespace CubePainter.UVUnwrapper
{
    public class UVUnwrapData : SerializedSingleton<UVUnwrapData>
    {
        public static readonly int[] FRONT = new int[] { 0, 2, 3, 1 };
        public static readonly int[] RIGHT = new int[] { 20, 21, 22, 23 };
        public static readonly int[] BACK = new int[] { 7, 11, 10, 6 };
        public static readonly int[] LEFT = new int[] { 16, 17, 18, 19 };
        public static readonly int[] TOP = new int[] { 8, 4, 5, 9 };
        public static readonly int[] BOTTOM = new int[] { 12, 13, 14, 15 };
        public static readonly int[][] ALL_SIDES = new int[][] { FRONT, TOP, BACK, RIGHT, BOTTOM, LEFT };
        static int[] _sideCache = new int[4];

        public enum CubeSide
        {
            FRONT = 0, TOP = 1, BACK = 2, RIGHT = 3, BOTTOM = 4, LEFT = 5
        }

        public enum UVChannel { UVChannle1, UVChannle2 }

        public static int[] GetSideIndices(CubeSide side)
        {
            return ALL_SIDES[(int) side];
        }

        public static int[] RotateSide(int[] side, Vector2 orientation)
        {

            return side;
        }

        public static Vector3 GetFaceDirection(CubeSide side)
        {
            switch (side)
            {
                case CubeSide.FRONT:
                    return Vector3.forward;
                case CubeSide.TOP:
                    return Vector3.up;
                case CubeSide.BACK:
                    return Vector3.back;
                case CubeSide.RIGHT:
                    return Vector3.right;
                case CubeSide.BOTTOM:
                    return Vector3.down;
                case CubeSide.LEFT:
                    return Vector3.left;
            }
            return Vector3.zero;
        }

        public static Vector3 GetFaceDirection(Vector3 point, Vector3 cubeScale, float penetration = 0.0001f)
        {
            return GetFaceDirection(GetSide(point, cubeScale, penetration));
        }

        public static Vector2 GetUVSideDirection(int[] indices, Vector2[] uv, Vector3[] vertices)
        {
            return (uv[indices[0]] - uv[indices[3]]).normalized;
        }

        public static Vector3 GetFaceDirection(int[] indices, Vector2[] uv, Vector3[] vertices)
        {
            return (vertices[indices[0]] - vertices[indices[1]]).normalized;
        }

        public static Vector2 GetUVPoint(Vector3 cubeScale, Vector3 point, Vector2[] uvs, out Vector3 faceDir, float penetration = 0.0001f)
        {
            var uv = GetUVPoint(cubeScale, point, uvs, penetration);
            faceDir = GetFaceDirection(GetSide(point, cubeScale, penetration));
            return uv;
        }

        public static Vector2 GetUVPoint(Vector3 cubeScale, Vector3 point, Vector2[] uvs, float penetration = 0.0001f)
        {
            var side = GetSide(point, cubeScale, penetration);

            var canvas = RotateSideIndices(side, Vector2.left, _sideCache);

            var u0 = uvs[canvas[0]];
            var u1 = uvs[canvas[1]];
            var u3 = uvs[canvas[3]];

            switch (side)
            {
                case CubeSide.FRONT:
                    return new Vector2(Helper.Remap(point.x, -cubeScale.x, cubeScale.x, u0.x, u3.x), Helper.Remap(point.y, cubeScale.y, -cubeScale.y, u0.y, u1.y));
                case CubeSide.TOP:
                    return new Vector2(Helper.Remap(point.x, -cubeScale.x, cubeScale.x, u0.x, u3.x), Helper.Remap(point.z, -cubeScale.z, cubeScale.z, u0.y, u1.y));
                case CubeSide.BACK:
                    return new Vector2(Helper.Remap(point.x, cubeScale.x, -cubeScale.x, u0.x, u3.x), Helper.Remap(point.y, cubeScale.y, -cubeScale.y, u0.y, u1.y));
                case CubeSide.RIGHT:
                    return new Vector2(Helper.Remap(point.z, cubeScale.z, -cubeScale.z, u0.x, u3.x), Helper.Remap(point.y, cubeScale.y, -cubeScale.y, u0.y, u1.y));
                case CubeSide.BOTTOM:
                    return new Vector2(Helper.Remap(point.x, -cubeScale.x, cubeScale.x, u0.x, u3.x), Helper.Remap(point.z, cubeScale.z, -cubeScale.z, u0.y, u1.y));
                case CubeSide.LEFT:
                    return new Vector2(Helper.Remap(point.z, -cubeScale.z, cubeScale.z, u0.x, u3.x), Helper.Remap(point.y, cubeScale.y, -cubeScale.y, u0.y, u1.y));

            }
            return Vector2.zero;
        }

        public static CubeSide GetSide(Vector3 point, Vector3 cubeScale, float penetration = 0.001f)
        {
            if (point.z >= cubeScale.z - penetration)
            {
                return CubeSide.FRONT;
            }
            if (point.y >= cubeScale.y - penetration)
            {
                return CubeSide.TOP;
            }
            if (point.z <= -cubeScale.z + penetration)
            {
                return CubeSide.BACK;
            }
            if (point.x >= cubeScale.x - penetration)
            {
                return CubeSide.RIGHT;
            }
            if (point.y <= -cubeScale.y + penetration)
            {
                return CubeSide.BOTTOM;
            }
            if (point.x <= -cubeScale.x + penetration)
            {
                return CubeSide.LEFT;
            }
            //  Debug.LogWarningFormat("{0} vector is out of bounds or too deep inside of box.", point);
            return CubeSide.FRONT;
        }

        public int targetGameObjectID;
        public UVChannel uvChannel = UVChannel.UVChannle1;
        public int textureSize = 16;
        public int textureWidth = 16;
        public int textureHeight = 16;
        public float gridSize = 16;
        public List<Side> sides = new List<Side>();
        public bool powerOfTwo = true;
        public bool snapToGrid = false;
        public bool showGrid = false;
        public bool createFolder = false;
        public Color color1 = Color.white;
        public Color color2 = new Color(0, .2f, .8f, 1);
        public float zoom = 1f;
        public Vector2 winSize;
        public bool maximized = false;
        public bool changeScale;
        public float pixelScale = 1f;
        public Vector3 scale = Vector3.one;
        public Vector3 pixelatedScale = Vector3.zero;
        public enum TextureType { Gradient, Clear, Checker, Margin, Color }
        public TextureType generateTextureType;
        [SerializeField]
        Rect textureRect;
        public Texture2D targetTexture = null;
        [SerializeField]
        Grid _grid;
        public bool scaleSidesWithGrid;
        public bool autoUpdateTargetUV = false;

        public string meshPath, texturePath, prefabPath, prefabFolder;
        public float poinsPerPixel { get { return textureRect.width / TextureSize.x; } }
        public Rect GetRawTextureRect()
        {
            return targetTexture == null ?
                new Rect(0, 0, grid.columnCount, grid.rowCount) :
                new Rect(0, 0, targetTexture.width, targetTexture.height);
        }

        public Rect containerRect;

        public Rect GetScaledTextureRect(float scale, Rect container)
        {
            var raw = GetRawTextureRect();

            var aspect = raw.height / raw.width;

            textureRect = new Rect(container.x, container.y, Helper.Remap(scale, 0f, 1f, 50, container.width), Helper.Remap(scale, 0f, 1f, 50, container.width) * aspect);

            if (textureRect.width > container.width)
            {
                textureRect.width = container.width;
                textureRect.height = container.width * aspect;
            }
            if (textureRect.height > container.height)
            {
                textureRect.height = container.height;
                textureRect.width = container.height / aspect;
            }

            //   textureRect.width = Mathf.Clamp(textureRect.width, 10, container.width);
            return textureRect;
        }

        [SerializeField]
        Vector2[] _uv = new Vector2[24];

        Vector3[] m_standardCubeVetices = new Vector3[0]; 
 
        public Grid grid
        {
            get { if (_grid == null) _grid = new Grid(TextureSize.x, TextureSize.y, poinsPerPixel, textureRect.position); return _grid; }
        }
        public Point TextureSize
        {
            get { return targetTexture != null ? new Point(targetTexture.width, targetTexture.height) : new Point(textureWidth, textureHeight); }
        }

        public void RecalculateGrid()
        {
            _grid.Recalculate(TextureSize.x, TextureSize.y, poinsPerPixel, textureRect.position);
        }

        public void BindSidesToContainer()
        {
            foreach (var s in sides)
            {
                s.MapRectToContainer(textureRect);
            }
        }

        public static Texture2D GenerateTexture(int width, int height, TextureType type, Color c1, Color c2)
        {
            var t = new Texture2D(width, height, TextureFormat.ARGB32, true);
            bool check = false;
            float minHue, maxHue, minSat, maxSat, minV, maxV;

            Color.RGBToHSV(c1, out minHue, out minSat, out minV);
            Color.RGBToHSV(c2, out maxHue, out maxSat, out maxV);
            for (int x = 0; x < width; x++)
            {
                check = !check;
                for (int y = 0; y < height; y++)
                {
                    check = !check;
                    Color color = new Color();
                    switch (type)
                    {
                        case TextureType.Gradient:
                            var diag = x + y;
                            color = Color.HSVToRGB(Helper.Remap(diag, 0, width + height, minHue, maxHue),
                                Helper.Remap(diag, 0, width + height, minSat, maxSat), Helper.Remap(diag, 0, width + height, minV, maxV));
                            break;
                        case TextureType.Clear:
                            color = new Color(0, 0, 0, 0);
                            break;
                        case TextureType.Checker:
                            color = check ? c1 : c2;
                            break;
                        case TextureType.Margin:
                            color = y == 0 || y == height - 1 || x == 0 || x == width - 1 ? c1 : c2;
                            break;
                        case TextureType.Color:
                            color = c1;
                            break;
                    }

                    t.SetPixel(x, y, color);
                }
            }
            t.Apply();
            return t;
        }
        public Texture2D GenerateTexture()
        {
            var t = new Texture2D(TextureSize.x, TextureSize.y, TextureFormat.ARGB32, true);
            return t;
        }

        public static int[] RotateSideIndices(CubeSide sideType, Vector2 orientation, int[] uvIndices, bool mirrored = false)
        {
            orientation.Normalize();
            var side = ALL_SIDES[(int) sideType];
            if (orientation.y == 1)
            {
                uvIndices[0] = side[3];
                uvIndices[1] = side[0];
                uvIndices[2] = side[1];
                uvIndices[3] = side[2];
            }
            else if (orientation.x == -1)
            {
                uvIndices[0] = side[2];
                uvIndices[1] = side[3];
                uvIndices[2] = side[0];
                uvIndices[3] = side[1];
            }
            else if (orientation.x == 1)
            {
                uvIndices[0] = side[0];
                uvIndices[1] = side[1];
                uvIndices[2] = side[2];
                uvIndices[3] = side[3];
            }
            else if (orientation.y == -1)
            {
                uvIndices[0] = side[0];
                uvIndices[1] = side[3];
                uvIndices[2] = side[2];
                uvIndices[3] = side[1];
            }
            if (mirrored)
                System.Array.Reverse(uvIndices);
            return uvIndices;
        }

        public Vector2[] GenerateUV()
        {
            for (int i = 0; i < 6; i++)
            {
                var side = sides[i];
                var canvas = RotateSideIndices(side.type, side.orientation, new int[4], side.mirrored);

                for (int j = 0; j < 4; j++)
                {
                    var _u = canvas[j];
                    _uv[_u] = side.uvs[j];
                }
            }
            return _uv;
        }

        public Vector2 GetGridPoint(Vector2 point)
        {
            return grid.GetPoint(point);
        }

        static Mesh CreateCubeMesh()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var mesh = cube.GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(cube);
            return Instantiate(mesh);
        }
        public void ApplyPixelScale()
        {
            MeshFilter target = UnityEditor.EditorUtility.InstanceIDToObject(targetGameObjectID) as MeshFilter;
            if(target)
            {
                if(m_standardCubeVetices.Length == 0)
                {
                    var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    m_standardCubeVetices = cube.GetComponent<MeshFilter>().sharedMesh.vertices;
                    DestroyImmediate(cube);
                }
                var mesh = target.sharedMesh;
                var vertices = mesh.vertices;
                pixelatedScale.x = (int) (scale.x * pixelScale);
                pixelatedScale.z = (int) (scale.z * pixelScale);
                pixelatedScale.y = (int) (scale.y * pixelScale);
                var maxSize = Instance.targetTexture.width;
                pixelatedScale.x = Mathf.Clamp(pixelatedScale.x, 1, maxSize);
                pixelatedScale.z = Mathf.Clamp(pixelatedScale.z, 1, maxSize);
                pixelatedScale.y = Mathf.Clamp(pixelatedScale.y, 1, maxSize);
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = Vector3.Scale(m_standardCubeVetices[i], pixelatedScale * pixelScale);
                }
                mesh.vertices = vertices;
                mesh.RecalculateBounds();
                foreach (var s in sides)
                {
                    s.Snap(containerRect, this);
                }
                if (uvChannel == UVChannel.UVChannle1)
                    mesh.uv = GenerateUV();
                else mesh.uv2 = GenerateUV();
                var box = target.GetComponent<BoxCollider>();
                if(box != null)
                {
                    box.size = mesh.bounds.size;
                    box.center = mesh.bounds.center;
                }
            }
        }
        
        public struct Point
        {
            public int x;
            public int y;
            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [System.Serializable]
        public class Side
        {
            public string name = "FRONT";
            public Color color;
            public Vector3 uvOrigin;
            public Vector3 size;
            public bool locked = false;
            public Rect rect;
            public bool showInfo;
            [SerializeField]
            Vector2[] _uvs = new Vector2[4];
            public CubeSide type;
            [SerializeField]
            Vector2 _orientation = Vector2.left;
            public bool mirrored = false;
            public Vector2 orientation { get { return _orientation; } }
            public void RotateUV()
            {
                if (_orientation == Vector2.up)
                    _orientation = Vector2.right;
                else if (_orientation == Vector2.right)
                    _orientation = Vector2.down;
                else if (_orientation == Vector2.down)
                    _orientation = Vector2.left;
                else if (_orientation == Vector2.left)
                    _orientation = Vector2.up;
            }
            public void RotateRect()
            {
                var x = size.x;
                var y = size.y;
                size.x = y;
                size.y = x;
            }

            public void Snap(Rect container, UVUnwrapData data)
            {
                var txtSize = new Vector2(data.targetTexture.width, data.targetTexture.height);
                var pixelScale = data.pixelatedScale;
                
                switch (type)
                {
                    case CubeSide.FRONT:
                        size.x = (pixelScale.x / txtSize.x);
                        size.y = (pixelScale.y / txtSize.y);
                        break;
                    case CubeSide.TOP:
                        size.x = (pixelScale.x / txtSize.x);
                        size.y = (pixelScale.z / txtSize.y);
                        break;
                    case CubeSide.BACK:
                        size.x = (pixelScale.x / txtSize.x);
                        size.y = (pixelScale.y / txtSize.y);
                        break;
                    case CubeSide.RIGHT:
                        size.x = (pixelScale.z / txtSize.x);
                        size.y = (pixelScale.y / txtSize.y);
                        break;
                    case CubeSide.BOTTOM:
                        size.x = (pixelScale.x / txtSize.x);
                        size.y = (pixelScale.z / txtSize.y);
                        break;
                    case CubeSide.LEFT:
                        size.x = (pixelScale.z / txtSize.x);
                        size.y = (pixelScale.y / txtSize.y);
                        break;
                    default:
                        break;
                }

                MapRectToContainer(container);
                MapPositionFromRect(container);

            }

            public Vector2[] uvs
            {
                get
                {
                    _uvs[3] = new Vector2(uvOrigin.x, uvOrigin.y);
                    _uvs[2] = new Vector2(uvOrigin.x, uvOrigin.y - size.y);
                    _uvs[1] = new Vector2(uvOrigin.x + size.x, uvOrigin.y - size.y);
                    _uvs[0] = new Vector2(uvOrigin.x + size.x, uvOrigin.y);
                    return _uvs;
                }
            }
            public bool Contains(Vector2 point)
            {
                return (point.x >= rect.x &&
                            point.x <= rect.x + rect.width &&
                             point.y >= rect.y &&
                              point.y <= rect.y + rect.height);
            }

            public Side(float x, float y, CubeSide type, Color color)
            {
                size = new Vector3(x, y, 0.1f);
                uvOrigin = new Vector3();
                color.a = .35f;
                this.color = color;
                this.name = type.ToString();
                this.type = type;
            }

            public void MapRectToContainer(Rect container)
            {

                rect = new Rect(
                    Helper.Remap(uvOrigin.x, 0f, 1f, container.x, container.x + container.width),
                    Helper.Remap(uvOrigin.y, 1f, 0f, container.y, container.y + container.height),
                    Helper.Remap(size.x, 0f, 1f, 0, container.width),
                    Helper.Remap(size.y, 0f, 1f, 0, container.width)
                );
            
                if (Instance.snapToGrid && Instance.poinsPerPixel > 3)
                {
                    rect.width = Instance.poinsPerPixel * Mathf.RoundToInt(rect.width / Instance.poinsPerPixel);
                    rect.height = Instance.poinsPerPixel * Mathf.RoundToInt(rect.height / Instance.poinsPerPixel);
                }
                rect.x = Mathf.Clamp(rect.x, container.x, container.x + container.width - rect.width);
                rect.y = Mathf.Clamp(rect.y, container.y, container.y + container.height - rect.height);

            }

            public void MapRectToGrid(Grid grid, int x, int y)
            {
                rect = new Rect(
                    Helper.Remap(uvOrigin.x, 0f, 1f, 0f, grid.columnCount),
                    Helper.Remap(uvOrigin.y, 0f, 1f, 0f, grid.rowCount),
                    x * grid.columnCount,
                    y * grid.rowCount);
            }

            public void MapPositionFromRect(Rect container)
            {
                uvOrigin = new Vector3(
                    Helper.Remap(rect.x, container.x, container.x + container.width, 0f, 1f),
                    Helper.Remap(rect.y, container.y, container.y + container.height, 1f, 0f)
                );
                _uvs = uvs;

            }

            public Side() { }

            public static Side operator *(Side side, float s)
            {
                side.size *= s;
                return side;
            }
        }

        public static List<Side> GetSides(Vector3 scale, Rect container, float size = 1)
        {
            var sides = new List<Side>();

            sides.Add(new Side(scale.x, scale.y, CubeSide.FRONT, Color.HSVToRGB(.1f, .7f, .7f)) * size);
            sides.Add(new Side(scale.x, scale.z, CubeSide.TOP, Color.HSVToRGB(.2f, .7f, .7f)) * size);
            sides.Add(new Side(scale.x, scale.y, CubeSide.BACK, Color.HSVToRGB(.3f, .7f, .7f)) * size);
            sides.Add(new Side(scale.z, scale.y, CubeSide.RIGHT, Color.HSVToRGB(.4f, .7f, .7f)) * size);
            sides.Add(new Side(scale.x, scale.z, CubeSide.BOTTOM, Color.HSVToRGB(.5f, .7f, .7f)) * size);
            sides.Add(new Side(scale.z, scale.y, CubeSide.LEFT, Color.HSVToRGB(.6f, .7f, .7f)) * size);
            var copyList = new List<Side>(sides);
            copyList.Sort((s1, s2) => s1.size.magnitude.CompareTo(s2.size.magnitude));
            var biggestSide = copyList.LastItem().size;
            var ms = biggestSide.x > biggestSide.y ? biggestSide.x : biggestSide.y;
            ms *= 3;

            for (int i = 0; i < sides.Count; i++)
            {
                var side = sides[i];
                side.size = new Vector3(Helper.Remap(side.size.x, 0, ms, 0, 1), Helper.Remap(side.size.y, 0, ms, 0, 1));
                switch (i)
                {
                    case 0:
                        side.uvOrigin = new Vector3(.5f - side.size.x / 2, .5f + side.size.y / 2);
                        break;
                    case 1:
                        side.uvOrigin = new Vector3(.5f - side.size.x / 2, 1);
                        break;
                    case 2:
                        side.uvOrigin = new Vector3(0, side.size.y);
                        break;
                    case 3:
                        side.uvOrigin = new Vector3(1 - side.size.x, 1);
                        break;
                    case 4:
                        side.uvOrigin = new Vector3(0, 1 - side.size.y);
                        break;
                    case 5:
                        side.uvOrigin = new Vector3(0, 1);
                        break;
                }
            }

            return sides;
        }
    }
}


