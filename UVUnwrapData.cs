using UnityEngine;
using System.Collections.Generic;

namespace UVUnwrapper
{
    public class UVUnwrapData : EditorSettings<UVUnwrapData>
    {
        public static readonly int[] FRONT = new int[] { 4, 5, 6, 7 };
        public static readonly int[] RIGHT = new int[] { 23, 20, 21, 22 };
        public static readonly int[] BACK = new int[] { 1, 2, 3, 0 };
        public static readonly int[] LEFT = new int[] { 13, 14, 15, 12 };
        public static readonly int[] TOP = new int[] { 16, 17, 18, 19 };
        public static readonly int[] BOTTOM = new int[] { 8, 9, 10, 11 };
        public static readonly int[][] ALL_SIDES = new int[][] { FRONT, TOP, BACK, RIGHT, BOTTOM, LEFT };

        public enum CubeSide
        {
            FRONT = 0, TOP = 1, BACK = 2, RIGHT = 3, BOTTOM = 4, LEFT = 5
        }

        public static int[] GetSideIndices(CubeSide side)
        {
            return ALL_SIDES[(int)side];
        }

        public static int[] RotateSide(int[] side, Vector2 orientation)
        {

            return side;
        }

        public static Vector3 GetSideDirection(CubeSide side)
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

        public static Vector2 GetUVSideDirection(int[] indices, Vector2[] uv, Vector3[] vertices)
        {
            return (uv[indices[0]] - uv[indices[3]]).normalized;
        }

        public static Vector3 GetFaceDirection(int[] indices, Vector2[] uv, Vector3[] vertices)
        {
            return (vertices[indices[0]] - vertices[indices[1]]).normalized;
        }


