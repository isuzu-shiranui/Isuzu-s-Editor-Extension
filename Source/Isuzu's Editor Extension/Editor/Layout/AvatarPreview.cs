using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace IsuzuEditorExtension.Layout
{
    internal sealed class AvatarPreview
    {
        private const string PREVIEW_STR = "Preview";

        private const string previewSceneStr = "PreviewSene";

        private const float kFloorFadeDuration = 0.2f;
        private const float kFloorScale = 5;
        private const float kFloorScaleSmall = 0.2f;
        private const float kFloorTextureScale = 4;
        private const float kFloorAlpha = 0.5f;
        private const float kFloorShadowAlpha = 0.3f;
        private static readonly int Alphas = Shader.PropertyToID("_Alphas");
        private readonly float nextFloorHeight = 0;
        private readonly float prevFloorHeight = 0;

//        private readonly Vector2 previewDir = new Vector2(180, -20);
        private readonly int previewHint = PREVIEW_STR.GetHashCode();
        private readonly int previewSceneHint = previewSceneStr.GetHashCode();

        private Vector3 centerPosition;

        private Material floorMaterial;
        private Material floorMaterialSmall;
        private Mesh floorPlane;
        private Texture2D floorTexture;

        private bool is2D;
        private bool isValid;
        private Vector3 pivotPositionOffset = Vector3.zero;

        private Vector2 previewDir = new Vector2(120, -20);
        private GameObject previewObject;

        private PreviewPopupOptions previewPopupOptions;
        private PreviewRenderUtility previewUtility;
        private ViewToolType viewTool = ViewToolType.None;

        private float zoomFactor = 1.0f;

        private Animator Animator
        {
            get { return this.previewObject != null ? this.previewObject.GetComponent<Animator>() : null; }
        }

        private Vector3 RootPosition
        {
            get { return this.previewObject ? this.previewObject.transform.position : Vector3.zero; }
        }

        private bool Is2D
        {
            get { return this.is2D; }
            set
            {
                this.is2D = value;
                if (this.is2D) this.previewDir = new Vector2();
            }
        }

        private PreviewRenderUtility PreviewRenderUtility
        {
            get
            {
                if (this.previewUtility != null) return this.previewUtility;

                this.previewUtility = new PreviewRenderUtility();
                this.previewUtility.camera.fieldOfView = 30.0f;
                this.previewUtility.camera.allowHDR = false;
                this.previewUtility.camera.allowMSAA = false;
                this.previewUtility.ambientColor = new Color(.1f, .1f, .1f, 0);
                this.previewUtility.lights[0].intensity = 1.4f;
                this.previewUtility.lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0);
                this.previewUtility.lights[1].intensity = 1.4f;
                return this.previewUtility;
            }
        }

        private ViewToolType ViewTool
        {
            get
            {
                var evt = Event.current;
                if (this.viewTool != ViewToolType.None) return this.viewTool;
                var controlKeyOnMac = evt.control && Application.platform == RuntimePlatform.OSXEditor;

                // actionKey could be command key on mac or ctrl on windows
                var actionKey = EditorGUI.actionKey;

                var noModifiers = !actionKey && !controlKeyOnMac && !evt.alt;

                if (evt.button <= 0 && noModifiers || evt.button <= 0 && actionKey || evt.button == 2)
                    this.viewTool = ViewToolType.Pan;
                else if (evt.button <= 0 && controlKeyOnMac || evt.button == 1 && evt.alt)
                    this.viewTool = ViewToolType.Zoom;
                else if (evt.button <= 0 && evt.alt || evt.button == 1) this.viewTool = ViewToolType.Orbit;

                return this.viewTool;
            }
        }

        private MouseCursor CurrentCursor
        {
            get
            {
                switch (this.viewTool)
                {
                    case ViewToolType.Orbit: return MouseCursor.Orbit;
                    case ViewToolType.Pan: return MouseCursor.Pan;
                    case ViewToolType.Zoom: return MouseCursor.Zoom;
                    default: return MouseCursor.Arrow;
                }
            }
        }

        public void Initialize(GameObject targetObject)
        {
            this.previewObject = targetObject;
            this.SetupBounds(this.previewObject);

            if (this.floorPlane == null)
                this.floorPlane = Resources.GetBuiltinResource(typeof(Mesh), "New-Plane.fbx") as Mesh;

            if (this.floorTexture == null)
                this.floorTexture = (Texture2D) EditorGUIUtility.Load("Avatar/Textures/AvatarFloor.png");

            if (this.floorMaterial == null)
            {
                var shader = EditorGUIUtility.LoadRequired("Previews/PreviewPlaneWithShadow.shader") as Shader;
                this.floorMaterial = new Material(shader)
                {
                    mainTexture = this.floorTexture,
                    mainTextureScale = kFloorTextureScale * kFloorScale * Vector2.one
                };
                this.floorMaterial.SetVector(Alphas, new Vector4(kFloorAlpha, kFloorShadowAlpha, 0, 0));
                this.floorMaterial.hideFlags = HideFlags.HideAndDontSave;

                this.floorMaterialSmall = new Material(this.floorMaterial)
                {
                    mainTextureScale = kFloorTextureScale * kFloorScaleSmall * Vector2.one,
                    hideFlags = HideFlags.HideAndDontSave
                };
            }

            this.SetPreviewCharacterEnabled(false);
            this.pivotPositionOffset = Vector3.zero;
        }

        private void SetupBounds(GameObject go)
        {
            this.isValid = go != null && go != GetGenericAnimationFallback() &&
                           go.GetComponentsInChildren<SkinnedMeshRenderer>().Length > 0;

            if (this.isValid) this.PreviewRenderUtility.AddSingleGO(go);
        }

        private static GameObject GetGenericAnimationFallback()
        {
            return (GameObject) EditorGUIUtility.Load("Avatar/DefaultGeneric.fbx");
        }

        private void DrawRenderPreview(Rect previewRect, GUIStyle background)
        {
            if (this.previewObject == null) return;

            var probe = RenderSettings.ambientProbe;
            this.PreviewRenderUtility.BeginPreview(previewRect, background);

            Quaternion bodyRot;
            Quaternion rootRot;
            Vector3 rootPos;
            var bodyPos = this.RootPosition;

            if (this.Animator && this.Animator.isHuman)
            {
                rootRot = this.Animator.rootRotation;
                rootPos = this.Animator.rootPosition;

                bodyRot = this.Animator.bodyRotation;
            }
            else if (this.Animator && this.Animator.hasRootMotion)
            {
                rootRot = this.Animator.rootRotation;
                rootPos = this.Animator.rootPosition;

                bodyRot = Quaternion.identity;
            }
            else
            {
                rootRot = Quaternion.identity;
                rootPos = Vector3.zero;

                bodyRot = Quaternion.identity;
            }

            var direction = bodyRot * Vector3.forward;
            direction[1] = 0;
            var directionRot = Quaternion.LookRotation(direction);
            var directionPos = rootPos;

            var pivotRot = rootRot;


            var dynamicFloorHeight = !this.Is2D &&
                                     Mathf.Abs(this.nextFloorHeight - this.prevFloorHeight) >
                                     this.zoomFactor * 0.01f;

            // Calculate floor height and alpha
            float mainFloorHeight, mainFloorAlpha;
            if (dynamicFloorHeight)
            {
                var fadeMoment = this.nextFloorHeight < this.prevFloorHeight
                    ? kFloorFadeDuration
                    : 1 - kFloorFadeDuration;
//                mainFloorHeight = timeControl.normalizedTime < fadeMoment ? m_PrevFloorHeight : m_NextFloorHeight;
                mainFloorHeight = 0;
//                mainFloorAlpha = Mathf.Clamp01(Mathf.Abs(timeControl.normalizedTime - fadeMoment) / kFloorFadeDuration);
                mainFloorAlpha = 1;
            }
            else
            {
                mainFloorHeight = this.prevFloorHeight;
                mainFloorAlpha = this.Is2D ? 0.5f : 1;
            }

            var floorRot = this.Is2D ? Quaternion.Euler(-90, 0, 0) : Quaternion.identity;
            var floorPos = this.previewObject.transform.position;
            floorPos.y = mainFloorHeight;

            // Position camera
            this.previewUtility.camera.orthographic = false;
            this.previewUtility.camera.nearClipPlane = 0.5f * this.zoomFactor;
            this.previewUtility.camera.farClipPlane = 100.0f * this.Animator.humanScale;
            var camRot = Quaternion.Euler(-this.previewDir.y, -this.previewDir.x, 0);

            var camPos = camRot * (Vector3.forward * -5.5f * this.zoomFactor) + bodyPos + this.pivotPositionOffset;
            this.previewUtility.camera.transform.position = camPos;
            this.previewUtility.camera.transform.rotation = camRot;

            this.SetPreviewCharacterEnabled(true);
            this.previewUtility.Render(this.previewPopupOptions != PreviewPopupOptions.DefaultModel);
            this.SetPreviewCharacterEnabled(false);

            var textureOffset = -new Vector2(floorPos.x, this.Is2D ? floorPos.y : floorPos.z);
            var propInfo =
                typeof(Camera).GetProperty("PreviewCullingLayer", BindingFlags.Static | BindingFlags.NonPublic);
            var previewLayer = (int) propInfo.GetValue(null, new object[0]);

            // Render main floor
            {
                var mat = this.floorMaterial;
                var matrix = Matrix4x4.TRS(floorPos, floorRot, this.Animator.humanScale * kFloorScale * Vector3.one);

                mat.mainTextureOffset = 1.0f / this.Animator.humanScale * kFloorScale * 0.08f * textureOffset;
//                mat.SetTexture("_ShadowTexture", shadowMap);
//                mat.SetMatrix("_ShadowTextureMatrix", shadowMatrix);
                mat.SetVector(Alphas,
                    new Vector4(kFloorAlpha * mainFloorAlpha, kFloorShadowAlpha * mainFloorAlpha, 0, 0));
                mat.renderQueue = (int) RenderQueue.Background;

                Graphics.DrawMesh(this.floorPlane, matrix, mat, previewLayer,
                    this.previewUtility.camera, 0);
            }

            // Render small floor
            if (dynamicFloorHeight)
            {
                var topIsNext = this.nextFloorHeight > this.prevFloorHeight;
                var floorHeight = topIsNext ? this.nextFloorHeight : this.prevFloorHeight;
                var otherFloorHeight = topIsNext ? this.prevFloorHeight : this.nextFloorHeight;
                var floorAlpha = (floorHeight == mainFloorHeight ? 1 - mainFloorAlpha : 1) *
                                 Mathf.InverseLerp(otherFloorHeight, floorHeight, rootPos.y);
                floorPos.y = floorHeight;

                var mat = this.floorMaterialSmall;
                mat.mainTextureOffset = 0.08f * kFloorScaleSmall * textureOffset;
//                mat.SetTexture("_ShadowTexture", shadowMap);
//                mat.SetMatrix("_ShadowTextureMatrix", shadowMatrix);
                mat.SetVector(Alphas, new Vector4(kFloorAlpha * floorAlpha, 0, 0, 0));
                var matrix = Matrix4x4.TRS(floorPos, floorRot,
                    this.Animator.humanScale * kFloorScaleSmall * Vector3.one);
                Graphics.DrawMesh(this.floorPlane, matrix, mat, previewLayer,
                    this.previewUtility.camera, 0);
            }

            var clearMode = this.previewUtility.camera.clearFlags;
            this.previewUtility.camera.clearFlags = CameraClearFlags.Nothing;
            this.previewUtility.Render();
            this.previewUtility.camera.clearFlags = clearMode;
        }

        private void SetPreviewCharacterEnabled(bool enabled)
        {
            if (this.previewObject != null) SetEnabledRecursive(this.previewObject, enabled);
        }

        private static void SetEnabledRecursive(GameObject go, bool enabled)
        {
            foreach (var componentsInChild in go.GetComponentsInChildren<Renderer>())
                componentsInChild.enabled = enabled;
        }

        public void DrawAvatarPreview(Rect previewRect, GUIStyle background)
        {
            if (!this.isValid)
            {
                var warningRect = previewRect;
                warningRect.yMax -= warningRect.height / 2 - 16;
                EditorGUI.DropShadowLabel(
                    warningRect,
                    "No model is available for preview.\nPlease set a model into this Target Object Field.");

                return;
            }

            var previewId = GUIUtility.GetControlID(this.previewHint, FocusType.Passive, previewRect);
            var evt = Event.current;
            var type = evt.GetTypeForControl(previewId);

            if (type == EventType.Repaint && this.isValid)
            {
                this.DrawRenderPreview(previewRect, background);
                this.previewUtility.EndAndDrawPreview(previewRect);
            }

            var previewSceneId = GUIUtility.GetControlID(this.previewSceneHint, FocusType.Passive);
            type = evt.GetTypeForControl(previewSceneId);
            this.HandleViewTool(evt, type, previewSceneId, previewRect);

            if (evt.type == EventType.Repaint)
                EditorGUIUtility.AddCursorRect(previewRect, this.CurrentCursor);
        }

        private void HandleViewTool(Event evt, EventType eventType, int id, Rect previewRect)
        {
            if (this.previewObject == null) return;
            switch (eventType)
            {
                case EventType.ScrollWheel:
                    this.DoAvatarPreviewZoom(evt, HandleUtility.niceMouseDeltaZoom * (evt.shift ? 2.0f : 0.5f),
                        previewRect);
                    break;
                case EventType.MouseDown:
                    this.HandleMouseDown(evt, id, previewRect);
                    break;
                case EventType.MouseUp:
                    this.HandleMouseUp(evt, id);
                    break;
                case EventType.MouseDrag:
                    this.HandleMouseDrag(evt, id, previewRect);
                    break;
            }
        }

        private void HandleMouseDrag(Event evt, int id, Rect previewRect)
        {
            if (this.previewObject == null)
                return;

            if (GUIUtility.hotControl != id) return;

            switch (this.ViewTool)
            {
                case ViewToolType.Orbit:
                    this.DoAvatarPreviewOrbit(evt, previewRect);
                    break;
                case ViewToolType.Pan:
                    this.DoAvatarPreviewPan(evt);
                    break;
                case ViewToolType.Zoom:
                    this.DoAvatarPreviewZoom(evt, -HandleUtility.niceMouseDeltaZoom * (evt.shift ? 2.0f : 0.5f),
                        previewRect);
                    break;
                default:
                    Debug.Log("Enum value not handled");
                    break;
            }
        }

        private void DoAvatarPreviewOrbit(Event evt, Rect previewRect)
        {
            if (this.is2D) this.is2D = false;
            this.previewDir -= 140.0f * (evt.shift ? 3 : 1) / Mathf.Min(previewRect.width, previewRect.height) *
                               evt.delta;
            this.previewDir.y = Mathf.Clamp(this.previewDir.y, -90, 90);
            evt.Use();
        }

        private void DoAvatarPreviewPan(Event evt)
        {
            var cam = this.previewUtility.camera;
            var screenPos = cam.WorldToScreenPoint(this.Animator.bodyPosition + this.pivotPositionOffset);
            var delta = new Vector3(-evt.delta.x, evt.delta.y, 0);
            // delta panning is scale with the zoom factor to allow fine tuning when user is zooming closely.
            screenPos += 2.0f * Mathf.Lerp(0.25f, 2.0f, this.zoomFactor * 0.5f) * delta;
            var worldDelta = cam.ScreenToWorldPoint(screenPos) -
                             (this.Animator.bodyPosition + this.pivotPositionOffset);
            this.pivotPositionOffset += worldDelta;
            evt.Use();
        }


        private void HandleMouseUp(Event evt, int id)
        {
            if (GUIUtility.hotControl != id) return;
            this.viewTool = ViewToolType.None;

            GUIUtility.hotControl = 0;
            EditorGUIUtility.SetWantsMouseJumping(0);
            evt.Use();
        }

        private void HandleMouseDown(Event evt, int id, Rect previewRect)
        {
            if (this.ViewTool == ViewToolType.None || !previewRect.Contains(evt.mousePosition)) return;
            EditorGUIUtility.SetWantsMouseJumping(1);
            evt.Use();
            GUIUtility.hotControl = id;
        }

        private void DoAvatarPreviewZoom(Event evt, float delta, Rect previewRect)
        {
            if (!previewRect.Contains(evt.mousePosition)) return;
            var zoomDelta = -delta * 0.05f;
            this.zoomFactor += this.zoomFactor * zoomDelta;

            // zoom is clamp too 10 time closer than the original zoom
            this.zoomFactor = Mathf.Max(this.zoomFactor, this.Animator.humanScale / 10.0f);
            evt.Use();
        }

        public void OnDisable()
        {
            if (this.previewUtility == null) return;
            this.previewUtility.Cleanup();
            this.previewUtility = null;
        }

        private enum ViewToolType
        {
            None,
            Pan,
            Zoom,
            Orbit
        }

        private enum PreviewPopupOptions
        {
            Auto,
            DefaultModel,
            Other
        }
    }
}