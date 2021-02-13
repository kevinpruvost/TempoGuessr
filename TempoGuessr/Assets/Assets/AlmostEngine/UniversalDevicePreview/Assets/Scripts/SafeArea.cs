using UnityEngine;
using UnityEngine.UI;

using AlmostEngine.Screenshot;


#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR && (UNITY_2019_4_OR_NEWER || UNITY_2020_1_OR_NEWER)
using System.IO;
#endif

namespace AlmostEngine.Preview
{
    [ExecuteInEditMode]
    public class SafeArea : MonoBehaviour
    {
        [HideInInspector]
        public RectTransform m_Panel;

        #region Constraints

        public enum HorizontalConstraint
        {
            LEFT,
            RIGHT,
            LEFT_AND_RIGHT
        }
        ;

        public enum VerticalConstraint
        {
            UP,
            DOWN,
            UP_AND_DOWN
        }
        ;


        public enum Constraint
        {
            NONE,
            SNAP,
            PUSH,
            ENLARGE
        }
        ;

        public Constraint m_HorizontalConstraintType = Constraint.NONE;
        public HorizontalConstraint m_HorizontalConstraint = HorizontalConstraint.LEFT_AND_RIGHT;

        public Constraint m_VerticalConstraintType = Constraint.NONE;
        public VerticalConstraint m_VerticalConstraint = VerticalConstraint.UP_AND_DOWN;

        #endregion



        #region Safe Area & constraints

        public Vector2 m_DefaultAnchorMin = new Vector2(-99f, -99f);
        public Vector2 m_DefaultAnchorMax = new Vector2(-99f, -99f);

        public void Restore()
        {
            // Restore default anchor
            // Debug.Log("Restore");
            if (m_DefaultAnchorMin != new Vector2(-99f, -99f))
            {
                m_Panel.anchorMin = m_DefaultAnchorMin;
                m_Panel.anchorMax = m_DefaultAnchorMax;
            }
        }

        void ApplySafeArea()
        {
            Rect safeArea = DeviceInfo.GetSafeArea();
            ApplySafeArea(safeArea);
        }

        [System.NonSerialized]
        Rect m_LastSafeArea = new Rect(-1, -1, -1, -1);

        void ApplySafeAreaIfNeeded()
        {
            // We only apply safe area when detecting a change
            Rect safeArea = DeviceInfo.GetSafeArea();
            if (safeArea != m_LastSafeArea)
            {
                ApplySafeArea();
            }
        }


