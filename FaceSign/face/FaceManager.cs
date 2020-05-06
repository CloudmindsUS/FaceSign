using FaceSign.data;
using FaceSign.log;
using FaceSign.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FaceSign
{
    class FaceLibWrapper
    {

        [DllImport("faceRecogInterface.dll", CallingConvention = CallingConvention.Cdecl,EntryPoint = "init_arcface")]
        public extern static IntPtr init_arcface(char[] model_path, char[] model_weight);

        [DllImport("faceRecogInterface.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "init_arcface")]
        public extern static IntPtr init_arcface(string model_path, string model_weight);

        [DllImport("face_wrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr init_arcface_net2(char[] model_path, char[] model_weight);

        [DllImport("faceRecogInterface.dll", CallingConvention = CallingConvention.Cdecl,EntryPoint = "init_mtcnn")]
        public extern static IntPtr init_mtcnn(char[] model_dir);

        [DllImport("faceRecogInterface.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "init_mtcnn")]
        public extern static IntPtr init_mtcnn(string model_dir);

        [DllImport("faceRecogInterface.dll", CallingConvention = CallingConvention.Cdecl,EntryPoint = "get_face_feature")]
        public extern static IntPtr get_face_feature(IntPtr mtcnnPtr,IntPtr arcPtr,char[] image_path,bool all_resize);

        [DllImport("faceRecogInterface.dll", CallingConvention = CallingConvention.Cdecl,EntryPoint = "get_face_smilarity")]
        public extern static float get_face_smilarity(float[] feature1,float[] feature2);
    }

    public class FaceManager {
        private string tag = "FaceManager";
        private static readonly object LockObj = new object();
        public static FaceManager Instance = new FaceManager();
        public const float FaceConfidence = 0.5f;
        private string model_path = $@"{FileUtil.GetAppRootPath()}\model\arcface_deploy-50.prototxt";
        private string model_weight = $@"{FileUtil.GetAppRootPath()}\model\arcface_model-50.caffemodel";
        private string model_dir = $@"{FileUtil.GetAppRootPath()}\model";
        IntPtr Mt_CNN_Ptr;
        IntPtr Arc_Face_Ptr;


        public void Init() {
            if (!BuildConfig.Debug) {
                Arc_Face_Ptr = FaceLibWrapper.init_arcface(model_path, model_weight);
                Log.I(tag, "init_arcface:"+Arc_Face_Ptr);
                Mt_CNN_Ptr = FaceLibWrapper.init_mtcnn(model_dir);
                Log.I(tag, "init_mtcnn:" + Mt_CNN_Ptr);
            }
        }

        public float[] GetFaceFeature(string path) {
            return GetFaceFeature(path,false);
        }

        public float[] GetFaceFeature(string path,bool resize)
        {
            float[] feature = new float[512];
            if (!BuildConfig.Debug)
            {
                try
                {
                    IntPtr featurePtr = FaceLibWrapper.get_face_feature(Mt_CNN_Ptr, Arc_Face_Ptr, path.ToCharArray(), resize);
                    if (featurePtr != IntPtr.Zero)
                    {
                        Marshal.Copy(featurePtr, feature, 0, feature.Length);
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception e) {
                    Log.I("parse feature fail:"+e.Message);
                    return feature;
                }
            }
            return feature;
        }

        public float GetFaceSmiliaty(float[] feature1, float[] feature2) {
            if (!BuildConfig.Debug)
            {
                return FaceLibWrapper.get_face_smilarity(feature1, feature2);
            }
            else
            {
                return 1.0f;
            }
        }
    }
}
