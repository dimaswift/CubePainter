using UnityEngine;
using System.Collections.Generic;

public class UVUnwrapSettings : EditorSettings<UVUnwrapSettings>
{
    public int textureSize = 16;
    public int textureWidth = 16;
    public int textureHeight = 16;
    public float gridSize = 16;
    [Range(0.0001f, 1f)]
    public float sidesScale = 1f;
    public Vector3 boxSize = new Vector3(1,1,1);
    public List<Side> sides;
    public bool powerOfTwo = true;
    public bool snapToGrid = false;
    public float zoom = 1f;
    public Texture2D targetTexture = null;
    [SerializeField]
    Grid _grid;
    public bool scaleSidesWithGrid;
    public Grid grid { get { if (_grid == null) _grid = CreateInstance<Grid>(); return _grid; } }
    public Rect textureRect;
    public string meshFolderPath;
    [SerializeField]
    Vector2[] _uv = new Vector2[24];
    public Point TextureSize
    {
        get { return powerOfTwo ? new Point(textureSize, textureSize) : new Point(textureWidth, textureHeight); }
    }

    public struct Point { public int x; public int y; public Point(int x, int y) { this.x = x; this.y = y; } }

    public void RecalculateGrid()
    {
        if(targetTexture == null)
        {
            grid.CreateGrid(powerOfTwo ? textureSize : textureWidth, powerOfTwo ? textureSize : textureHeight, gridSize, textureRect.position);
        }
        else
        {
            grid.CreateGrid(Mathf.FloorToInt (textureRect.width / gridSize), Mathf.FloorToInt(textureRect.height / gridSize), gridSize, textureRect.position);
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

    public Vector2[] GenerateUV()
    {
        // side 1
        _uv[0] = sides[0].uvs[0];
        _uv[1] = sides[0].uvs[2];
        _uv[2] = sides[0].uvs[3];
        _uv[3] = sides[0].uvs[1];
        // side 2
        _uv[12] = sides[1].uvs[0];
        _uv[15] = sides[1].uvs[1];
        _uv[13] = sides[1].uvs[2];
        _uv[14] = sides[1].uvs[3];
        // side 3
        _uv[4] = sides[2].uvs[0];
        _uv[7] = sides[2].uvs[1];
        _uv[5] = sides[2].uvs[2];
        _uv[6] = sides[2].uvs[3];
        // side 4
        _uv[8] = sides[3].uvs[0];
        _uv[11] = sides[3].uvs[1];
        _uv[9] = sides[3].uvs[2];
        _uv[10] = sides[3].uvs[3];
        // side 5
        _uv[20] = sides[4].uvs[0];
        _uv[23] = sides[4].uvs[1];
        _uv[21] = sides[4].uvs[2];
        _uv[22] = sides[4].uvs[3];
        // side 6
        _uv[16] = sides[5].uvs[0];
        _uv[19] = sides[5].uvs[1];
        _uv[17] = sides[5].uvs[2];
        _uv[18] = sides[5].uvs[3];

        return _uv;
    }

    public void ScaleSides()
    {
        foreach (var s in sides)
        {
            s.scaledSize = s.size * sidesScale;
            s.MapRectToContainer(textureRect);
        }
        RecalculateGrid();
    }

    public Vector2 GetGridPoint(Vector2 point)
    {
        return grid.GetPoint(point);
    }

    public List<Side> GenerateSides(Vector3 size)
    {
        boxSize = size;
        sides = GetSides(size, 1, sidesScale);
        return sides;
    }
    [System.Serializable]
    public class Side
    {
        public string name = "side";
        public Color color;
        public Vector3 uvOrigin;
        public Vector3 size;
        public Vector3 scaledSize;
        public bool locked = false;
        public Rect rect;
        public bool showInfo;
        [SerializeField]
        Vector2[] _uvs = new Vector2[4];
        public Vector2[] uvs
        {
            get
            {
                _uvs[0] = new Vector2(uvOrigin.x, uvOrigin.y - scaledSize.y);
                _uvs[1] = new Vector2(uvOrigin.x + scaledSize.x, uvOrigin.y - scaledSize.y);
                _uvs[2] = new Vector2(uvOrigin.x, uvOrigin.y);
                _uvs[3] = new Vector2(uvOrigin.x + scaledSize.x, uvOrigin.y);
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

        public Side(float x, float y, Color color)
        {
            size = new Vector3(x, y, 0.1f);
            uvOrigin = new Vector3();
            color.a = .35f;
            this.color = color;
            scaledSize = size;
        }

        public void MapRectToContainer(Rect container)
        {
            rect = new Rect(
                InterfaceUtilily.Remap(uvOrigin.x, 0f, 1f, container.x, container.x + container.width),
                InterfaceUtilily.Remap(uvOrigin.y, 1f, 0f, container.y, container.y + container.height),
                InterfaceUtilily.Remap(scaledSize.x, 0f, 1f, 0, container.width),
                InterfaceUtilily.Remap(scaledSize.y, 0f, 1f, 0, container.height)
            );
        }

        public void MapPositionFromRect(Rect container)
        {
            uvOrigin = new Vector3(
                InterfaceUtilily.Remap(rect.x, container.x, container.x + container.width, 0f, 1f),
                InterfaceUtilily.Remap(rect.y, container.y, container.y + container.height, 1f, 0f)
            );
        }


        public Side() { }

        public static Side operator * (Side side, float s)
        {
            side.size *= s;
            return side;
        }
    }


    public static List<Side> GetSides(Vector3 scale, float maxSize, float size = 1)
    {
        var sides = new List<Side>();

        sides.Add(new Side(scale.x, scale.z, Color.HSVToRGB(.1f, .7f, .7f)) * size);
        sides.Add(new Side(scale.y, scale.z, Color.HSVToRGB(.2f, .7f, .7f)) * size);
        sides.Add(new Side(scale.x, scale.y, Color.HSVToRGB(.3f, .7f, .7f)) * size);
        sides.Add(new Side(scale.x, scale.z, Color.HSVToRGB(.4f, .7f, .7f)) * size);
        sides.Add(new Side(scale.y, scale.z, Color.HSVToRGB(.5f, .7f, .7f)) * size);
        sides.Add(new Side(scale.x, scale.y, Color.HSVToRGB(.6f, .7f, .7f)) * size);


 

        sides.Sort((s1, s2) => s1.size.magnitude.CompareTo(s2.size.magnitude));
        var largest = sides.LastItem().size;
        maxSize = largest.x > largest.y ? largest.x : largest.y;

        foreach (var side  in sides)
        {
            side.size = new Vector3(InterfaceUtilily.Remap(side.size.x, 0, maxSize, 0, 1), InterfaceUtilily.Remap(side.size.y, 0, maxSize, 0, 1));
            side.uvOrigin = new Vector3(0, side.size.y);
        }

        return sides;
    }
}
