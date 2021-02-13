using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace AlmostEngine.Screenshot
{
    /// <summary>
    /// The Screenshot taker is a component used to capture the game, with various capture modes and custom settings.
    /// </summary>
    [ExecuteInEditMode]
    public class ScreenshotTaker : MonoBehaviour
    {
        public enum ColorFormat
        {
            RGB,
            RGBA
        }
        ;

        public enum CaptureMode
        {
            GAMEVIEW_RESIZING,
            RENDER_TO_TEXTURE,
            FIXED_GAMEVIEW
        }
        ;


        /// <summary>
        /// The texture containing the last screenshot taken.
        /// </summary>
        public Texture2D m_Texture;


        public enum GameViewResizingWaitingMode
        {
            FRAMES,
            TIME
        }
        ;


        /*
		[Tooltip ("In gameview resizing mode, the number of frames the screenshot taker waits before to take the screenshot after the gameview has been resized. " +
		"The default value of 2 should be enough for most settings. " +
		"Increase this number when some elements are not well updated, like GUI, or when you see some post effects artefacts. " +
		"Post effects like temporal anti aliasing requier a value of at least 10.")]
		*/
        [HideInInspector]
        public GameViewResizingWaitingMode m_GameViewResizingWaitingMode;
        [HideInInspector]
        public float m_GameViewResizingWaitingTime = 1f;
        [HideInInspector]
        public int m_GameViewResizingWaitingFrames = 2;



        //		ScreenshotResolution m_CaptureResolution = new ScreenshotResolution ();
        Dictionary<ScreenshotResolution, RenderTexture> m_RenderTextureCache = new Dictionary<ScreenshotResolution, RenderTexture>();


#if (UNITY_EDITOR) && (!UNITY_5_4_OR_NEWER)
		[HideInInspector]
		public bool m_NeedRestoreLayout;

		[Tooltip ("If true, the editor layout is saved and restored before and after each capture process.")]
		public bool m_ForceLayoutPreservation = true;
#endif

        [HideInInspector]
        public static bool m_IsRunning = false;
        List<ScreenshotCamera> m_Cameras = new List<ScreenshotCamera>();
        List<ScreenshotCamera> m_SceneCameras = new List<ScreenshotCamera>();
        List<ScreenshotOverlay> m_Overlays = new List<ScreenshotOverlay>();
        List<ScreenshotOverlay> m_SceneOverlays = new List<ScreenshotOverlay>();


        void OnDestroy()
        {
            Reset();
        }

        void Update()
        {
#if UNITY_EDITOR
            if (EditorApplication.isCompiling)
            {
                Reset();
                return;
            }
#endif

#if (UNITY_EDITOR) && (!UNITY_5_4_OR_NEWER)
			RestoreLayoutIfNeeded ();							
#endif
        }

        public void Reset()
        {
            StopAllCoroutines();


#if (UNITY_EDITOR) && (!UNITY_5_4_OR_NEWER)
			RestoreLayoutIfNeeded ();				
#endif


            RestoreTime();
            RestoreSettings();

            m_IsRunning = false;
        }

        /// <summary>
        /// Clears the cache of RenderTexture used to capture the screenshots.
        /// </summary>
        public void ClearCache()
        {
            m_RenderTextureCache.Clear();
            //			m_CaptureResolution.m_Texture = null;
        }

        #region API

        //		public delegate void UpdateDelegate (ScreenshotResolution res);

        /// <summary>
        /// Delegate called when the capture starts.
        /// </summary>
        public static UnityAction<ScreenshotResolution> onResolutionUpdateStartDelegate = (ScreenshotResolution res) =>
        {
        };
        /// <summary>
        /// Delegate called when the capture ends.
        /// </summary>
        public static UnityAction<ScreenshotResolution> onResolutionUpdateEndDelegate = (ScreenshotResolution res) =>
        {
        };
        /// <summary>
        /// Delegate called when the screen is resized.
        /// </summary>
        public static UnityAction<ScreenshotResolution> onResolutionScreenResizedDelegate = (ScreenshotResolution res) =>
        {
        };



        /// <summary>
        /// Captures the current screen at its current resolution.
        /// The texture will be resized if needed to match the capture settings.
        /// </summary>
        public IEnumerator CaptureScreenToTextureCoroutine(Texture2D texture,
                                                            bool captureGameUI = true,
                                                            ScreenshotTaker.ColorFormat colorFormat = ScreenshotTaker.ColorFormat.RGB,
                                                            bool recomputeAlphaMask = false)
        {
            Vector2 size = GameViewController.GetCurrentGameViewSize();
            yield return StartCoroutine(CaptureToTextureCoroutine(texture, (int)size.x, (int)size.y, null, null,
                ScreenshotTaker.CaptureMode.FIXED_GAMEVIEW,
                8, captureGameUI, colorFormat, recomputeAlphaMask));
        }

        /// <summary>
        /// Captures the scene with the specified width, height, using the mode RENDER_TO_TEXTURE.
        /// Screenspace Overlay Canvas can not be captured with this mode.
        /// The texture will be resized if needed to match the capture settings.
        /// </summary>
        public IEnumerator CaptureCamerasToTextureCoroutine(Texture2D texture, int width, int height,
                                                             List<Camera> cameras,
                                                             int antiAliasing = 8,
                                                             ScreenshotTaker.ColorFormat colorFormat = ScreenshotTaker.ColorFormat.RGB,
                                                             bool recomputeAlphaMask = false)
        {
            yield return StartCoroutine(CaptureToTextureCoroutine(texture, width, height, cameras, null, ScreenshotTaker.CaptureMode.RENDER_TO_TEXTURE, antiAliasing, true, colorFormat, recomputeAlphaMask));
        }

        /// <summary>
        /// Captures the game with the specified width, height.
        /// The texture will be resized if needed to match the capture settings.
        /// </summary>
        public IEnumerator CaptureToTextureCoroutine(Texture2D texture, int width, int height,
                                                      List<Camera> cameras = null,
                                                      List<Canvas> canvas = null,
                                                      ScreenshotTaker.CaptureMode captureMode = ScreenshotTaker.CaptureMode.RENDER_TO_TEXTURE,
                                                      int antiAliasing = 8,
                                                      bool captureGameUI = true,
                                                      ScreenshotTaker.ColorFormat colorFormat = ScreenshotTaker.ColorFormat.RGB,
                                                      bool recomputeAlphaMask = false)
        {
            // Check texture
            if (texture == null)
            {
                Debug.LogError("The texture can not be null. You must provide a texture initialized with any width and height.");
                yield break;
            }

            // Update resolution item
            ScreenshotResolution captureResolution = new ScreenshotResolution(width, height);
            captureResolution.m_Texture = texture;

            yield return StartCoroutine(CaptureResolutionCoroutine(captureResolution, cameras, canvas, captureMode, antiAliasing, captureGameUI, colorFormat, recomputeAlphaMask));

        }


        public IEnumerator CaptureResolutionCoroutine(ScreenshotResolution captureResolution,
                                                       List<Camera> cameras = null,
                                                       List<Canvas> canvas = null,
                                                       ScreenshotTaker.CaptureMode captureMode = ScreenshotTaker.CaptureMode.RENDER_TO_TEXTURE,
                                                       int antiAliasing = 8,
                                                       bool captureGameUI = true,
                                                       ScreenshotTaker.ColorFormat colorFormat = ScreenshotTaker.ColorFormat.RGB,
                                                       bool recomputeAlphaMask = false)
        {

            // Create camera items
            List<ScreenshotCamera> screenshotCameras = new List<ScreenshotCamera>();
            if (cameras != null)
            {
                foreach (Camera camera in cameras)
                {
                    ScreenshotCamera scamera = new ScreenshotCamera(camera);
                    screenshotCameras.Add(scamera);
                }
            }

            // Create the overlays items
            List<ScreenshotOverlay> screenshotCanvas = new List<ScreenshotOverlay>();
            if (canvas != null)
            {
                foreach (Canvas c in canvas)
                {
                    ScreenshotOverlay scanvas = new ScreenshotOverlay(c);
                    screenshotCanvas.Add(scanvas);
                }
            }

            yield return StartCoroutine(CaptureAllCoroutine(new List<ScreenshotResolution> { captureResolution },
                screenshotCameras, screenshotCanvas,
                captureMode, antiAliasing, captureGameUI, colorFormat, recomputeAlphaMask));

        }


        #endregion


        #region Capture



        public IEnumerator CaptureAllCoroutine(List<ScreenshotResolution> resolutions,
                                                List<ScreenshotCamera> cameras,
                                                List<ScreenshotOverlay> overlays,
                                                CaptureMode captureMode,
                                                int antiAliasing = 8,
                                                bool captureGameUI = true,
                                                ColorFormat colorFormat = ColorFormat.RGB,
                                                bool recomputeAlphaMask = false,
                                                bool stopTime = false,
                                                bool restore = true)
        {

            //			Debug.Log ("Capture all");

            if (resolutions == null)
            {
                Debug.LogError("Resolution list is null.");
                yield break;
            }
            if (cameras == null)
            {
                Debug.LogError("Cameras list is null.");
                yield break;
            }
            if (cameras.Count == 0 && captureMode == CaptureMode.RENDER_TO_TEXTURE)
            {
                cameras.Add(new ScreenshotCamera(Camera.main));
            }
            if (overlays == null)
            {
                Debug.LogError("Overlays list is null.");
                yield break;
            }
            if (captureMode == CaptureMode.RENDER_TO_TEXTURE && !UnityVersion.HasPro())
            {
                Debug.LogError("RENDER_TO_TEXTURE requires Unity Pro or Unity 5.0 and later.");
                yield break;
            }

            // If a capture is in progress we wait until we can take the screenshot
            if (m_IsRunning == true)
            {
                Debug.LogWarning("A capture process is already running.");
            }
            while (m_IsRunning == true)
            {
                yield return null;
            }


#if (!UNITY_EDITOR && !UNITY_STANDALONE_WIN)
						if (captureMode == CaptureMode.GAMEVIEW_RESIZING) {
								Debug.LogError ("GAMEVIEW_RESIZING capture mode is only available for Editor and Windows Standalone.");
								yield break;
						}
#endif

            // Init
            m_IsRunning = true;

            // Stop the time so all screenshots are exactly the same
            if (Application.isPlaying && stopTime)
            {
                StopTime();
            }


            // Apply settings: enable and disable the cameras and canvas
            ApplySettings(cameras, overlays, captureMode, captureGameUI);


            // Save the screen config to be restored after the capture process
            if (captureMode == CaptureMode.GAMEVIEW_RESIZING)
            {
                GameViewController.SaveCurrentGameViewSize();

#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
								yield return null;
								yield return new WaitForEndOfFrame();
#endif
            }

            // Capture all resolutions
            foreach (ScreenshotResolution resolution in resolutions)
            {
                if (!resolution.IsValid())
                    continue;

                // Delegate call
                onResolutionUpdateStartDelegate(resolution);

                if (colorFormat == ColorFormat.RGBA && recomputeAlphaMask)
                {
                    yield return StartCoroutine(CaptureAlphaMaskCoroutine(resolution, captureMode, antiAliasing, colorFormat));
                }
                else
                {
                    yield return StartCoroutine(CaptureResolutionTextureCoroutine(resolution, captureMode, antiAliasing, colorFormat));
                }

                // Delegate call
                onResolutionUpdateEndDelegate(resolution);




#if UNITY_EDITOR
                // Dirty hack: we force a gameview repaint, to prevent the coroutine to stay locked.
                if (!Application.isPlaying)
                {
                    GameViewUtils.GetGameView().Repaint();
                }
#endif


                //				if (captureMode == CaptureMode.RENDER_TO_TEXTURE && captureGameUI) {
                //					RestoreCanvasRenderMode ();
                //				}

            }

            // Restore screen config
            if (restore && captureMode == CaptureMode.GAMEVIEW_RESIZING)
            {
                GameViewController.RestoreGameViewSize();
            }


#if (UNITY_EDITOR && !UNITY_5_4_OR_NEWER)
			// Call restore layout for old unity versions
			if (restore && captureMode == CaptureMode.GAMEVIEW_RESIZING) {		
				m_NeedRestoreLayout = true;
			} 
#endif

#if UNITY_EDITOR
            // Dirty hack, try to force an editor Update() to get the gameview back to normal
            if (!Application.isPlaying)
            {
                GameViewUtils.GetGameView().Repaint();
            }
#endif

            // Restore everything
            if (Application.isPlaying && stopTime)
            {
                RestoreTime();
            }
            if (Application.isEditor || restore)
            {
                RestoreSettings();
            }

            // End
            m_IsRunning = false;
        }


        /// <summary>
        /// Captures the resolution texture.
        /// </summary>
        IEnumerator CaptureResolutionTextureCoroutine(ScreenshotResolution resolution, CaptureMode captureMode, int antiAliasing, ColorFormat colorFormat)
        {


            // Init texture
            m_Texture = GetOrCreateTexture(resolution, colorFormat, captureMode == CaptureMode.FIXED_GAMEVIEW ? true : false);

            if (captureMode == CaptureMode.GAMEVIEW_RESIZING)
            {

                //				Debug.Log ("Resize ");

                // Force screen size change
                GameViewController.SetGameViewSize(m_Texture.width, m_Texture.height);
                yield return new WaitForEndOfFrame();

                // Force wait
                if (!Application.isPlaying)
                {
                    // Useless texture update in editor mode when game is not running,
                    // that takes some computational times to be sure that the UI is updated at least one time before the capture
                    if (MultiDisplayUtils.IsMultiDisplay())
                    {
                        yield return MultiDisplayCopyRenderBufferToTextureCoroutine(m_Texture);
                    }
                    else
                    {
                        CopyScreenToTexture(m_Texture);
                    }
                }

                // Delegate call to notify screen is resized
                onResolutionScreenResizedDelegate(resolution);


                // Wait several frames
                // Particularly needed for special effects using several frame to compute their effects, like temporal anti aliasing
                if (m_GameViewResizingWaitingMode == GameViewResizingWaitingMode.FRAMES || !Application.isPlaying)
                {
                    for (int i = 0; i < m_GameViewResizingWaitingFrames; ++i)
                    {
                        //						Debug.Log ("Wait " + i);
                        //GameViewController.SetGameViewSize(m_Texture.width, m_Texture.height);
                        #if UNITY_EDITOR					
						if (!Application.isPlaying){
							GameViewUtils.GetGameView().Repaint();
						}
                        #endif
                        yield return new WaitForEndOfFrame();
                        #if UNITY_EDITOR					
						if (!Application.isPlaying && i < 2){
                            CopyScreenToTexture(m_Texture); // Useless update to force waiting for the gamview to be correctly updated
						}
                        #endif
                        //						Debug.Log ("Wait " + i + " end");
                    }
                }
                else
                {
#if (UNITY_5_4_OR_NEWER)
                    yield return new WaitForSecondsRealtime(m_GameViewResizingWaitingTime);
#else
					if (Time.timeScale > 0f) {
						yield return new WaitForSeconds (m_GameViewResizingWaitingTime);
					}
#endif
                    yield return new WaitForEndOfFrame();
                }


                //				Debug.Log ("Capture");



                // Capture the screen content
                if (MultiDisplayUtils.IsMultiDisplay())
                {
                    yield return MultiDisplayCopyRenderBufferToTextureCoroutine(m_Texture);
                }
                else
                {
                    CopyScreenToTexture(m_Texture);
                }

                //				Debug.Log ("End Capture");

            }
            else if (captureMode == CaptureMode.RENDER_TO_TEXTURE)
            {

                // Wait for the end of rendering
                yield return new WaitForEndOfFrame();

                /*
				#if UNITY_EDITOR
				if (Application.isPlaying) {
					yield return new WaitForEndOfFrame ();
				} else {
					// We need to force a gameview repaint
					Vector2 current = GameViewController.GetCurrentGameViewSize ();
					GameViewController.SetGameViewSize ((int)current.x, (int)current.y);
					yield return new WaitForEndOfFrame ();
				}
				#endif
				*/


                // Do not need to wait anything, just capture the cameras
                RenderTexture renderTexture = GetOrCreateRenderTexture(resolution, antiAliasing);
                RenderCamerasToTexture(m_Cameras, m_Texture, renderTexture);


            }
            else if (captureMode == CaptureMode.FIXED_GAMEVIEW)
            {
                #if UNITY_EDITOR						
                // Force repaint in case the gameview is not focus to prevent a lock
                if (!Application.isPlaying){
                    GameViewUtils.GetGameView().Repaint();
                }
                #endif

                // Wait for the end of rendering
                yield return new WaitForEndOfFrame();

                // Capture the screen content
                if (MultiDisplayUtils.IsMultiDisplay())
                {
                    yield return MultiDisplayCopyRenderBufferToTextureCoroutine(m_Texture);
                }
                else
                {
                    CopyScreenToTexture(m_Texture);
                }
            }

            // Alpha mask
            // if (colorFormat == ColorFormat.RGBA && recomputeAlphaMask)
            // {
            //     // Capture the screen content
            //     yield return StartCoroutine(RecomputeAlphaMask(resolution, m_Cameras, captureMode));
            // }

        }

        public void CopyScreenToTexture(Texture2D targetTexture)
        {
            targetTexture.ReadPixels(new Rect(0, 0, targetTexture.width, targetTexture.height), 0, 0);
            targetTexture.Apply(false);
        }

        public void RenderCamerasToTexture(List<ScreenshotCamera> cameras, Texture2D targetTexture, RenderTexture renderTexture)
        {
            RenderTexture tmp = RenderTexture.active;
            RenderTexture.active = renderTexture;

            // Screescpace Overlays Canvas
            //			if (m_UICamera != null) {
            //				cameras.Add (new ScreenshotCamera (m_UICamera));
            //			}

            // Render all cameras in custom render texture
            foreach (ScreenshotCamera camera in cameras)
            {

                if (camera.m_Active == false)
                    continue;
                if (camera.m_Camera == null)
                    continue;
                if (camera.m_Camera.enabled == false)
                    continue;

                RenderTexture previous = camera.m_Camera.targetTexture;
                camera.m_Camera.targetTexture = renderTexture;

                //				camera.m_Camera.ResetAspect ();
                //				camera.m_Camera.ResetCullingMatrix ();
                //				camera.m_Camera.ResetProjectionMatrix ();
                //				Canvas.ForceUpdateCanvases ();

                camera.m_Camera.Render();
                camera.m_Camera.targetTexture = previous;

            }

            // Copy render buffer to texture
            targetTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            targetTexture.Apply(false);
            RenderTexture.active = tmp;
        }

        #endregion


        #region Multi Display


        Camera GetFirstActiveCamera()
        {
            // Get last camera on the list
            for (int i = 0; i < m_Cameras.Count; i++)
            {
                if (m_Cameras[i].m_Active && m_Cameras[i].m_Camera != null && m_Cameras[i].m_Camera.enabled == true)
                {
                    return m_Cameras[i].m_Camera;
                }
            }
            // If not cameras on the list, get the active camera for display 1 with the higher depth
            Camera[] cameras = GameObject.FindObjectsOfType<Camera>();
            Camera best = null;
            foreach (Camera cam in cameras)
            {

#if (UNITY_5_4_OR_NEWER)
                if (cam.enabled == true && cam.targetDisplay == 0)
                {
#else
				if (cam.enabled == true) {
#endif
                    if (best == null || cam.depth < best.depth)
                    {
                        best = cam;
                    }
                }
            }
            if (best != null)
                return best;
            // Return camera tagged as MainCamera
            return Camera.main;
        }

        Camera GetLastActiveCamera()
        {
            // Get last camera on the list
            for (int i = m_Cameras.Count - 1; i >= 0; i--)
            {
                if (m_Cameras[i].m_Active && m_Cameras[i].m_Camera != null && m_Cameras[i].m_Camera.enabled == true)
                {
                    return m_Cameras[i].m_Camera;
                }
            }
            // If not cameras on the list, get the active camera for display 1 with the higher depth
            Camera[] cameras = GameObject.FindObjectsOfType<Camera>();
            Camera best = null;
            foreach (Camera cam in cameras)
            {

#if (UNITY_5_4_OR_NEWER)
                if (cam.enabled == true && cam.targetDisplay == 0)
                {
#else
				if (cam.enabled == true) {
#endif
                    if (best == null || cam.depth > best.depth)
                    {
                        best = cam;
                    }
                }
            }
            if (best != null)
                return best;
            // Return camera tagged as MainCamera
            return Camera.main;
        }

        IEnumerator MultiDisplayCopyRenderBufferToTextureCoroutine(Texture2D targetTexture)
        {
            Camera lastCamera = GetLastActiveCamera();

            // On multi display we need to wait for the last camera to capture to be rendered
            if (lastCamera != null)
            {
                // Add a capture camera component and start the capture process
                if (lastCamera.GetComponent<MultiDisplayCameraCapture>() == null)
                {
                    lastCamera.gameObject.AddComponent<MultiDisplayCameraCapture>();
                }
                MultiDisplayCameraCapture capture = lastCamera.GetComponent<MultiDisplayCameraCapture>();
                capture.CaptureCamera(targetTexture);
                // Wait for capture
                while (!capture.CopyIsOver())
                {
                    yield return null;
                }
                // Clean
                GameObject.DestroyImmediate(capture);

            }
            else
            {
                // Just read the actual render buffer
                CopyScreenToTexture(targetTexture);
            }
        }

        #endregion


        #region TEXTURE

        Texture2D GetOrCreateTexture(ScreenshotResolution resolution, ColorFormat colorFormat, bool noScale = false)
        {

            // Compute real dimensions
            int width = noScale ? resolution.m_Width : resolution.ComputeTargetWidth();
            int height = noScale ? resolution.m_Height : resolution.ComputeTargetHeight();

            // Create texture if needed
            if (resolution.m_Texture == null)
            {
                resolution.m_Texture = new Texture2D(width, height, colorFormat == ColorFormat.RGBA ? TextureFormat.ARGB32 : TextureFormat.RGB24, false);
            }
            else if (resolution.m_Texture.width != width || resolution.m_Texture.height != height ||
                     (resolution.m_Texture.format == TextureFormat.ARGB32 && colorFormat != ColorFormat.RGBA) ||
                     (resolution.m_Texture.format == TextureFormat.RGB24 && colorFormat != ColorFormat.RGB))
            {
                resolution.m_Texture.Resize(width, height, colorFormat == ColorFormat.RGBA ? TextureFormat.ARGB32 : TextureFormat.RGB24, false);
            }

            return resolution.m_Texture;
        }

        RenderTexture GetOrCreateRenderTexture(ScreenshotResolution resolution, int antiAliasing = 0)
        {
            // Compute real resolutions
            int width = resolution.ComputeTargetWidth();
            int height = resolution.ComputeTargetHeight();

            // Create render texture if needed
            if (!m_RenderTextureCache.ContainsKey(resolution) || m_RenderTextureCache[resolution] == null
                || m_RenderTextureCache[resolution].width != width || m_RenderTextureCache[resolution].height != height
                || m_RenderTextureCache[resolution].antiAliasing != antiAliasing)
            {
                m_RenderTextureCache[resolution] = new RenderTexture(width, height, 32, RenderTextureFormat.ARGB32);
                if (antiAliasing != 0)
                {
                    m_RenderTextureCache[resolution].antiAliasing = antiAliasing;
                }
            }

            return m_RenderTextureCache[resolution];
        }

        #endregion


        #region Screenspae Overlay Canvas in Render To Texture

        /*
		List<Canvas> m_ScreenspaceOverlayCanvas = new List<Canvas> ();

		void FindScreenspaceOverlayCanvas ()
		{
			m_ScreenspaceOverlayCanvas.Clear ();
			var canvas = GameObject.FindObjectsOfType<Canvas> ();
			foreach (Canvas c in canvas) {
				if (c.renderMode == RenderMode.ScreenSpaceOverlay) {
					m_ScreenspaceOverlayCanvas.Add (GameObject.Instantiate (c.gameObject).GetComponent<Canvas> ());
				}
			}
//			foreach (ScreenshotOverlay c in m_Overlays) {
//				if (c.m_Canvas.renderMode == RenderMode.ScreenSpaceOverlay) {
//					m_ScreenspaceOverlayCanvas.Add (GameObject.Instantiate (c.m_Canvas.gameObject).GetComponent<Canvas> ());
//				}
//			}
		}

		Camera m_UICamera;
		Vector3 farWay = new Vector3 (99999f, -999999f, 999999f);

		void SetCanvasRenderMode (ScreenshotResolution resolution)
		{
			var go = new GameObject ();
			m_UICamera = go.AddComponent<Camera> ();
			m_UICamera.gameObject.SetActive (false);
			m_UICamera.enabled = true;
			m_UICamera.transform.position = farWay + new Vector3 (0f, 0f, -1f);	
			m_UICamera.name = "Temporary UI Camera - destroy if persistent";
			m_UICamera.depth = 999;
			m_UICamera.orthographic = true;
			m_UICamera.clearFlags = CameraClearFlags.Depth;
			m_UICamera.cullingMask = 1 << LayerMask.NameToLayer ("UI");
//			float max = Mathf.Max (resolution.ComputeTargetWidth (), resolution.ComputeTargetHeight ());
			m_UICamera.orthographicSize = resolution.ComputeTargetHeight () / 2f;
			m_UICamera.rect = new Rect (0, 0, resolution.ComputeTargetWidth (), resolution.ComputeTargetHeight ());
//			Debug.Log ("Aspect " + m_UICamera.aspect);
//			m_UICamera.aspect = (float)resolution.ComputeTargetWidth () / (float)resolution.ComputeTargetHeight ();
//			Debug.Log ("new Aspect " + m_UICamera.aspect);




			foreach (Canvas c in m_ScreenspaceOverlayCanvas) {
				c.worldCamera = m_UICamera;
//				c.renderMode = RenderMode.ScreenSpaceCamera;
				c.renderMode = RenderMode.WorldSpace;
				c.transform.position = farWay;
				c.transform.localScale = Vector3.one;
				c.GetComponent<RectTransform> ().SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, (float)resolution.ComputeTargetWidth ());
				c.GetComponent<RectTransform> ().SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, (float)resolution.ComputeTargetHeight ());
//				c.GetComponent<RectTransform>(). .rect = new Rect(0f,0f, (float)resolution.ComputeTargetWidth (), (float)resolution.ComputeTargetHeight ());
//				c.BroadcastMessage ("Update");
//				c.scaleFactor = (float)resolution.ComputeTargetWidth () / (float)resolution.ComputeTargetHeight ();
//				c.GetComponent<CanvasScaler> ();
			}
		}

		void RestoreCanvasRenderMode ()
		{
			foreach (Canvas c in m_ScreenspaceOverlayCanvas) {
				GameObject.DestroyImmediate (c.gameObject);
//				c.renderMode = RenderMode.ScreenSpaceOverlay;
			}
			GameObject.DestroyImmediate (m_UICamera.gameObject);
		}

*/

        #endregion


        #region GENERAL SETTINGS

        public void ApplySettings(List<ScreenshotCamera> cameras, List<ScreenshotOverlay> overlays, CaptureMode captureMode, bool renderUI)
        {

            // SET CAMERAS	
            m_Cameras = cameras;
            m_SceneCameras = FindAllOtherSceneCameras(m_Cameras);
            if (captureMode != CaptureMode.RENDER_TO_TEXTURE && m_Cameras.Count > 0)
            {
                DisableCameras(m_SceneCameras);
            }
            ApplyCameraSettings(m_Cameras, captureMode);

            // SET OVERLAYS
            m_Overlays = overlays;
            m_SceneOverlays = FindAllOtherSceneCanvas(m_Overlays);
            if (captureMode != CaptureMode.RENDER_TO_TEXTURE && renderUI == false)
            {
                DisableCanvas(m_SceneOverlays);
            }
            ApplyOverlaySettings(m_Overlays);
        }

        void RestoreSettings()
        {
            // Restore cameras
            RestoreCameraSettings(m_Cameras);
            RestoreCameraSettings(m_SceneCameras);

            // Restore overlays
            RestoreOverlaySettings(m_Overlays);
            RestoreOverlaySettings(m_SceneOverlays);
        }

        float m_PreviousTimeScale = 1f;

        void StopTime()
        {
            m_PreviousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        void RestoreTime()
        {
            Time.timeScale = m_PreviousTimeScale;
        }

#if (UNITY_EDITOR) && (!UNITY_5_4_OR_NEWER)
		void RestoreLayoutIfNeeded ()
		{
//			if (m_NeedRestoreLayout) {
//				m_NeedRestoreLayout = false;
//				if (m_ForceLayoutPreservation) {
//					GameViewController.RestoreLayout ();
//				}
//			}
		}
#endif

        #endregion


        #region CANVAS SETTINGS

        List<ScreenshotOverlay> FindAllOtherSceneCanvas(List<ScreenshotOverlay> overlays)
        {
            List<ScreenshotOverlay> sceneUIOverlaysCanvas = new List<ScreenshotOverlay>();

            // Find all canvas using screenspaceoverlay on the scene
            Canvas[] canvas = GameObject.FindObjectsOfType<Canvas>();
            foreach (Canvas canva in canvas)
            {
                if (canva.gameObject.activeInHierarchy == true
                    && canva.enabled == true)
                {

                    // Ignore overlays canvas
                    bool contains = false;
                    foreach (ScreenshotOverlay overlay in overlays)
                    {
                        if (overlay.m_Canvas == canva && overlay.m_Active)
                            contains = true;
                    }
                    if (contains)
                        continue;

                    // Add canvas to list
                    sceneUIOverlaysCanvas.Add(new ScreenshotOverlay(canva));
                }
            }
            return sceneUIOverlaysCanvas;
        }

        void DisableCanvas(List<ScreenshotOverlay> overlays)
        {
            if (overlays == null)
                return;
            foreach (ScreenshotOverlay overlay in overlays)
            {
                if (overlay == null)
                    continue;
                overlay.Disable();
            }
        }

        void ApplyOverlaySettings(List<ScreenshotOverlay> overlays)
        {
            if (overlays == null)
                return;
            foreach (ScreenshotOverlay overlay in overlays)
            {
                if (overlay == null)
                    continue;
                if (overlay.m_Active && overlay.m_Canvas != null)
                {
                    overlay.ApplySettings();
                }
            }
        }

        public void RestoreOverlaySettings(List<ScreenshotOverlay> overlays)
        {
            if (overlays == null)
                return;
            foreach (ScreenshotOverlay overlay in overlays)
            {
                if (overlay == null)
                    continue;
                if (overlay.m_Active && overlay.m_Canvas != null)
                {
                    overlay.RestoreSettings();
                }
            }
        }

        #endregion


        #region CAMERAS SETTINGS


        List<ScreenshotCamera> FindAllOtherSceneCameras(List<ScreenshotCamera> cameras)
        {
            List<ScreenshotCamera> cams = new List<ScreenshotCamera>();
            Camera[] sceneCameras = GameObject.FindObjectsOfType<Camera>();
            foreach (Camera camera in sceneCameras)
            {
                bool contains = false;
                foreach (ScreenshotCamera cam in cameras)
                {
                    if (cam.m_Camera == camera && cam.m_Active == true)
                    {
                        contains = true;
                    }
                }
                if (!contains)
                {
                    cams.Add(new ScreenshotCamera(camera));
                }
            }
            return cams;
        }

        void DisableCameras(List<ScreenshotCamera> cameras)
        {
            if (cameras == null)
                return;

            foreach (ScreenshotCamera camera in cameras)
            {
                if (camera == null)
                    continue;
                camera.Disable();
            }
        }

        public void RestoreCameraSettings(List<ScreenshotCamera> cameras)
        {
            if (cameras == null)
                return;

            foreach (ScreenshotCamera camera in cameras)
            {
                if (camera == null)
                    continue;
                if (camera.m_Active == false)
                    continue;
                if (camera.m_Camera == null)
                    continue;
                camera.RestoreSettings();
            }
        }

        void ApplyCameraSettings(List<ScreenshotCamera> cameras, CaptureMode captureMode)
        {
            if (cameras == null)
                return;

            foreach (ScreenshotCamera camera in cameras)
            {
                if (camera == null)
                    continue;
                if (camera.m_Active == false)
                    continue;
                if (camera.m_Camera == null)
                    continue;

                camera.ApplySettings(captureMode == CaptureMode.RENDER_TO_TEXTURE);
            }
        }

        #endregion


        #region Transparency

        IEnumerator CaptureAlphaMaskCoroutine(ScreenshotResolution resolution, CaptureMode captureMode, int antiAliasing, ColorFormat colorFormat)
        {


            // Backup first camera clear mode
            var firstCamera = GetFirstActiveCamera();
            var clearMode = firstCamera.clearFlags;
            var clearColor = firstCamera.backgroundColor;

            // Set clear white alpha
            firstCamera.clearFlags = CameraClearFlags.Color;
            firstCamera.backgroundColor = new Color(1f, 1f, 1f, 0.1f);

            // Capture the texture
            yield return StartCoroutine(CaptureResolutionTextureCoroutine(resolution, captureMode, antiAliasing, colorFormat));

            // Copy white texture
            Texture2D whiteTexture = new Texture2D(resolution.m_Texture.width, resolution.m_Texture.height, resolution.m_Texture.format, false, false);
            CopyTexture(resolution.m_Texture, whiteTexture);

            // Set clear black alpha
            firstCamera.clearFlags = CameraClearFlags.Color;
            firstCamera.backgroundColor = new Color(0f, 0f, 0f, 0.1f);

            // Capture the texture, again
            yield return StartCoroutine(CaptureResolutionTextureCoroutine(resolution, captureMode, antiAliasing, colorFormat));

            // Copy black texture
            Texture2D blackTexture = new Texture2D(resolution.m_Texture.width, resolution.m_Texture.height, resolution.m_Texture.format, false, false);
            CopyTexture(resolution.m_Texture, blackTexture);

            // Compute diff
            Color color;
            for (int x = 0; x < resolution.m_Texture.width; ++x)
            {
                for (int y = 0; y < resolution.m_Texture.height; ++y)
                {
                    var alpha = whiteTexture.GetPixel(x, y).r - blackTexture.GetPixel(x, y).r;
                    alpha = 1f - alpha;
                    if (alpha == 0)
                    {
                        color = Color.clear;
                    }
                    else
                    {
                        // color = whiteTexture.GetPixel(x, y) / alpha;
                        color = whiteTexture.GetPixel(x, y);
                    }
                    color.a = alpha;
                    resolution.m_Texture.SetPixel(x, y, color);
                }
            }
            resolution.m_Texture.Apply();

            // Clean
            DestroyImmediate(whiteTexture);
            DestroyImmediate(blackTexture);

            // Restore clear settings
            firstCamera.clearFlags = clearMode;
            firstCamera.backgroundColor = clearColor;

        }

        #endregion

        static void CopyTexture(Texture2D src, Texture2D dest)
        {
#if UNITY_5_4_OR_NEWER
            Graphics.CopyTexture(src, dest);
#else
			dest.Resize(src.width, src.height, src.format, false);
            for (int x = 0; x < src.width; ++x)
            {
                for (int y = 0; y < src.height; ++y)
                {
					dest.SetPixel(x,y,src.GetPixel(x,y));
				}
			}
			dest.Apply();
#endif
        }



    }
}
