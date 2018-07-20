using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace opdemo.dll
{
    public class OP_API : MonoBehaviour
    {
        // Singleton
        public static OP_API instance { get; private set; }
        protected virtual void Awake() { instance = this; }

        // OP API functions
        //[DllImport("openpose")] public static extern int OP_TestFunction();
        [DllImport("openpose")] private static extern void OP_RegisterDebugCallback(DebugCallback callback);
        [DllImport("openpose")] private static extern void OP_RegisterOutputCallback(OutputCallback callback);
        [DllImport("openpose")] private static extern void OP_SetParameters(int argc, string[] argv);
        [DllImport("openpose")] private static extern void OP_Run();
        [DllImport("openpose")] private static extern void OP_Shutdown();

        #region Interface
        public void OPSetParameter(OPFlag param, string value = "")
        {
            if (param == OPFlag.NONE) return;
            if (parameters.ContainsKey(param))
            {
                parameters[param] = value;
            } else
            {
                parameters.Add(param, value);
            }
        }

        public void OPClearParameters()
        {
            parameters.Clear();
        }

        public void OPRun()
        {
            if (!isRunning)
            {
                // Register OP log callback
                OP_RegisterDebugCallback(new DebugCallback(OPLog));
                OP_RegisterOutputCallback(new OutputCallback(OPOutput));
                // Start OP thread
                opThread = new Thread(new ThreadStart(OPExecuteThread));
                opThread.Start();
            } else
            {
                Debug.Log("OP already started");
            }
        }

        public void OPShutdown()
        {
            OP_Shutdown();
        }

        /// <summary>
        /// Logging messages from OP, callback invoked from OP
        /// </summary>
        /// <param name="message"> The message in string </param>
        /// <param name="type"> 0: log, -1: error, 1: warning </param>
        protected virtual void OPLog(string message, int type = 0)
        {
            if (!instance.enableDebug) return;
            switch (type)
            {
                case 0: Debug.Log("OP_Log: " + message); break;
                case -1: Debug.LogError("OP_Error: " + message); break;
                case 1: Debug.LogWarning("OP_Warning: " + message); break;
            }
        }

        protected virtual void OPOutput(string output, byte[] imageData, int type = 0) 
        {
            Debug.Log("ok here" + imageData[0].ToString());
            //Debug.Log("OP_Output: " + output);
        }
        #endregion

        // Settings
        public bool enableDebug = true;

        // Regester type
        private delegate void DebugCallback(string message, int type);
        private delegate void OutputCallback(string output, byte[] imageData, int type);

        // Parameters
        private Dictionary<OPFlag, string> parameters = new Dictionary<OPFlag, string>();

        // Control
        private Thread opThread;
        private bool isRunning = false;

        private string[] GenerateArgs()
        {
            List<string> args = new List<string>();
            args.Add("program_name"); // First parameter always be the program name
            foreach (var p in parameters)
            {
                args.Add(OPConfig.ParamInfo[p.Key].flagName);
                if (p.Value != "")
                {
                    args.Add(p.Value);
                }
            }
            return args.ToArray();
        }

        // OP thread
        private void OPExecuteThread()
        {
            isRunning = true;
            if(enableDebug) Debug.Log("OP_Start");

            string[] args = GenerateArgs();
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
                OP_Shutdown();
            }

            if (enableDebug) Debug.Log("OP_End");
            isRunning = false;
        }

        // For safety
        private void OnDestroy()
        {
            OP_Shutdown();
        }
    }

    public enum OPFlag
    {
        NONE,
        LOGGING_LEVEL,
        DISABLE_MULTI_THREAD,
        PROFILE_SPEED,
        CAMERA,
        CAMERA_RESOLUTION,
        CAMERA_FPS,
        VIDEO,
        IMAGE_DIR,
        FLIR_CAMERA,
        FLIR_CAMERA_INDEX,
        IP_CAMERA,
        FRAME_FIRST,
        FRAME_LAST,
        FRAME_FLIP,
        FRAME_ROTATE,
        FRAMES_REPEAT,
        PROCESS_REAL_TIME,
        CAMERA_PARAMETER_FOLDER,
        FRAME_KEEP_DISTORTION,
        MODEL_FOLDER,
        OUTPUT_RESOLUTION,
        NUM_GPU,
        NUM_GPU_START,
        KEYPOINT_SCALE,
        NUMBER_PEOPLE_MAX,
        BODY_DISABLE,
        MODEL_POSE,
        NET_RESOLUTION,
        SCALE_NUMBER,
        SCALE_GAP,
        HEATMAPS_ADD_PARTS,
        HEATMAPS_ADD_BKG,
        HEATMAPS_ADD_PAFS,
        HEATMAPS_SCALE,
        PART_CANDIDATES,
        FACE,
        FACE_NET_RESOLUTION,
        HAND,
        HAND_NET_RESOLUTION,
        HAND_SCALE_NUMBER,
        HAND_SCALE_RANGE,
        HAND_TRACKING,
        _3D,
        _3D_MIN_VIEWS,
        _3D_VIEWS,
        IDENTIFICATION,
        TRACKING,
        IK_THREDS,
        PART_TO_SHOW,
        DISABLE_BLENDING,
        RENDER_THRESHOLD,
        RENDER_POSE,
        ALPHA_POSE,
        ALPHA_HEATMAP,
        FACE_RENDER_THRESHOLD,
        FACE_RENDER,
        FACE_ALPHA_POSE,
        FACE_ALPHA_HEATMAP,
        HAND_RENDER_THRESHOLD,
        HAND_RENDER,
        HAND_ALPHA_POSE,
        HAND_ALPHA_HEATMAP,
        FULLSCREEN,
        NO_GUI_VERBOSE,
        DISPLAY,
        WRITE_IMAGES,
        WRITE_IMAGES_FORMAT,
        WRITE_VIDEO,
        WRITE_JSON,
        WRITE_COCO_JSON,
        WRITE_COCO_FOOT_JSON,
        WRITE_HEATMAPS,
        WRITE_HEATMAPS_FORMAT,
        WRITE_KEYPOINT,
        WRITE_KEYPOINT_FORAMT
    }

    internal static class OPConfig
    {
        public static Dictionary<OPFlag, ParameterInfo> ParamInfo = SetParamInfo();
        private static Dictionary<OPFlag, ParameterInfo> SetParamInfo()
        {
            var tempParamInfo = new Dictionary<OPFlag, ParameterInfo>();

            #region Parameters
            // Debugging/Other
            tempParamInfo.Add(OPFlag.LOGGING_LEVEL, new ParameterInfo("--logging_level", "", "Debugging/Other"));
            tempParamInfo.Add(OPFlag.DISABLE_MULTI_THREAD, new ParameterInfo("--disable_multi_thread", "", "Debugging/Other"));
            tempParamInfo.Add(OPFlag.PROFILE_SPEED, new ParameterInfo("--profile_speed", "", "Debugging/Other"));

            // Producer
            tempParamInfo.Add(OPFlag.CAMERA, new ParameterInfo("--camera", "", "Producer"));
            tempParamInfo.Add(OPFlag.CAMERA_RESOLUTION, new ParameterInfo("--camera_resolution", "", "Producer"));
            tempParamInfo.Add(OPFlag.CAMERA_FPS, new ParameterInfo("--camera_fps", "", "Producer"));
            tempParamInfo.Add(OPFlag.VIDEO, new ParameterInfo("--video", "", "Producer"));
            tempParamInfo.Add(OPFlag.IMAGE_DIR, new ParameterInfo("--image_dir", "", "Producer"));
            tempParamInfo.Add(OPFlag.FLIR_CAMERA, new ParameterInfo("--flir_camera", "", "Producer"));
            tempParamInfo.Add(OPFlag.FLIR_CAMERA_INDEX, new ParameterInfo("--flir_camera_index", "", "Producer"));
            tempParamInfo.Add(OPFlag.IP_CAMERA, new ParameterInfo("--ip_camera", "", "Producer"));
            tempParamInfo.Add(OPFlag.FRAME_FIRST, new ParameterInfo("--frame_first", "", "Producer"));
            tempParamInfo.Add(OPFlag.FRAME_LAST, new ParameterInfo("--frame_last", "", "Producer"));
            tempParamInfo.Add(OPFlag.FRAME_FLIP, new ParameterInfo("--frame_flip", "", "Producer"));
            tempParamInfo.Add(OPFlag.FRAME_ROTATE, new ParameterInfo("--frame_rotate", "", "Producer"));
            tempParamInfo.Add(OPFlag.FRAMES_REPEAT, new ParameterInfo("--frames_repeat", "", "Producer"));
            tempParamInfo.Add(OPFlag.PROCESS_REAL_TIME, new ParameterInfo("--process_real_time", "", "Producer"));
            tempParamInfo.Add(OPFlag.CAMERA_PARAMETER_FOLDER, new ParameterInfo("--camera_parameter_folder", "", "Producer"));
            tempParamInfo.Add(OPFlag.FRAME_KEEP_DISTORTION, new ParameterInfo("--frame_keep_distortion", "", "Producer"));

            // OpenPose
            tempParamInfo.Add(OPFlag.MODEL_FOLDER, new ParameterInfo("--model_folder", "", "OpenPose"));
            tempParamInfo.Add(OPFlag.OUTPUT_RESOLUTION, new ParameterInfo("--output_resolution", "", "OpenPose"));
            tempParamInfo.Add(OPFlag.NUM_GPU, new ParameterInfo("--num_gpu", "", "OpenPose"));
            tempParamInfo.Add(OPFlag.NUM_GPU_START, new ParameterInfo("--num_gpu_start", "", "OpenPose"));
            tempParamInfo.Add(OPFlag.KEYPOINT_SCALE, new ParameterInfo("--keypoint_scale", "", "OpenPose"));
            tempParamInfo.Add(OPFlag.NUMBER_PEOPLE_MAX, new ParameterInfo("--number_people_max", "", "OpenPose"));

            // OpenPose Body Pose
            tempParamInfo.Add(OPFlag.BODY_DISABLE, new ParameterInfo("--body_disable", "", "OpenPose Body Pose"));
            tempParamInfo.Add(OPFlag.MODEL_POSE, new ParameterInfo("--model_pose", "", "OpenPose Body Pose"));
            tempParamInfo.Add(OPFlag.NET_RESOLUTION, new ParameterInfo("--net_resolution", "", "OpenPose Body Pose"));
            tempParamInfo.Add(OPFlag.SCALE_NUMBER, new ParameterInfo("--scale_number", "", "OpenPose Body Pose"));
            tempParamInfo.Add(OPFlag.SCALE_GAP, new ParameterInfo("--scale_gap", "", "OpenPose Body Pose"));

            // OpenPose Body Pose Heatmaps and Part Candidates
            tempParamInfo.Add(OPFlag.HEATMAPS_ADD_PARTS, new ParameterInfo("--heatmaps_add_parts", "", "OpenPose Body Pose Heatmaps and Part Candidates"));
            tempParamInfo.Add(OPFlag.HEATMAPS_ADD_BKG, new ParameterInfo("--heatmaps_add_bkg", "", "OpenPose Body Pose Heatmaps and Part Candidates"));
            tempParamInfo.Add(OPFlag.HEATMAPS_ADD_PAFS, new ParameterInfo("--heatmaps_add_PAFs", "", "OpenPose Body Pose Heatmaps and Part Candidates"));
            tempParamInfo.Add(OPFlag.HEATMAPS_SCALE, new ParameterInfo("--heatmaps_scale", "", "OpenPose Body Pose Heatmaps and Part Candidates"));
            tempParamInfo.Add(OPFlag.PART_CANDIDATES, new ParameterInfo("--part_candidates", "", "OpenPose Body Pose Heatmaps and Part Candidates"));

            // OpenPose Face
            tempParamInfo.Add(OPFlag.FACE, new ParameterInfo("--face", "", "OpenPose Face"));
            tempParamInfo.Add(OPFlag.FACE_NET_RESOLUTION, new ParameterInfo("--face_net_resolution", "", "OpenPose Face"));

            // OpenPose Hand
            tempParamInfo.Add(OPFlag.HAND, new ParameterInfo("--hand", "", "OpenPose Hand"));
            tempParamInfo.Add(OPFlag.HAND_NET_RESOLUTION, new ParameterInfo("--hand_net_resolution", "", "OpenPose Hand"));
            tempParamInfo.Add(OPFlag.HAND_SCALE_NUMBER, new ParameterInfo("--hand_scale_number", "", "OpenPose Hand"));
            tempParamInfo.Add(OPFlag.HAND_SCALE_RANGE, new ParameterInfo("--hand_scale_range", "", "OpenPose Hand"));
            tempParamInfo.Add(OPFlag.HAND_TRACKING, new ParameterInfo("--hand_tracking", "", "OpenPose Hand"));

            // OpenPose 3-D Reconstruction
            tempParamInfo.Add(OPFlag._3D, new ParameterInfo("--3d", "", "OpenPose 3-D Reconstruction"));
            tempParamInfo.Add(OPFlag._3D_MIN_VIEWS, new ParameterInfo("--3d_min_views", "", "OpenPose 3-D Reconstruction"));
            tempParamInfo.Add(OPFlag._3D_VIEWS, new ParameterInfo("--3d_views", "", "OpenPose 3-D Reconstruction"));

            // Extra algorithms
            tempParamInfo.Add(OPFlag.IDENTIFICATION, new ParameterInfo("--identification", "", "Extra algorithms"));
            tempParamInfo.Add(OPFlag.TRACKING, new ParameterInfo("--tracking", "", "Extra algorithms"));
            tempParamInfo.Add(OPFlag.IK_THREDS, new ParameterInfo("--ik_threds", "", "Extra algorithms"));

            // OpenPose Rendering
            tempParamInfo.Add(OPFlag.PART_TO_SHOW, new ParameterInfo("--part_to_show", "", "OpenPose Rendering"));
            tempParamInfo.Add(OPFlag.DISABLE_BLENDING, new ParameterInfo("--disable_blending", "", "OpenPose Rendering"));

            // OpenPose Rendering Pose
            tempParamInfo.Add(OPFlag.RENDER_THRESHOLD, new ParameterInfo("--render_threshold", "", "OpenPose Rendering Pose"));
            tempParamInfo.Add(OPFlag.RENDER_POSE, new ParameterInfo("--render_pose", "", "OpenPose Rendering Pose"));
            tempParamInfo.Add(OPFlag.ALPHA_POSE, new ParameterInfo("--alpha_pose", "", "OpenPose Rendering Pose"));
            tempParamInfo.Add(OPFlag.ALPHA_HEATMAP, new ParameterInfo("--alpha_heatmap", "", "OpenPose Rendering Pose"));

            // OpenPose Rendering Face
            tempParamInfo.Add(OPFlag.FACE_RENDER_THRESHOLD, new ParameterInfo("--face_render_threshold", "", "OpenPose Rendering Face"));
            tempParamInfo.Add(OPFlag.FACE_RENDER, new ParameterInfo("--face_render", "", "OpenPose Rendering Face"));
            tempParamInfo.Add(OPFlag.FACE_ALPHA_POSE, new ParameterInfo("--face_alpha_pose", "", "OpenPose Rendering Face"));
            tempParamInfo.Add(OPFlag.FACE_ALPHA_HEATMAP, new ParameterInfo("--face_alpha_heatmap", "", "OpenPose Rendering Face"));

            // OpenPose Rendering Hand
            tempParamInfo.Add(OPFlag.HAND_RENDER_THRESHOLD, new ParameterInfo("--hand_render_threshold", "", "OpenPose Rendering Hand"));
            tempParamInfo.Add(OPFlag.HAND_RENDER, new ParameterInfo("--hand_render", "", "OpenPose Rendering Hand"));
            tempParamInfo.Add(OPFlag.HAND_ALPHA_POSE, new ParameterInfo("--hand_alpha_pose", "", "OpenPose Rendering Hand"));
            tempParamInfo.Add(OPFlag.HAND_ALPHA_HEATMAP, new ParameterInfo("--hand_alpha_heatmap", "", "OpenPose Rendering Hand"));

            // Display
            tempParamInfo.Add(OPFlag.FULLSCREEN, new ParameterInfo("--fullscreen", "", "Display"));
            tempParamInfo.Add(OPFlag.NO_GUI_VERBOSE, new ParameterInfo("--no_gui_verbose", "", "Display"));
            tempParamInfo.Add(OPFlag.DISPLAY, new ParameterInfo("--display", "", "Display"));

            // Result Saving
            tempParamInfo.Add(OPFlag.WRITE_IMAGES, new ParameterInfo("--write_images", "", "Result Saving"));
            tempParamInfo.Add(OPFlag.WRITE_IMAGES_FORMAT, new ParameterInfo("--write_images_format", "", "Result Saving"));
            tempParamInfo.Add(OPFlag.WRITE_VIDEO, new ParameterInfo("--write_video", "", "Result Saving"));
            tempParamInfo.Add(OPFlag.WRITE_JSON, new ParameterInfo("--write_json", "", "Result Saving"));
            tempParamInfo.Add(OPFlag.WRITE_COCO_JSON, new ParameterInfo("--write_coco_json", "", "Result Saving"));
            tempParamInfo.Add(OPFlag.WRITE_COCO_FOOT_JSON, new ParameterInfo("--write_coco_foot_json", "", "Result Saving"));
            tempParamInfo.Add(OPFlag.WRITE_HEATMAPS, new ParameterInfo("--write_heatmaps", "", "Result Saving"));
            tempParamInfo.Add(OPFlag.WRITE_HEATMAPS_FORMAT, new ParameterInfo("--write_heatmaps_format", "", "Result Saving"));
            tempParamInfo.Add(OPFlag.WRITE_KEYPOINT, new ParameterInfo("--write_keypoint", "", "Result Saving"));
            tempParamInfo.Add(OPFlag.WRITE_KEYPOINT_FORAMT, new ParameterInfo("--write_keypoint_foramt", "", "Result Saving"));
            #endregion

            return tempParamInfo;
        }

        internal struct ParameterInfo
        {
            public string flagName { get; private set; }
            public string defaultValue { get; private set; }
            public string tag { get; private set; }

            public ParameterInfo(string flagName, string defaultValue = "", string tag = "")
            {
                this.flagName = flagName;
                this.defaultValue = defaultValue;
                this.tag = tag;
            }
        }
    }
}