        public static Vector2 GetUVPoint(Vector3 boxScale, Vector3 point, Vector2[] uvs, Vector3[] vertices, float penetration = 0.0001f)
        {
            var side = GetSide(point, boxScale, penetration);

            var canvas = RotateSideIndices(side, Vector2.left, new int[4]);
            var faceDir = GetFaceDirection(canvas, uvs, vertices);
            var uvDir = GetUVSideDirection(canvas, uvs, vertices);

      

            var u0 = uvs[canvas[0]];
            var u1 = uvs[canvas[1]];
            var u2 = uvs[canvas[2]];
            var u3 = uvs[canvas[3]];

            var v0 = uvs[canvas[0]];
            var v1 = uvs[canvas[1]];
            var v2 = uvs[canvas[2]];
            var v3 = uvs[canvas[3]];


            var normal = Vector3.Dot(faceDir, uvDir);
            Vector3 axis = GetSideDirection(side);
            //Debug.Log(string.Format("n:{0}", normal));
            //Debug.Log(string.Format("{0}", side)); 
            //switch (side)
            //{
            //    case CubeSide.FRONT:
            //        if(normal == 0)
            //            return new Vector2(InterfaceUtilily.Remap(point.x, boxScale.x, -boxScale.x, u0.x, u3.x), InterfaceUtilily.Remap(point.y, -boxScale.y, boxScale.y, u0.y, u1.y));
            //        else if(normal > 0)
            //            return new Vector2(InterfaceUtilily.Remap(point.y, -boxScale.y, boxScale.y, u0.y, u1.y), InterfaceUtilily.Remap(point.x, boxScale.x, -boxScale.x, u0.x, u3.x));
            //        else return new Vector2(InterfaceUtilily.Remap(point.y, boxScale.y, -boxScale.y, u0.y, u1.y), InterfaceUtilily.Remap(point.x, -boxScale.x, boxScale.x, u0.x, u3.x));
            //    case CubeSide.TOP:
            //        break;
            //    case CubeSide.BACK:
            //        if (normal == 0)
            //            return new Vector2(InterfaceUtilily.Remap(point.x, boxScale.x, -boxScale.x, u0.x, u1.x), InterfaceUtilily.Remap(point.y, -boxScale.y, boxScale.y, u0.y, u3.y));
            //        else if (normal > 0)
            //            return new Vector2(InterfaceUtilily.Remap(point.y, -boxScale.y, boxScale.y, u0.y, u1.y), InterfaceUtilily.Remap(point.x, boxScale.x, -boxScale.x, u0.x, u3.x));
            //        else return new Vector2(InterfaceUtilily.Remap(point.y, boxScale.y, -boxScale.y, u0.y, u1.y), InterfaceUtilily.Remap(point.x, -boxScale.x, boxScale.x, u0.x, u3.x));
            //    case CubeSide.RIGHT:
            //        break;
            //    case CubeSide.BOTTOM:
            //        break;
            //    case CubeSide.LEFT:
            //        break;
            //    default:
            //        break;
            //}







            switch (side)
            {
                case CubeSide.FRONT:
                    return new Vector2(InterfaceUtilily.Remap(point.x, -boxScale.x, boxScale.x, u0.x, u3.x), InterfaceUtilily.Remap(point.y, boxScale.y, -boxScale.y, u0.y, u1.y));
                case CubeSide.TOP:
                    return new Vector2(InterfaceUtilily.Remap(point.x, boxScale.x, -boxScale.x, u0.x, u3.x), InterfaceUtilily.Remap(point.z, boxScale.z, -boxScale.z, u0.y, u1.y));
                case CubeSide.BACK:
                    return new Vector2(InterfaceUtilily.Remap(point.x, boxScale.x, -boxScale.x, u0.x, u3.x), InterfaceUtilily.Remap(point.y, boxScale.y, -boxScale.y, u0.y, u1.y));
                case CubeSide.RIGHT:
                    return new Vector2(InterfaceUtilily.Remap(point.z, boxScale.z, -boxScale.z, u0.x, u3.x), InterfaceUtilily.Remap(point.y, boxScale.y, -boxScale.y, u0.y, u1.y));
                case CubeSide.BOTTOM:
                    return new Vector2(InterfaceUtilily.Remap(point.x, -boxScale.x, boxScale.x, u0.x, u3.x), InterfaceUtilily.Remap(point.z, boxScale.z, -boxScale.z, u0.y, u1.y));
                case CubeSide.LEFT:
                    return new Vector2(InterfaceUtilily.Remap(point.z, -boxScale.z, boxScale.z, u0.x, u3.x), InterfaceUtilily.Remap(point.y, boxScale.y, -boxScale.y, u0.y, u1.y));

            }

            Debug.LogWarning(string.Format("{0}", "Cube side is out of range, enter values between 0 and 5."));
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
        public int textureSize = 16;
        public int textureWidth = 16;
        public int textureHeight = 16;
        public float gridSize = 16;
        public float sidesScale = 1f;
        public Vector3 boxSize = new Vector3(1,1,1);
        public List<Side> sides;
        public bool powerOfTwo = true;
        public bool snapToGrid = false;
        public bool useGrid = false;
        public float zoom = 1f;
        public Vector2 winSize;
        [SerializeField]
        Rect textureRect;
        public Texture2D targetTexture = null;
        [SerializeField]
        Grid _grid;
        public bool scaleSidesWithGrid;
        public bool autoUpdateTargetUV = false;

        public string meshPath, texturePath;

        public Rect GetRawTextureRect()
        {
            return targetTexture == null ?
                new Rect(0, 0, grid.Width, grid.Height) :
                new Rect(0, 0, targetTexture.width, targetTexture.height);
        }

        public Rect GetScaledTextureRect(float scale, Rect container)
        {
            var raw = GetRawTextureRect();
            var scaled = new Rect(raw.x, raw.y, raw.width * scale, raw.height * scale);
            var aspect = raw.height / raw.width;
            textureRect = new Rect(container.x, container.y, InterfaceUtilily.Remap(scale, 0f, 1f, 50, container.width), container.height);
            textureRect.height = aspect * textureRect.width;
            return textureRect;
        }

        [SerializeField]
        Vector2[] _uv = new Vector2[24];


        public Grid grid
        {
            get { if (_grid == null) _grid = new Grid(); return _grid; }
        }
        public Point TextureSize
        {
            get { return powerOfTwo ? new Point(textureSize, textureSize) : new Point(textureWidth, textureHeight);  }
        }

        public void RecalculateGrid()
        {
            if (targetTexture == null)
            {
                grid.CreateGrid(TextureSize.x, TextureSize.y, zoom * gridSize, textureRect.position);
            }
            else
            {
                grid.CreateGrid(Mathf.FloorToInt (textureRect.width / gridSize), Mathf.FloorToInt(textureRect.height / gridSize), zoom * gridSize, textureRect.position);
            }
            if(scaleSidesWithGrid)
            {
                BindSidesToContainer();
            }
        }

        public void BindSidesToContainer()
        {
            foreach (var s in sides)
            {
                s.MapRectToContainer(textureRect);
            }
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

        public void ScaleSides()
        {
            foreach (var s in sides)
            {
                s.scaledSize = s.size * sidesScale;
                s.MapRectToContainer(textureRect);
                s.relativeSize = new Vector3(s.rect.width / textureRect.width, s.rect.height / textureRect.height);
            }

            RecalculateGrid();
        }

        public Vector2 GetGridPoint(Vector2 point)
        {
            return grid.GetPoint(point);
        }

        public List<Side> GenerateSides(Vector3 size, Rect container)
        {
            boxSize = size;
            sides = GetSides(size, container, sidesScale);
            return sides;
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
            public Vector3 scaledSize;
            public Vector3 relativeSize;
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
                else if(_orientation == Vector2.right)
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
            public Vector2[] uvs
            {
                get
                {
                    _uvs[3] = new Vector2(uvOrigin.x, uvOrigin.y);
                    _uvs[2] = new Vector2(uvOrigin.x, uvOrigin.y - relativeSize.y);
                    _uvs[1] = new Vector2(uvOrigin.x + relativeSize.x, uvOrigin.y - relativeSize.y);
                    _uvs[0] = new Vector2(uvOrigin.x + relativeSize.x, uvOrigin.y);
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
                scaledSize = size;
                this.name = type.ToString();
                this.type = type;
            }

            public void MapRectToContainer(Rect container)
            {
                rect = new Rect(
                    InterfaceUtilily.Remap(uvOrigin.x, 0f, 1f, container.x, container.x + container.width),
                    InterfaceUtilily.Remap(uvOrigin.y, 1f, 0f, container.y, container.y + container.height),
                    InterfaceUtilily.Remap(scaledSize.x, 0f, 1f, 0, container.width),
                    InterfaceUtilily.Remap(scaledSize.y, 0f, 1f, 0, container.width)
                );
                //if (Instance.snapToGrid)
                //{
                //    rect.width = Mathf.RoundToInt(Instance.grid.Width / rect.width) * Instance.grid.Size;
                //    rect.height = Mathf.RoundToInt(Instance.grid.Height / rect.height) * Instance.grid.Size;
                //}
            }

            public void MapRectToGrid(Grid grid, int x, int y)
            {
                rect = new Rect(
                    InterfaceUtilily.Remap(uvOrigin.x, 0f, 1f, 0f, grid.Width),
                    InterfaceUtilily.Remap(uvOrigin.y, 0f, 1f, 0f, grid.Height),
                    x * grid.Width,
                    y * grid.Height);
            }

            public void MapPositionFromRect(Rect container)
            {
                uvOrigin = new Vector3(
                    InterfaceUtilily.Remap(rect.x, container.x, container.x + container.width, 0f, 1f),
                    InterfaceUtilily.Remap(rect.y, container.y, container.y + container.height, 1f, 0f)
                );
                _uvs = uvs;

            }

            public Side() { }

            public static Side operator * (Side side, float s)
            {
                side.size *= s;
                return side;
            }
        }

        public static List<Side> GetSides(Vector3 scale, Rect container, float size = 1)
        {
            var sides = new List<Side>();

            sides.Add(new Side(scale.x, scale.y, CubeSide.FRONT,Color.HSVToRGB(.1f, .7f, .7f)) * size);
            sides.Add(new Side(scale.x, scale.z, CubeSide.TOP, Color.HSVToRGB(.2f, .7f, .7f)) * size);
            sides.Add(new Side(scale.y, scale.x, CubeSide.BACK, Color.HSVToRGB(.3f, .7f, .7f)) * size);
            sides.Add(new Side(scale.y, scale.z, CubeSide.RIGHT, Color.HSVToRGB(.4f, .7f, .7f)) * size);
            sides.Add(new Side(scale.x, scale.z, CubeSide.BOTTOM, Color.HSVToRGB(.5f, .7f, .7f)) * size);
            sides.Add(new Side(scale.y, scale.z, CubeSide.LEFT, Color.HSVToRGB(.6f, .7f, .7f)) * size);
            var copyList = new List<Side>(sides);
            copyList.Sort((s1, s2) => s1.size.magnitude.CompareTo(s2.size.magnitude));
            var biggestSide = copyList.LastItem().size;
            var ms = biggestSide.x > biggestSide.y ? biggestSide.x : biggestSide.y;
            ms *= 3;

            for (int i = 0; i < sides.Count; i++)
            {
                var side = sides[i];
                side.size = new Vector3(InterfaceUtilily.Remap(side.size.x, 0, ms, 0, 1), InterfaceUtilily.Remap(side.size.y, 0, ms, 0, 1));
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


