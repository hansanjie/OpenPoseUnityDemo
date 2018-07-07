using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace opdemo
{
    public class OPImporter : MonoBehaviour
    {
        // Singleton
        private static OPImporter _instance;
        public static OPImporter instance { get { if (_instance) return _instance; else { Debug.LogError("Instance not exist"); return null; } } private set { _instance = value; } }

        // OP API functions
        //[DllImport("openpose")] public static extern int OP_TestFunction();
        [DllImport("openpose")] private static extern void OP_RegisterDebugCallback(DebugCallback callback);
        [DllImport("openpose")] private static extern void OP_SetParameters(int argc, string[] argv);
        [DllImport("openpose")] private static extern void OP_Run();

        // Settings
        public bool importDll = true;
        public bool opLogEnabled = true;

        // Multithreading
        private Thread opThread;

        // OP debug 
        private delegate void DebugCallback(string message, int type);
        private static void OPLog(string s, int type = 0)
        {
            switch (type)
            {
                case 0: Debug.Log("OP_Log: " + s); break;
                case -1: Debug.LogError("OP_Error: " + s); break;
                case 1: Debug.LogWarning("OP_Warning: " + s); break;
            }
        }

        private void Awake()
        {
            instance = this;
            if (importDll)
            {
                // Register OP log callback
                OP_RegisterDebugCallback(new DebugCallback(OPLog));
                // Start OP thread
                opThread = new Thread(new ThreadStart(OPExecute));
                opThread.Start();
            }

        }

        private void OPExecute()
        {
            Debug.Log("OP_Start");

            //OPConfiguration.video.SetValue("C:/Users/tz1/Documents/OpenPoseUnityDemo/unitydemo/examples/media/video.avi");
            OPConfig.hand.SetValue("");

            string[] args = OPConfig.GenerateArgs();
            Debug.Log("OP_Params: " + string.Join(" ", args));

            try
            {
                OP_SetParameters(args.Length, args);
            } catch(Exception err)
            {
                Debug.LogError("OP_ParamSettingError: " + err.Message);
            }

            try
            {
                OP_Run();
            } catch(Exception err)
            {
                Debug.LogError("OP_RunError: " + err.Message);
            }

            Debug.Log("OP_End");
        }

        // Use this for initialization
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

    public static class OPConfig
    {
        // param list (for looping use)
        private static List<OPParameter> paramList = new List<OPParameter>();
        private static OPParameter AddToList(OPParameter p) { paramList.Add(p); return p; }

        // Parameters
        #region
        // Debugging/Other
        public static OPParameter logging_level = AddToList(new OPParameter("--logging_level", "Debugging/Other"));
        public static OPParameter disable_multi_thread = AddToList(new OPParameter("--disable_multi_thread", "Debugging/Other"));
        public static OPParameter profile_speed = AddToList(new OPParameter("--profile_speed", "Debugging/Other"));

        // Producer
        public static OPParameter camera = AddToList(new OPParameter("--camera", "Producer"));
        public static OPParameter camera_resolution = AddToList(new OPParameter("--camera_resolution", "Producer"));
        public static OPParameter camera_fps = AddToList(new OPParameter("--camera_fps", "Producer"));
        public static OPParameter video = AddToList(new OPParameter("--video", "Producer"));
        public static OPParameter image_dir = AddToList(new OPParameter("--image_dir", "Producer"));
        public static OPParameter flir_camera = AddToList(new OPParameter("--flir_camera", "Producer"));
        public static OPParameter flir_camera_index = AddToList(new OPParameter("--flir_camera_index", "Producer"));
        public static OPParameter ip_camera = AddToList(new OPParameter("--ip_camera", "Producer"));
        public static OPParameter frame_first = AddToList(new OPParameter("--frame_first", "Producer"));
        public static OPParameter frame_last = AddToList(new OPParameter("--frame_last", "Producer"));
        public static OPParameter frame_flip = AddToList(new OPParameter("--frame_flip", "Producer"));
        public static OPParameter frame_rotate = AddToList(new OPParameter("--frame_rotate", "Producer"));
        public static OPParameter frames_repeat = AddToList(new OPParameter("--frames_repeat", "Producer"));
        public static OPParameter process_real_time = AddToList(new OPParameter("--process_real_time", "Producer"));
        public static OPParameter camera_parameter_folder = AddToList(new OPParameter("--camera_parameter_folder", "Producer"));
        public static OPParameter frame_keep_distortion = AddToList(new OPParameter("--frame_keep_distortion", "Producer"));

        // OpenPose
        public static OPParameter model_folder = AddToList(new OPParameter("--model_folder", "OpenPose"));
        public static OPParameter output_resolution = AddToList(new OPParameter("--output_resolution", "OpenPose"));
        public static OPParameter num_gpu = AddToList(new OPParameter("--num_gpu", "OpenPose"));
        public static OPParameter num_gpu_start = AddToList(new OPParameter("--num_gpu_start", "OpenPose"));
        public static OPParameter keypoint_scale = AddToList(new OPParameter("--keypoint_scale", "OpenPose"));
        public static OPParameter number_people_max = AddToList(new OPParameter("--number_people_max", "OpenPose"));

        // OpenPose Body Pose
        public static OPParameter body_disable = AddToList(new OPParameter("--body_disable", "OpenPose Body Pose"));
        public static OPParameter model_pose = AddToList(new OPParameter("--model_pose", "OpenPose Body Pose"));
        public static OPParameter net_resolution = AddToList(new OPParameter("--net_resolution", "OpenPose Body Pose"));
        public static OPParameter scale_number = AddToList(new OPParameter("--scale_number", "OpenPose Body Pose"));
        public static OPParameter scale_gap = AddToList(new OPParameter("--scale_gap", "OpenPose Body Pose"));

        // OpenPose Body Pose Heatmaps and Part Candidates
        public static OPParameter heatmaps_add_parts = AddToList(new OPParameter("--heatmaps_add_parts", "OpenPose Body Pose Heatmaps and Part Candidates"));
        public static OPParameter heatmaps_add_bkg = AddToList(new OPParameter("--heatmaps_add_bkg", "OpenPose Body Pose Heatmaps and Part Candidates"));
        public static OPParameter heatmaps_add_PAFs = AddToList(new OPParameter("--heatmaps_add_PAFs", "OpenPose Body Pose Heatmaps and Part Candidates"));
        public static OPParameter heatmaps_scale = AddToList(new OPParameter("--heatmaps_scale", "OpenPose Body Pose Heatmaps and Part Candidates"));
        public static OPParameter part_candidates = AddToList(new OPParameter("--part_candidates", "OpenPose Body Pose Heatmaps and Part Candidates"));

        // OpenPose Face
        public static OPParameter face = AddToList(new OPParameter("--face", "OpenPose Face"));
        public static OPParameter face_net_resolution = AddToList(new OPParameter("--face_net_resolution", "OpenPose Face"));

        // OpenPose Hand
        public static OPParameter hand = AddToList(new OPParameter("--hand", "OpenPose Hand"));
        public static OPParameter hand_net_resolution = AddToList(new OPParameter("--hand_net_resolution", "OpenPose Hand"));
        public static OPParameter hand_scale_number = AddToList(new OPParameter("--hand_scale_number", "OpenPose Hand"));
        public static OPParameter hand_scale_range = AddToList(new OPParameter("--hand_scale_range", "OpenPose Hand"));
        public static OPParameter hand_tracking = AddToList(new OPParameter("--hand_tracking", "OpenPose Hand"));

        // OpenPose 3-D Reconstruction
        public static OPParameter _3d = AddToList(new OPParameter("--3d", "OpenPose 3-D Reconstruction"));
        public static OPParameter _3d_min_views = AddToList(new OPParameter("--3d_min_views", "OpenPose 3-D Reconstruction"));
        public static OPParameter _3d_views = AddToList(new OPParameter("--3d_views", "OpenPose 3-D Reconstruction"));

        // Extra algorithms
        public static OPParameter identification = AddToList(new OPParameter("--identification", "Extra algorithms"));
        public static OPParameter tracking = AddToList(new OPParameter("--tracking", "Extra algorithms"));
        public static OPParameter ik_threds = AddToList(new OPParameter("--ik_threds", "Extra algorithms"));

        // OpenPose Rendering
        public static OPParameter part_to_show = AddToList(new OPParameter("--part_to_show", "OpenPose Rendering"));
        public static OPParameter disable_blending = AddToList(new OPParameter("--disable_blending", "OpenPose Rendering"));

        // OpenPose Rendering Pose
        public static OPParameter render_threshold = AddToList(new OPParameter("--render_threshold", "OpenPose Rendering Pose"));
        public static OPParameter render_pose = AddToList(new OPParameter("--render_pose", "OpenPose Rendering Pose"));
        public static OPParameter alpha_pose = AddToList(new OPParameter("--alpha_pose", "OpenPose Rendering Pose"));
        public static OPParameter alpha_heatmap = AddToList(new OPParameter("--alpha_heatmap", "OpenPose Rendering Pose"));

        // OpenPose Rendering Face
        public static OPParameter face_render_threshold = AddToList(new OPParameter("--face_render_threshold", "OpenPose Rendering Face"));
        public static OPParameter face_render = AddToList(new OPParameter("--face_render", "OpenPose Rendering Face"));
        public static OPParameter face_alpha_pose = AddToList(new OPParameter("--face_alpha_pose", "OpenPose Rendering Face"));
        public static OPParameter face_alpha_heatmap = AddToList(new OPParameter("--face_alpha_heatmap", "OpenPose Rendering Face"));

        // OpenPose Rendering Hand
        public static OPParameter hand_render_threshold = AddToList(new OPParameter("--hand_render_threshold", "OpenPose Rendering Hand"));
        public static OPParameter hand_render = AddToList(new OPParameter("--hand_render", "OpenPose Rendering Hand"));
        public static OPParameter hand_alpha_pose = AddToList(new OPParameter("--hand_alpha_pose", "OpenPose Rendering Hand"));
        public static OPParameter hand_alpha_heatmap = AddToList(new OPParameter("--hand_alpha_heatmap", "OpenPose Rendering Hand"));

        // Display
        public static OPParameter fullscreen = AddToList(new OPParameter("--fullscreen", "Display"));
        public static OPParameter no_gui_verbose = AddToList(new OPParameter("--no_gui_verbose", "Display"));
        public static OPParameter display = AddToList(new OPParameter("--display", "Display"));

        // Result Saving
        public static OPParameter write_images = AddToList(new OPParameter("--write_images", "Result Saving"));
        public static OPParameter write_images_format = AddToList(new OPParameter("--write_images_format", "Result Saving"));
        public static OPParameter write_video = AddToList(new OPParameter("--write_video", "Result Saving"));
        public static OPParameter write_json = AddToList(new OPParameter("--write_json", "Result Saving"));
        public static OPParameter write_coco_json = AddToList(new OPParameter("--write_coco_json", "Result Saving"));
        public static OPParameter write_coco_foot_json = AddToList(new OPParameter("--write_coco_foot_json", "Result Saving"));
        public static OPParameter write_heatmaps = AddToList(new OPParameter("--write_heatmaps", "Result Saving"));
        public static OPParameter write_heatmaps_format = AddToList(new OPParameter("--write_heatmaps_format", "Result Saving"));
        public static OPParameter write_keypoint = AddToList(new OPParameter("--write_keypoint", "Result Saving"));
        public static OPParameter write_keypoint_foramt = AddToList(new OPParameter("--write_keypoint_foramt", "Result Saving"));
        #endregion
        
        public static string[] GenerateArgs()
        {
            List<string> args = new List<string>();
            args.Add("program_name"); // First parameter always be the program name
            foreach (OPParameter p in paramList)
            {
                if (p.usingDefault)
                {
                    
                } else
                {
                    args.Add(p.flagName);
                    if (p.value != "")
                    {
                        args.Add(p.value);
                    }
                }
            }
            return args.ToArray();
        }
    }

    public class OPParameter
    {
        public OPParameter(string flagName, string tag = "")
        {
            this.flagName = flagName;
            this.tag = tag;

            this.value = "";
            this.usingDefault = true;
        }

        public void SetValue(string value)
        {
            this.value = value;
            this.usingDefault = false;
        }

        public void UseDefault()
        {
            this.usingDefault = true;
        }

        public string value { get; private set; } // value to be set in string
        public bool usingDefault { get; private set; } // no flag will be set if true
        public string flagName { get; private set; }
        public string tag { get; private set; }
    }
}
