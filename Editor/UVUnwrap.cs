using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace UVUnwrapper
{
    public class UVUnwrapEditor : EditorWindow
    {
        UVUnwrapData.Side draggedSide;

        Vector2 pressedMousePos, pressedSidePos;

        Texture2D _dummyTexture, _blackTexture, _tmpTexture, _lockIcon;

        public MeshFilter target
        {
            get
            {
                return EditorUtility.InstanceIDToObject(UVUnwrapData.Instance.targetGameObjectID) as MeshFilter;
            }
            set
            {
                if (value)
                    UVUnwrapData.Instance.targetGameObjectID = value.GetInstanceID();
            }
        }

        public Texture2D lockIcon
        {
            get
            {
                if (_lockIcon == null)
                    _lockIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/UVUnwrapper/lock_icon.png");
                return _lockIcon;
            }
        }

        public Texture2D blackTexture
        {
            get
            {
                if (_blackTexture == null)
                {
                    _blackTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                    _blackTexture.SetPixel(0, 0, new Color(0, 0, 0, .25f));
                    _blackTexture.Apply();
                }
                return _blackTexture;
            }
        }

        public Texture2D dummyTexture
        {
            get
            {
                if (_dummyTexture == null)
                {
                    _dummyTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                    _dummyTexture.SetPixel(0, 0, new Color(0, 0, 0, 0));
                    _dummyTexture.Apply();
                }
                return _dummyTexture;
            }
        }

        [MenuItem("Tools/UV Unwrap")]
        static void Open()
        {
            var w = GetWindow<UVUnwrapEditor>("UV Unwrap", true);
            w.minSize = new Vector2(800, 500);
            w.Show();
            EditorInterface.CenterOnMainWin(w);
            Undo.undoRedoPerformed += w.OnUndo;
            UVUnwrapData.Instance.RecalculateGrid();
            UVUnwrapData.Instance.ScaleSides();
        }

        void OnDestroy()
        {
            DestroyImmediate(_tmpTexture);
            DestroyImmediate(dummyTexture);
            DestroyImmediate(blackTexture);
        }


        void OnUndo()
        {
            var data = UVUnwrapData.Instance;
            if (data.targetTexture == null)
                data.RecalculateGrid();
            if (data.autoUpdateTargetUV)
                UpdateTargetUV();
        }

        void OnGUI()
        {

            var y = 5f;
            float dockWidth = 350;
            float elementHeight = 25;
            var data = UVUnwrapData.Instance;
            var dockPosX = position.width - dockWidth;
            var gc = GUI.color;
            var winSize = new Vector2(position.width, position.height);

            if(data.winSize != winSize)
            {
                data.ScaleSides();
                data.winSize = winSize;
                if (data.autoUpdateTargetUV)
                    UpdateTargetUV();
            }
            Undo.RecordObject(data, "UVUnwraper Settings");

       
            if (data.targetTexture != null)
            {
                
            }

        

            target = (EditorGUI.ObjectField(new Rect(dockPosX, y += elementHeight, dockWidth, 16), target, typeof(MeshFilter), true) as MeshFilter);

            var _sidesScale = data.sidesScale;
            data.sidesScale = EditorGUI.Slider(new Rect(dockPosX, y += elementHeight, dockWidth, 16), "Sides Scale", data.sidesScale, .1f, 5f);
            if (data.sidesScale != _sidesScale)
            {
                data.ScaleSides();
                if (data.autoUpdateTargetUV)
                    UpdateTargetUV();
            }

            data.useGrid = EditorGUI.Toggle(new Rect(dockPosX, y += elementHeight, dockWidth, 16), "Use Grid", data.useGrid);

            if (data.useGrid)
            {
                if (data.targetTexture == null)
                {
                    data.textureWidth = EditorGUI.IntSlider(new Rect(dockPosX, y += elementHeight, dockWidth, 16), "Texture Width", data.textureWidth, 8, 128);
                    data.textureHeight = EditorGUI.IntSlider(new Rect(dockPosX, y += elementHeight, dockWidth, 16), "Texture Height", data.textureHeight, 8, 128);

                }
     
              
                var _gridSize = data.gridSize;
                data.gridSize = EditorGUI.Slider(new Rect(dockPosX, y += elementHeight, dockWidth, 16), "Grid Size", data.gridSize, 1f, 50);
                if (data.gridSize != _gridSize)
                    data.RecalculateGrid();
                data.snapToGrid = EditorGUI.Toggle(new Rect(dockPosX, y += elementHeight, dockWidth, 16), "Snap To Grid", data.snapToGrid);

            }

            data.autoUpdateTargetUV = EditorGUI.Toggle(new Rect(dockPosX, y += elementHeight, dockWidth, 16), "Auto Update UVs", data.autoUpdateTargetUV);

            data.targetTexture = EditorGUI.ObjectField(new Rect(dockPosX, y += elementHeight, dockWidth, 16), "Target Texture", data.targetTexture, typeof(Texture2D), true) as Texture2D;

            var _zoom = data.zoom;
            //    data.zoom = EditorGUI.Slider(new Rect(dockPosX, y += elementHeight, dockWidth, 16), "Zoom", data.zoom, 0.01f, 1f);
            data.zoom = 1f;
            if (data.zoom != _zoom)
            {
                data.ScaleSides();

            }

            if (GUI.Button(new Rect(dockPosX, y += elementHeight, dockWidth, 16), "Recalculate Grid"))
            {
                data.RecalculateGrid();
            }
            var container = data.GetScaledTextureRect(data.zoom, new Rect(10, 10, position.width - dockWidth - 20, position.height - 20));

            if (GUI.Button(new Rect(dockPosX, y += elementHeight, dockWidth, 16), "Generate Texture"))
            {
                var texName = string.Format("{0}_{1}x{2}", target != null ? target.name + "_texture" : "texture", data.TextureSize.x, data.TextureSize.y);
                var path = EditorUtility.SaveFilePanelInProject("Save texture", texName, "png", data.texturePath, data.texturePath);
                if (!string.IsNullOrEmpty(path))
                {
                    data.texturePath = path;
                    var t = GenerateGradientMap(data.TextureSize.x, data.TextureSize.y, .0f, .9f, .75f, .75f);
                    System.IO.File.WriteAllBytes(path, t.EncodeToPNG());
                    AssetDatabase.ImportAsset(path);
                }
            }

            if (target != null)
            {
                if (GUI.Button(new Rect(dockPosX, y += elementHeight, dockWidth, 16), "Generate Sides"))
                {
                    data.GenerateSides(target.transform.localScale, container);

                    data.ScaleSides();
                }
               
                if (GUI.Button(new Rect(dockPosX, y += elementHeight, dockWidth, 16), "Generate Mesh"))
                {
                    var path = EditorUtility.SaveFilePanelInProject("Save mesh", target.name + "_mesh", "asset", data.meshPath, data.meshPath);
                    if (!string.IsNullOrEmpty(path))
                    {
                        data.meshPath = path;
                        GenerateMesh(target, path);
                    }
                }
                if (GUI.Button(new Rect(dockPosX, y += elementHeight, dockWidth, 16), "Update UVs"))
                {
                    UpdateTargetUV();
                }
            }
            y += elementHeight;
            for (int i = 0; i < data.sides.Count; i++)
            {
                var side = data.sides[i];
                GUI.color = gc;
                side.locked = EditorGUI.Toggle(new Rect(dockPosX + dockWidth - 50, y, 16, 16), side.locked);
                GUI.DrawTexture(new Rect(dockPosX + dockWidth - 35, y, 16, 16), lockIcon);
                GUI.color = side == draggedSide ? side.color * 3 : side.color;
                GUI.Box(new Rect(dockPosX, y, 70, 20), blackTexture);
                GUI.color = side.locked ? gc.SetAlpha(.2f) : gc;
                EditorGUI.LabelField(new Rect(dockPosX, y, 70, 16), side.name, new GUIStyle() { alignment = TextAnchor.MiddleCenter });
               // EditorGUI.LabelField(new Rect(dockPosX + 75, y, 150, 16), string.Format("x:{0:0.000}, y:{1:0.000}", side.uvOrigin.x, side.uvOrigin.y));
                var _si = side.showInfo;
                side.showInfo = EditorGUI.Foldout(new Rect(dockPosX + dockWidth - 20, y, 20, 16), side.showInfo, "", true);
                if (side.showInfo && side.showInfo != _si)
                    SwitchSide(side); 
                if (GUI.Button(new Rect(dockPosX + 80, y, 70, 16), "Rotate UV"))
                {
                    side.RotateUV();
                    data.ScaleSides();
                    UpdateTargetUV();
                }
                if (GUI.Button(new Rect(dockPosX + 155, y, 80, 16), "Rotate Rect"))
                {
                    side.RotateRect();
                    data.ScaleSides();
                    UpdateTargetUV();
                }
                if (GUI.Button(new Rect(dockPosX + 240, y, 50, 16), "Reflect"))
                {
                    side.mirrored = !side.mirrored;
                    UpdateTargetUV();
                }

                if (side.showInfo)
                {
                    EditorGUI.LabelField(new Rect(dockPosX, y += elementHeight, dockWidth, 16), "UVS:", EditorStyles.centeredGreyMiniLabel);
                    for (int j = 0; j < 4; j++)
                    {
                        EditorGUI.Vector2Field(new Rect(dockPosX, y += elementHeight, dockWidth, 16), "", side.uvs[j]);
                    }
                    EditorGUI.LabelField(new Rect(dockPosX, y += elementHeight, dockWidth, 16), "Rect:", EditorStyles.centeredGreyMiniLabel);
                    EditorGUI.RectField(new Rect(dockPosX, y += elementHeight, dockWidth, 16), side.rect);
                    y += elementHeight;
                }
                y += elementHeight;
            }
            GUI.color = gc;

            var e = Event.current;

            if (data.targetTexture != null)
            {
                GUI.DrawTexture(container, data.targetTexture);
            }

            if (draggedSide != null)
            {
                y += elementHeight;
                //  GUI.DrawTexture(new Rect(width - _tmpTexture.width, y, _tmpTexture.width, _tmpTexture.height), _tmpTexture, ScaleMode.ScaleToFit);
            }

            if (e.button == 0)
            {
                if (e.type == EventType.MouseDown && container.Contains(e.mousePosition))
                {
                    pressedMousePos = e.mousePosition;
                    bool touchedSide = false;
                    foreach (var side in data.sides)
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
                if (e.type == EventType.MouseDrag && draggedSide != null && container.Contains(e.mousePosition))
                {
                    var r = draggedSide.rect;
                    var pos = pressedSidePos + (e.mousePosition - pressedMousePos);
                    pos.x = Mathf.Clamp(pos.x, container.x, container.x + container.width - r.width);
                    pos.y = Mathf.Clamp(pos.y, container.y, container.y + container.height - r.height);
                    r.position = data.snapToGrid ? data.GetGridPoint(pos) : pos;
                    draggedSide.rect = r;

                    draggedSide.MapPositionFromRect(container);
                    if (data.autoUpdateTargetUV)
                        UpdateTargetUV();
                    //DrawSidePreview();
                }
            }
            if (draggedSide != null)
            {
                GUI.color = new Color(0, 0, 0, 1);
                DrawRectagle(draggedSide.rect, 2);
                GUI.color = gc;
            }
            if (data.targetTexture == null)
            {
                for (int i = 0; i < data.grid.Width; i++)
                {
                    GUI.DrawTexture(new Rect(container.x, data.grid.GetColumn(i), container.width, 1), blackTexture, ScaleMode.StretchToFill);
                }
                for (int i = 0; i < data.grid.Height; i++)
                {
                    GUI.DrawTexture(new Rect(data.grid.GetRow(i), container.y, 1, container.height), blackTexture, ScaleMode.StretchToFill);
                }

                GUI.DrawTexture(new Rect(container.x, container.y + container.height, container.width, 1), blackTexture, ScaleMode.StretchToFill);
                GUI.DrawTexture(new Rect(container.x + container.width, container.y, 1, container.height), blackTexture, ScaleMode.StretchToFill);

            }
            else
            {
                DrawRectagle(container);
            }

            for (int i = 0; i < data.sides.Count; i++)
            {
                var side = data.sides[i];

                GUI.color = side.color;
                GUI.Box(side.rect, dummyTexture);
                GUI.color = gc;
                GUI.Label(new Rect((side.rect.x + (side.rect.width / 2)) - 50, (side.rect.y + (side.rect.height / 2)) - 50, 100, 100), side.name, new GUIStyle() { alignment = TextAnchor.MiddleCenter });
                if (side.locked)
                {
                    var lockWidth = side.rect.width > side.rect.height ? side.rect.width / 3 : side.rect.height / 3;
                    GUI.DrawTexture(new Rect(side.rect.center.x - lockWidth / 2, side.rect.center.y - lockWidth / 2, lockWidth, lockWidth), lockIcon);
                }
            }
            EditorUtility.SetDirty(data);
            Repaint();
        }

        public static Texture2D GenerateGradientMap(int width, int height, float minHue = 0f, float maxHue = 1f, float minSat = 0f, float maxSat = 1f)
        {
            var t = new Texture2D(width, height, TextureFormat.ARGB32, true);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var diag = x + y;
                    var c = Color.HSVToRGB(InterfaceUtilily.Remap(diag, 0, width + height, minHue, maxHue), 1, 1);
                    t.SetPixel(x, y, c);
                }
            }
            t.Apply();
            return t;
        }

        public void UpdateTargetUV()
        {
            if(target)
            {
                var mesh = target.sharedMesh;
                mesh.uv = UVUnwrapData.Instance.GenerateUV();
                EditorUtility.SetDirty(mesh);
            }

        }

        void GenerateMesh(MeshFilter target, string path)
        {
            var mesh = Instantiate(target.sharedMesh);
            mesh.uv = UVUnwrapData.Instance.GenerateUV();
            var existingMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            List<MeshFilter> filtersToReplace = new List<MeshFilter>();
            if (existingMesh != null)
            {
                var filters = FindObjectsOfType<MeshFilter>();
                foreach (var mf in filters)
                {
                    if (mf.sharedMesh == existingMesh)
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
            if (draggedSide != null && UVUnwrapData.Instance.targetTexture != null)
            {
                var targetTex = UVUnwrapData.Instance.targetTexture;
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

        void SwitchSide(UVUnwrapData.Side side)
        {
            if (draggedSide != side)
            {
                var settings = UVUnwrapData.Instance;
                draggedSide = side;
                DestroyImmediate(_tmpTexture);
                if (settings.targetTexture != null)
                {
                    _tmpTexture = new Texture2D((int) (settings.targetTexture.width * draggedSide.scaledSize.x), (int) (settings.targetTexture.height * draggedSide.scaledSize.y), TextureFormat.ARGB32, false);
                    DrawSidePreview();
                }
            }

        }

        public void DrawRectagle(Rect rect, float width = 1f)
        {
            GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height, rect.width, width), blackTexture, ScaleMode.StretchToFill);
            GUI.DrawTexture(new Rect(rect.x, rect.y + width, width, rect.height), blackTexture, ScaleMode.StretchToFill);
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, width), blackTexture, ScaleMode.StretchToFill);
            GUI.DrawTexture(new Rect(rect.x + rect.width, rect.y, width, rect.height), blackTexture, ScaleMode.StretchToFill);
        }

        class SidePanel
        {
            Color color;
            int index;
        }
    }

}
