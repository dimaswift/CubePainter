using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class UVUnwrap : EditorWindow
{

    MeshFilter target;
    Texture2D dummyTexture, blackTexture;
    UVUnwrapSettings.Side draggedSide;
    Vector2 pressedMousePos, pressedSidePos;
    Texture2D _tmpTexture;
    Texture2D _lockIcon;
    Texture2D lockIcon
    {
        get
        {
            if (_lockIcon == null)
                _lockIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/UVUnwrapper/lock_icon.png");
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
        DestroyImmediate(_tmpTexture);
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
        var y = 5f;
        float dockWidth = 350;
        float elementYOffset = 25;

        var settings = UVUnwrapSettings.Instance;
        if (dummyTexture == null)
             dummyTexture = GetDummyTexture();
        if (blackTexture == null)
            blackTexture = GetBlackTexture();
        var width = EditorGUILayout.GetControlRect().width;
        var height = EditorGUILayout.GetControlRect().height;
        settings.textureRect = settings.targetTexture == null ?
            new Rect(5, 35, settings.textureSize * settings.gridSize * settings.zoom, settings.textureSize * settings.gridSize * settings.zoom) :
            new Rect(5, 35, settings.targetTexture.width * settings.zoom * 2, settings.targetTexture.height * settings.zoom * 2);
        //var aspect = settings.textureRect.width / settings.textureRect.height;
        //settings.textureRect.width = Mathf.Clamp(settings.textureRect.width, 10, width - dockWidth - settings.textureRect.x);
        //settings.textureRect.height = settings.textureRect.width / aspect;

       target = EditorGUI.ObjectField(new Rect(width - dockWidth, y += elementYOffset, dockWidth, 16), target, typeof(MeshFilter), true) as MeshFilter;
        var _sidesScale = settings.sidesScale;
        settings.sidesScale = EditorGUI.Slider(new Rect(width - dockWidth, y += elementYOffset, dockWidth, 16), "Sides Scale", settings.sidesScale, .1f, .5f);
        if (settings.sidesScale != _sidesScale)
            settings.ScaleSides();

        var _pixelCount = settings.textureSize;
        settings.textureSize = EditorGUI.IntSlider(new Rect(width - dockWidth, y += elementYOffset, dockWidth, 16), "Texture Size", settings.textureSize, 8, 128);
        if (settings.textureSize != _pixelCount)
            settings.RecalculateGrid();

        var _gridSize = settings.gridSize;
        settings.gridSize = EditorGUI.Slider(new Rect(width - dockWidth, y += elementYOffset, dockWidth, 16), "Grid Size", settings.gridSize, 1f, 50);
        if (settings.gridSize != _gridSize)
            settings.RecalculateGrid();

        settings.scaleSidesWithGrid = EditorGUI.Toggle(new Rect(width - dockWidth, y += elementYOffset, dockWidth, 16), "Keep Sides Ratio", settings.scaleSidesWithGrid);

        settings.snapToGrid = EditorGUI.Toggle(new Rect(width - dockWidth, y += elementYOffset, dockWidth, 16), "Snap To Grid", settings.snapToGrid);
        
        settings.targetTexture = EditorGUI.ObjectField(new Rect(width - dockWidth, y += elementYOffset, dockWidth, 16), "Target Texture", settings.targetTexture, typeof(Texture2D),  true) as Texture2D;

        var _zoom = settings.zoom;
        settings.zoom = EditorGUI.Slider(new Rect(width - dockWidth, y += elementYOffset, dockWidth, 16), "Zoom", settings.zoom, 0.1f, 5f);
        if(settings.zoom != _zoom)
        {
            settings.ScaleSides();
          
        }
           

        var gc = GUI.color;
     

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
            if (GUI.Button(new Rect(width - dockWidth, y += elementYOffset, dockWidth, 16), "Generate Texture"))
            {
                var path = EditorUtility.SaveFilePanelInProject("Save texture", target.name + "_texture", "png", settings.texturePath, settings.texturePath);
                if (!string.IsNullOrEmpty(path))
                {
                    settings.texturePath = path;
                    var t = settings.GenerateTexture();
                    System.IO.File.WriteAllBytes(path, t.EncodeToPNG());
                    AssetDatabase.ImportAsset(path);
                }
            }
            if (GUI.Button(new Rect(width - dockWidth, y += elementYOffset, dockWidth, 16), "Generate Mesh"))
            {
                var path = EditorUtility.SaveFilePanelInProject("Save mesh", target.name + "_mesh", "asset", settings.meshPath, settings.meshPath);
                if (!string.IsNullOrEmpty(path))
                {
                    settings.meshPath = path;
                    GenerateMesh(target, path);
                }
            }
        }
        y += elementYOffset;
        for (int i = 0; i < settings.sides.Count; i++)
        {
            var side = settings.sides[i];
            GUI.color = gc;
            side.locked = EditorGUI.Toggle(new Rect(width - dockWidth + 180, y, 16, 16), side.locked);
            GUI.DrawTexture(new Rect(width - dockWidth + 195, y, 16, 16), lockIcon);
            GUI.color = side.color;
            GUI.Box(new Rect(width - dockWidth, y, 70, 20), blackTexture);
            GUI.color = side.locked ? gc.SetAlpha(.2f) : gc;
            EditorGUI.LabelField(new Rect(width - dockWidth, y, 50, 16), side.name);
            EditorGUI.LabelField(new Rect(width - dockWidth + 75, y, 150, 16), string.Format("x:{0:0.000}, y:{1:0.000}", side.uvOrigin.x, side.uvOrigin.y));
            var _si = side.showInfo;
            side.showInfo = EditorGUI.Foldout(new Rect(width - dockWidth + 220, y, 150, 16), side.showInfo, "Show Info", true);
            if (side.showInfo && side.showInfo != _si)
                SwitchSide(side);
            if (side.showInfo)
            {
                EditorGUI.LabelField(new Rect(width - dockWidth, y += elementYOffset, dockWidth, 16), "UVS:", EditorStyles.centeredGreyMiniLabel);
                for (int j = 0; j < 4; j++)
                {
                    EditorGUI.Vector2Field(new Rect(width - dockWidth, y += elementYOffset, dockWidth, 16),"", side.uvs[j]);
                }
                EditorGUI.LabelField(new Rect(width - dockWidth, y += elementYOffset, dockWidth, 16), "Rect:", EditorStyles.centeredGreyMiniLabel);
                EditorGUI.RectField(new Rect(width - dockWidth, y += elementYOffset, dockWidth, 16), side.rect);
                y += elementYOffset;
            }
            y += elementYOffset;
        }
        GUI.color = gc;
        var e = Event.current;




        if (settings.targetTexture != null)
        {
            GUI.DrawTexture(settings.textureRect, settings.targetTexture);
        }

        if (draggedSide != null)
        {
            y += elementYOffset;
          //  GUI.DrawTexture(new Rect(width - _tmpTexture.width, y, _tmpTexture.width, _tmpTexture.height), _tmpTexture, ScaleMode.ScaleToFit);
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
                        SwitchSide(side);
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

                //DrawSidePreview();
            }
        }
        if(draggedSide != null)
        {
            GUI.color = new Color(0, 0, 0, 1);
            DrawRectagle(draggedSide.rect, 2);
            GUI.color = gc;
        }
        if(settings.targetTexture == null)
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
            if(side.locked)
            {
                var lockWidth = side.rect.width > side.rect.height ? side.rect.width / 3 : side.rect.height / 3;
                GUI.DrawTexture(new Rect( side.rect.center.x - lockWidth / 2, side.rect.center.y - lockWidth / 2, lockWidth, lockWidth), lockIcon);
            }
        }
        EditorUtility.SetDirty(settings);   
        Repaint();
    }

    void GenerateMesh(MeshFilter target, string path)
    {
        var mesh = Instantiate(target.sharedMesh);
        mesh.uv = UVUnwrapSettings.Instance.GenerateUV();
        var existingMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        List<MeshFilter> filtersToReplace = new List<MeshFilter>();
        if (existingMesh != null)
        {
            var filters = FindObjectsOfType<MeshFilter>();
            foreach (var mf in filters)
            {
                if(mf.sharedMesh == existingMesh)
                {
                    filtersToReplace.Add(mf);
                }
            }
        }
        AssetDatabase.CreateAsset(mesh, path);
        foreach (var mf in filtersToReplace)
        {
            mf.sharedMesh = mesh;
        }
    }

    void DrawSidePreview()
    {
        if(draggedSide != null && UVUnwrapSettings.Instance.targetTexture != null)
        {
            var targetTex = UVUnwrapSettings.Instance.targetTexture;
            var settings = UVUnwrapSettings.Instance;
            var offset = new Vector2(draggedSide.uvs[0].x * targetTex.width, draggedSide.uvs[0].y * targetTex.height);
            for (int x = 0; x < _tmpTexture.width; x++)
            {
                for (int y = 0; y < _tmpTexture.height; y++)
                {
                    _tmpTexture.SetPixel(x, y, targetTex.GetPixel((int) (offset.x + x), (int) (offset.y + y)));
                }
            }
            _tmpTexture.Apply();
        }
    }

    void SwitchSide(UVUnwrapSettings.Side side)
    {
        if(draggedSide != side)
        {
            var settings = UVUnwrapSettings.Instance;
            draggedSide = side;
            DestroyImmediate(_tmpTexture);
            if(settings.targetTexture != null)
            {
                _tmpTexture = new Texture2D((int) (settings.targetTexture.width * draggedSide.scaledSize.x), (int) (settings.targetTexture.height * draggedSide.scaledSize.y), TextureFormat.ARGB32, false);
                DrawSidePreview();
            }
        }
      
    }

    public void DrawRectagle(Rect rect, float width = 1f)
    {
        GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - width, rect.width - width, width), blackTexture, ScaleMode.StretchToFill);
        GUI.DrawTexture(new Rect(rect.x, rect.y + width, width, rect.height - width), blackTexture, ScaleMode.StretchToFill);
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width - width, width), blackTexture, ScaleMode.StretchToFill);
        GUI.DrawTexture(new Rect(rect.x, rect.y, width, rect.height - width), blackTexture, ScaleMode.StretchToFill);
    }

    class SidePanel
    {
        Color color;
        int index;
    }
}
