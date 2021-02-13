using UnityEngine;

using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using AlmostEngine.Screenshot;

namespace AlmostEngine.Preview
{
    public class ResolutionSettingsWindow : EditorWindow
    {
        [MenuItem("Window/Almost Engine/Universal Device Preview/Settings")]
        public static void Init()
        {
            m_Window = (ResolutionSettingsWindow)EditorWindow.GetWindow(typeof(ResolutionSettingsWindow), false, "Devices");
            m_Window.Show();
        }

        [InitializeOnLoadMethod]
        // Load all gameview size at startup
        public static void InitOnLoad()
        {
            m_ConfigAsset = GetConfig(false);
            if (m_ConfigAsset != null)
            {
                InitAllGameviewSizes();
            }
        }

        public static ResolutionSettingsWindow m_Window;

        public static bool IsOpen()
        {
            return m_Window != null;
        }
        static PreviewConfigAsset m_ConfigAsset;

        ScreenshotConfigDrawer m_ConfigDrawer;
        SerializedObject serializedObject;

        Vector2 m_ScrollPos;

        public static PreviewConfigAsset GetConfig(bool allowCreation = true)
        {
            PreviewConfigAsset asset = null;
            if (allowCreation)
            {
                asset = AssetUtils.GetOrCreate<PreviewConfigAsset>("Assets/AlmostEngine/UniversalDevicePreview/Assets/Editor/UniversalDevicePreviewConfig.asset", "UniversalDevicePreviewConfig", "Assets/");
            }
            else
            {
                asset = AssetUtils.GetFirst<PreviewConfigAsset>();
            }
            if (asset == null)
                return null;

            asset.m_Config.m_CaptureMode = ScreenshotTaker.CaptureMode.GAMEVIEW_RESIZING;
            asset.m_Config.m_CameraMode = ScreenshotConfig.CamerasMode.GAME_VIEW;
            asset.m_Config.m_ResolutionCaptureMode = ScreenshotConfig.ResolutionMode.CUSTOM_RESOLUTIONS;
            asset.m_Config.m_ShotMode = ScreenshotConfig.ShotMode.ONE_SHOT;
            asset.m_Config.m_StopTimeOnCapture = false;
            asset.m_Config.m_PlaySoundOnCapture = false;

            return asset;
        }

        void OnEnable()
        {
            if (m_ConfigAsset == null)
            {
                m_ConfigAsset = GetConfig();
            }
            serializedObject = new SerializedObject(m_ConfigAsset);
            m_ConfigDrawer = new ScreenshotConfigDrawer();
            m_ConfigDrawer.m_ShowDetailedDevice = true;
            m_ConfigDrawer.Init(serializedObject, m_ConfigAsset, m_ConfigAsset.m_Config, serializedObject.FindProperty("m_Config"));
            m_ConfigDrawer.m_Selector.m_ShowDetailedDevice = true;
        }

        #region GUI

        void OnGUI()
        {
            InitAllGameviewSizes();

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();


            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Gallery Preview", EditorStyles.toolbarButton))
            {
                ResolutionGalleryWindow.Init();
            }
            if (GUILayout.Button("Device Preview", EditorStyles.toolbarButton))
            {
                ResolutionPreviewWindow.Init();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("About", EditorStyles.toolbarButton))
            {
                UniversalDevicePreview.About();
            }
            EditorGUILayout.EndHorizontal();



            m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);

            EditorGUILayout.Separator();
            DrawConfig();
            EditorGUILayout.Separator();

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            DrawSupportGUI();


            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            DrawContactGUI();

            EditorGUILayout.EndScrollView();


            if (EditorGUI.EndChangeCheck())
            {
                ResolutionWindowBase.RepaintWindows();
            }

            serializedObject.ApplyModifiedProperties();
        }


        protected int m_WindowWidth;

