using UnityEngine;
using System.Collections;
using UnityEditor;

public class UVUnwrap : EditorWindow
{
    MeshFilter target;
    Texture2D dummyTexture, blackTexture;
    UVUnwrapSettings.Side draggedSide;
    Vector2 pressedMousePos, pressedSidePos;
    Texture2D _lockIcon;
    Texture2D lockIcon
    {
        get
        {
            if (_lockIcon == null)
                _lockIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/UV Unwrapper/lock.png");
            return _lockIcon;
        }
    }

    [MenuItem("Tools/UV Unwrap")]
    static void Open()
    {
        var w = GetWindow<UVUnwrap>("UV Unwrap", true);

        w.Show();
        EditorInterface.CenterOnMainWin(w); 
    }

    void OnDestroy()
    {
        DestroyImmediate(dummyTexture);
        DestroyImmediate(blackTexture);
    }

    Texture2D GetDummyTexture() 
    {
        if (dummyTexture == null)
        {
            dummyTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            dummyTexture.SetPixel(0, 0, new Color(0,0,0,0));
            dummyTexture.Apply();
        }
        return dummyTexture;
    }

    Texture2D GetBlackTexture()
    {
        if (blackTexture == null)
        {
            blackTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            blackTexture.SetPixel(0, 0, new Color(0,0,0,.25f));
            blackTexture.Apply();
        }
        return blackTexture;
    }