        public void ApplySafeArea(Rect rect)
        {
            // Debug.Log("Apply " + name + " safe area " + rect);

            if (m_Panel == null || Application.isEditor)
            {
                m_Panel = GetComponent<RectTransform>();
            }
            // Init the original anchor values if required
            if (m_DefaultAnchorMin == new Vector2(-99f, -99f))
            {
                m_DefaultAnchorMin = m_Panel.anchorMin;
                m_DefaultAnchorMax = m_Panel.anchorMax;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // Save current values to enable UNDO if something goes wrong
                Undo.RecordObject(m_Panel, "Safe Area");
            }
#endif

            // Safe current safe area to apply the modification only when changed
            m_LastSafeArea = rect;

            // Init default anchor change
            m_Panel.anchorMin = m_DefaultAnchorMin;
            m_Panel.anchorMax = m_DefaultAnchorMax;

            // HORIZONTAL
            if (m_HorizontalConstraintType == Constraint.SNAP)
            {
                if (m_HorizontalConstraint == HorizontalConstraint.LEFT || m_HorizontalConstraint == HorizontalConstraint.LEFT_AND_RIGHT)
                {
                    Vector2 anchorMin = m_Panel.anchorMin;
                    anchorMin.x = rect.position.x / Screen.width;
                    m_Panel.anchorMin = anchorMin;
                }
                if (m_HorizontalConstraint == HorizontalConstraint.RIGHT || m_HorizontalConstraint == HorizontalConstraint.LEFT_AND_RIGHT)
                {
                    Vector2 anchorMax = m_Panel.anchorMax;
                    anchorMax.x = (rect.position.x + rect.size.x) / Screen.width;
                    m_Panel.anchorMax = anchorMax;
                }
            }

            if (m_HorizontalConstraintType == Constraint.ENLARGE)
            {
                if (m_HorizontalConstraint == HorizontalConstraint.LEFT || m_HorizontalConstraint == HorizontalConstraint.LEFT_AND_RIGHT)
                {
                    Vector2 anchorMax = m_Panel.anchorMax;
                    anchorMax.x = anchorMax.x + rect.position.x / Screen.width;
                    m_Panel.anchorMax = anchorMax;
                }
                if (m_HorizontalConstraint == HorizontalConstraint.RIGHT || m_HorizontalConstraint == HorizontalConstraint.LEFT_AND_RIGHT)
                {
                    Vector2 anchorMin = m_Panel.anchorMin;
                    anchorMin.x = anchorMin.x - (Screen.width - rect.width - rect.position.x) / Screen.width;
                    m_Panel.anchorMin = anchorMin;
                }
            }


            if (m_HorizontalConstraintType == Constraint.PUSH)
            {
                if (m_HorizontalConstraint == HorizontalConstraint.LEFT || m_HorizontalConstraint == HorizontalConstraint.LEFT_AND_RIGHT)
                {
                    Vector2 anchorMin = m_Panel.anchorMin;
                    anchorMin.x = anchorMin.x + rect.position.x / Screen.width;
                    m_Panel.anchorMin = anchorMin;
                    Vector2 anchorMax = m_Panel.anchorMax;
                    anchorMax.x = anchorMax.x + rect.position.x / Screen.width;
                    m_Panel.anchorMax = anchorMax;
                }
                if (m_HorizontalConstraint == HorizontalConstraint.RIGHT || m_HorizontalConstraint == HorizontalConstraint.LEFT_AND_RIGHT)
                {
                    Vector2 anchorMin = m_Panel.anchorMin;
                    anchorMin.x = anchorMin.x - (Screen.width - rect.width - rect.position.x) / Screen.width;
                    m_Panel.anchorMin = anchorMin;
                    Vector2 anchorMax = m_Panel.anchorMax;
                    anchorMax.x = anchorMax.x - (Screen.width - rect.width - rect.position.x) / Screen.width;
                    m_Panel.anchorMax = anchorMax;
                }
            }


            // VERTICAL

            if (m_VerticalConstraintType == Constraint.SNAP)
            {
                if (m_VerticalConstraint == VerticalConstraint.DOWN || m_VerticalConstraint == VerticalConstraint.UP_AND_DOWN)
                {
                    Vector2 anchorMin = m_Panel.anchorMin;
                    anchorMin.y = rect.position.y / Screen.height;
                    m_Panel.anchorMin = anchorMin;
                }
                if (m_VerticalConstraint == VerticalConstraint.UP || m_VerticalConstraint == VerticalConstraint.UP_AND_DOWN)
                {
                    Vector2 anchorMax = m_Panel.anchorMax;
                    anchorMax.y = (rect.position.y + rect.size.y) / Screen.height;
                    m_Panel.anchorMax = anchorMax;
                }
            }

            if (m_VerticalConstraintType == Constraint.ENLARGE)
            {
                if (m_VerticalConstraint == VerticalConstraint.UP || m_VerticalConstraint == VerticalConstraint.UP_AND_DOWN)
                {
                    Vector2 anchorMin = m_Panel.anchorMin;
                    anchorMin.y = anchorMin.y - (Screen.height - rect.height - rect.position.y) / Screen.height;
                    m_Panel.anchorMin = anchorMin;
                }
                if (m_VerticalConstraint == VerticalConstraint.DOWN || m_VerticalConstraint == VerticalConstraint.UP_AND_DOWN)
                {
                    Vector2 anchorMax = m_Panel.anchorMax;
                    anchorMax.y = anchorMax.y + rect.position.y / Screen.height;
                    m_Panel.anchorMax = anchorMax;
                }
            }

            if (m_VerticalConstraintType == Constraint.PUSH)
            {
                if (m_VerticalConstraint == VerticalConstraint.UP || m_VerticalConstraint == VerticalConstraint.UP_AND_DOWN)
                {
                    Vector2 anchorMin = m_Panel.anchorMin;
                    anchorMin.y = anchorMin.y - (Screen.height - rect.height - rect.position.y) / Screen.height;
                    m_Panel.anchorMin = anchorMin;
                    Vector2 anchorMax = m_Panel.anchorMax;
                    anchorMax.y = anchorMax.y - (Screen.height - rect.height - rect.position.y) / Screen.height;
                    m_Panel.anchorMax = anchorMax;
                }
                if (m_VerticalConstraint == VerticalConstraint.DOWN || m_VerticalConstraint == VerticalConstraint.UP_AND_DOWN)
                {
                    Vector2 anchorMin = m_Panel.anchorMin;
                    anchorMin.y = anchorMin.y + rect.position.y / Screen.height;
                    m_Panel.anchorMin = anchorMin;
                    Vector2 anchorMax = m_Panel.anchorMax;
                    anchorMax.y = anchorMax.y + rect.position.y / Screen.height;
                    m_Panel.anchorMax = anchorMax;
                }
            }

        }