        protected void DrawGallerySettings()
        {

            m_ConfigAsset.m_ShowGallery = EditorGUILayout.Foldout(m_ConfigAsset.m_ShowGallery, "Preview settings".ToUpper());
            if (m_ConfigAsset.m_ShowGallery == false)
                return;
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Display", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ScreenPPI"));

            EditorGUILayout.HelpBox("Set ScreenPPI to the PPI of your display to have correct physical sizes in PPI mode.", MessageType.Info);


            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Safe Area", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DrawSafeArea"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SafeAreaCanvas"));
            EditorGUILayout.HelpBox("Note that safe area is only displayed with devices containing safe area data. ", MessageType.Info);
            var col = GUI.color;
            GUI.color = new Color(0.6f, 1f, 0.6f, 1.0f);
            EditorGUILayout.HelpBox("Send me your safe are data! If you get more safe area values, please send them to me, so I can update the asset presets and make them available to all users."
            + " To get them, simply run the CanvasExamples scene on your target device, and look at the device info values.", MessageType.Info);
            GUI.color = col;


            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Drawing", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DrawingMode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TransparentDeviceBackground"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DeviceRendererCamera"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DefaultDeviceCanvas"));


            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Auto Refresh", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AutoRefresh"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RefreshMode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RefreshDelay"));

            EditorGUILayout.HelpBox("Autorefresh recompute all previews periodically using the refresh delay." +
            " Because the gameview needs to be resized during the preview capture, it is recommended to use autorefresh with only one or very few devices at a time, and with a high delay value." +
            " Prefer using the hotkey (F5) to refresh manually the previews when you need it." +
            " For a live preview, it is recommended to use only one device, and to set DrawingMode to texture only.",
            MessageType.Info);


            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Behavior", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AutoGenerateEmptyPreview"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BackupPreviewToDisk"));

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Gallery device names", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ShowResolution"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ShowPPI"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ShowRatio"));

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Misc.", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ZoomScrollSpeed"));

        }


        protected void DrawConfig()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            m_ConfigAsset.m_Config.m_ShowResolutions = EditorGUILayout.Foldout(m_ConfigAsset.m_Config.m_ShowResolutions, "Devices".ToUpper());

            if (m_ConfigAsset.m_ExpandDevices)
            {
                m_ConfigDrawer.m_Expanded = true;
                if (GUILayout.Button("Hide device settings"))
                {
                    m_ConfigAsset.m_ExpandDevices = false;
                    m_ConfigDrawer.m_Expanded = false;
                }
            }
            else
            {
                m_ConfigDrawer.m_Expanded = false;
                if (GUILayout.Button("Expand device settings "))
                {
                    m_ConfigAsset.m_ExpandDevices = true;
                    m_ConfigDrawer.m_Expanded = true;
                }
            }

            EditorGUILayout.Separator();

            m_ConfigDrawer.DrawResolutionContentGUI();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            DrawGallerySettings();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            m_ConfigDrawer.DrawFolderGUI();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            m_ConfigDrawer.DrawNameGUI();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            m_ConfigDrawer.DrawDelay(false);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();


            EditorGUILayout.BeginVertical(GUI.skin.box);
            m_ConfigAsset.m_Config.m_ShowUtils = EditorGUILayout.Foldout(m_ConfigAsset.m_Config.m_ShowUtils, "Hotkeys".ToUpper());
            if (m_ConfigAsset.m_Config.m_ShowUtils != false)
            {

                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Default hotkeys: Update preview (F5) Export preview (F6)");
                EditorGUILayout.HelpBox("You can customize the hotkeys by editing the UpdateDevicePreviewMenuItem.cs and ExportDevicePreviewMenuItem.cs scripts.", MessageType.Info);

            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            m_ConfigDrawer.DrawUsage();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();

        }

        public static void DrawSupportGUI()
        {

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("SUPPORT");

            EditorGUILayout.HelpBox("If you notice any mistake in the device resolutions, PPI values, or preview pictures, please let me know. " +
            "Also, you can contact me if you want a specific device to be added. " +
            "All suggestions are welcome.", MessageType.Info);

            Color cc = GUI.color;
            GUI.color = new Color(0.55f, 0.7f, 1f, 1.0f);
            if (GUILayout.Button("More assets from Wild Mage Games"))
            {
                Application.OpenURL("https://www.wildmagegames.com/unity/");
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Contact support"))
            {
                Application.OpenURL("mailto:support@wildmagegames.com");
            }
            GUI.color = new Color(0.6f, 1f, 0.6f, 1.0f);
            if (GUILayout.Button("Leave a Review"))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/utilities/universal-device-preview-82015");
            }
            GUI.color = cc;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();

        }


        protected void DrawContactGUI()
        {
            EditorGUILayout.LabelField(UniversalDevicePreview.VERSION, UIStyle.centeredGreyTextStyle);
            EditorGUILayout.LabelField(UniversalDevicePreview.AUTHOR, UIStyle.centeredGreyTextStyle);
        }

        #endregion

        static string m_GameviewSizeName = "UniversalDevicePreview - ";

        static void InitAllGameviewSizes()
        {
            // Remove all resolutions not in devices
            List<string> deviceSizes = new List<string>();
            foreach (ScreenshotResolution res in m_ConfigAsset.m_Config.GetActiveResolutions())
            {
                deviceSizes.Add(m_GameviewSizeName + res.ToString());
            }
            List<string> gameviewSizes = GameViewUtils.GetAllSizeNames();
            foreach (string size in gameviewSizes)
            {
                if (size.Contains(m_GameviewSizeName) && !deviceSizes.Contains(size))
                {
                    GameViewUtils.RemoveCustomSize(GameViewUtils.FindSize(size));
                }
            }

            // Add all new devices
            foreach (ScreenshotResolution res in m_ConfigAsset.m_Config.GetActiveResolutions())
            {
                if (!gameviewSizes.Contains(m_GameviewSizeName + res.ToString()))
                {
                    GameViewUtils.AddCustomSize(GameViewUtils.SizeType.FIXED_RESOLUTION, res.ComputeTargetWidth(), res.ComputeTargetHeight(), m_GameviewSizeName + res.ToString());
                }
            }

        }


    }
}