    void OnGUI()
    {
        var settings = UVUnwrapSettings.Instance;
        if (dummyTexture == null)
             dummyTexture = GetDummyTexture();
        if (blackTexture == null)
            blackTexture = GetBlackTexture();
        var width = EditorGUILayout.GetControlRect().width;
        var height = EditorGUILayout.GetControlRect().height;
        settings.textureRect = settings.targetTexture == null ?
            new Rect(5, 35, settings.textureSize * settings.gridSize, settings.textureSize * settings.gridSize) :
            new Rect(5, 35, settings.targetTexture.width * settings.zoom, settings.targetTexture.height * settings.zoom);


        var y = 5f;
        float dockWidth = 350;
        float elementYOffset = 25;
        target = EditorGUI.ObjectField(new Rect(width - dockWidth, y += elementYOffset, dockWidth, 16), target, typeof(MeshFilter), true) as MeshFilter;
        var _sidesScale = settings.sidesScale;
        settings.sidesScale = EditorGUI.Slider(new Rect(width - dockWidth, y += elementYOffset, dockWidth, 16), "Sides Scale", settings.sidesScale, 0f, 1f);
        if (settings.sidesScale != _sidesScale)
            settings.ScaleSides();

        var _pixelCount = settings.textureSize;
        settings.textureSize = EditorGUI.IntSlider(new Rect(width - dockWidth, y += elementYOffset, dockWidth, 16), "Pixel Count", settings.textureSize, 8, 128);
        if (settings.textureSize != _pixelCount)
            settings.RecalculateGrid();

        var _gridSize = settings.gridSize;
        settings.gridSize = EditorGUI.Slider(new Rect(width - dockWidth, y += elementYOffset, dockWidth, 16), "Grid Size", settings.gridSize, 5f, 50);
        if (settings.gridSize != _gridSize)
            settings.RecalculateGrid();

        settings.scaleSidesWithGrid = EditorGUI.Toggle(new Rect(width - dockWidth, y += elementYOffset, dockWidth, 16), "Keep Sides Ratio", settings.scaleSidesWithGrid);

        settings.snapToGrid = EditorGUI.Toggle(new Rect(width - dockWidth, y += elementYOffset, dockWidth, 16), "Snap To Grid", settings.snapToGrid);
        
        settings.targetTexture = EditorGUI.ObjectField(new Rect(width - dockWidth, y += elementYOffset, dockWidth, 16), "Target Texture", settings.targetTexture, typeof(Texture2D),  true) as Texture2D;

        var _zoom = settings.zoom;
        settings.zoom = EditorGUI.Slider(new Rect(width - dockWidth, y += elementYOffset, dockWidth, 16), "Zoom", settings.zoom, 0.1f, 1f);
        if(settings.zoom != _zoom)
            settings.ScaleSides();

        var gc = GUI.color;
        y += elementYOffset;
        for (int i = 0; i < 6; i++)
        {
            var side = settings.sides[i];
            GUI.color = gc;
            side.locked = EditorGUI.ToggleLeft(new Rect(width - dockWidth + 180, y, 50, 16), " Lock", side.locked);
            GUI.color = side.color;
            GUI.Box(new Rect(width - dockWidth, y, 70, 20), blackTexture);
            GUI.color = side.locked ? gc.SetAlpha(.2f) : gc;
            EditorGUI.LabelField(new Rect(width - dockWidth, y, 50, 16), side.name);
            EditorGUI.LabelField(new Rect(width - dockWidth + 75, y, 150, 16), string.Format("x:{0:0.000}, y:{1:0.000}", side.uv.x, side.uv.y));
         
          
        
            y += elementYOffset;
        }
        GUI.color = gc;
        var e = Event.current;

        if(settings.targetTexture != null)
        {
            GUI.DrawTexture(settings.textureRect, settings.targetTexture);
        }
    
        if (GUI.Button(new Rect(width - dockWidth, y += elementYOffset, dockWidth, 16), "Recalculate Grid"))
        {
            settings.RecalculateGrid();
        }

        if (target != null)
        {
            if (GUI.Button(new Rect(width - dockWidth, y += elementYOffset, dockWidth, 16), "Generate Sides"))
            {
                settings.GenerateSides(target.transform.localScale);
                settings.ScaleSides();
            }
        }

        if(draggedSide != null)
        {

        }

        var container = settings.textureRect;
        if(e.button == 0)
        {
            if (e.type == EventType.MouseDown)
            {
                pressedMousePos = e.mousePosition;
                bool touchedSide = false;
                foreach (var side in settings.sides)
                {
                    if (!side.locked && side.Contains(pressedMousePos))
                    {
                        touchedSide = true;
                        draggedSide = side;
                        pressedSidePos = side.rect.position;
                        break;
                    }
                }
                if (!touchedSide)
                    draggedSide = null;
            }
            if (e.type == EventType.MouseDrag && draggedSide != null)
            {
                var r = draggedSide.rect;
                var pos = pressedSidePos + (e.mousePosition - pressedMousePos);
                pos.x = Mathf.Clamp(pos.x, container.x, container.x + container.width - r.width);
                pos.y = Mathf.Clamp(pos.y, container.y, container.y + container.height - r.height);
                r.position = settings.snapToGrid ? settings.GetGridPoint(pos) : pos;
                draggedSide.rect = r;
                draggedSide.MapPositionFromRect(container);
            }
        }
        if(draggedSide != null)
        {
            GUI.color = new Color(0, 0, 0, 1);
            DrawRectagle(draggedSide.rect, 2);
            GUI.color = gc;
        }
        if(settings.snapToGrid)
        {
            for (int i = 0; i < settings.grid.Width; i++)
            {
                GUI.DrawTexture(new Rect(container.x, settings.grid.GetColumn(i), container.width, 1), blackTexture, ScaleMode.StretchToFill);
            }
            for (int i = 0; i < settings.grid.Height; i++)
            {
                GUI.DrawTexture(new Rect(settings.grid.GetRow(i), container.y, 1, container.height), blackTexture, ScaleMode.StretchToFill);
            }

            GUI.DrawTexture(new Rect(container.x, container.y + container.height, container.width, 1), blackTexture, ScaleMode.StretchToFill);
            GUI.DrawTexture(new Rect(container.x + container.width, container.y, 1, container.height), blackTexture, ScaleMode.StretchToFill);
        }
        else
        {
            DrawRectagle(container);
        }
        
        for (int i = 0; i < settings.sides.Count; i++)
        {
            var side = settings.sides[i];

            GUI.color = side.color;
            GUI.Box(side.rect, dummyTexture);
        }

        Repaint();
    }

    public void DrawRectagle(Rect rect, float width = 1f)
    {
        GUI.DrawTexture(new Rect(rect.x - width, rect.y + rect.height, rect.width + width * 2, width), blackTexture, ScaleMode.StretchToFill);
        GUI.DrawTexture(new Rect(rect.x + rect.width, rect.y, width, rect.height), blackTexture, ScaleMode.StretchToFill);
        GUI.DrawTexture(new Rect(rect.x - width, rect.y - width, rect.width + width * 2, width), blackTexture, ScaleMode.StretchToFill);
        GUI.DrawTexture(new Rect(rect.x - width, rect.y, width, rect.height), blackTexture, ScaleMode.StretchToFill);
    }

    class SidePanel
    {
        Color color;
        int index;
    }
}