        #endregion


#if !UNITY_EDITOR
        #region IN BUILD LOGIC
        void Start()
        {
            // Apply safe area at startup
            ApplySafeArea();
        }
        void Update()
        {
            // We constantly check if the safe area changed, for instance if the screen is rotated            
            ApplySafeAreaIfNeeded();
        }
        #endregion
#endif


#if UNITY_EDITOR && (UNITY_2019_4_OR_NEWER || UNITY_2020_1_OR_NEWER)
        #region DEVICE SIMULATOR COMPATIBILITY

        static int deviceSimulatorEnabled = -1;
        public static bool IsDeviceSimulatorEnabled()
        {
            if (deviceSimulatorEnabled < 0)
            {
                string manifestPath = Application.dataPath + "/../Packages/manifest.json";  
                if (!System.IO.File.Exists(manifestPath))
                {
                    // Debug.Log("Package manigest not found " + manifestPath);
                    deviceSimulatorEnabled = 0;
                }
                else
                {
                    string manifest = System.IO.File.ReadAllText(manifestPath);
                    if (manifest.Contains("device-simulator"))
                    {
                        // Debug.Log("device-simulator is enabled");
                        deviceSimulatorEnabled = 1;
                    }
                    else
                    {
                        // Debug.Log("device-simulator is disabled");
                        deviceSimulatorEnabled = 0;
                    }
                }
            }
            return deviceSimulatorEnabled == 1;
        }
        
        void Start()
        {
            // Apply safe area at startup
            ApplySafeArea();
        }

        void Update()
        {
            // We prevent to automatically update the safe area if a device simulation is in process to prevent any conflict
            if (m_IsSimulatingDevice)
            {
                return;
            }
            // Auto update only needed when Device Simulator is enabled 
            if (!IsDeviceSimulatorEnabled())
            {
                return;
            }
            ApplySafeAreaIfNeeded();
        }

        #endregion
#endif


#if UNITY_EDITOR
        #region SIMULATION LOGIC

        [System.NonSerialized]
        public bool m_IsSimulatingDevice = false;

        void OnEnable()
        {
            // Register to simulation events
            ScreenshotTaker.onResolutionUpdateStartDelegate += DeviceBeforeCapture;
            ScreenshotTaker.onResolutionUpdateEndDelegate += DeviceAfterCapture;
            ScreenshotTaker.onResolutionScreenResizedDelegate += DeviceResized;
        }

        void OnDisable()
        {
            ScreenshotTaker.onResolutionUpdateStartDelegate -= DeviceBeforeCapture;
            ScreenshotTaker.onResolutionUpdateEndDelegate -= DeviceAfterCapture;
            ScreenshotTaker.onResolutionScreenResizedDelegate -= DeviceResized;
        }

        void DeviceBeforeCapture(ScreenshotResolution device)
        {
        }

        void DeviceResized(ScreenshotResolution device)
        {
            // In live preview, we ignore the resize event for non device,
            // to prevent resizing to the device full preview with border 
            if (Application.isPlaying && device.m_ResolutionName == "" && DeviceInfo.m_IsLivePreview)
            {
                return;
            }

            m_IsSimulatingDevice = true;
            ApplySafeArea();
        }

        void DeviceAfterCapture(ScreenshotResolution device)
        {
            // We restore the safe area except if it is a live preview to prevent flickering
            if (DeviceInfo.m_IsLivePreview)
            {
                return;
            }
            Restore();
            m_IsSimulatingDevice = false;
        }

        #endregion
#endif


    }
}
