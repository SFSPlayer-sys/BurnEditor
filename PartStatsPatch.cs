using HarmonyLib;
using UnityEngine;
using SFS.Parts;
using SFS.Parts.Modules;
using SFS.UI;

namespace BurnEditor
{
    //烧灼数值滑块
    [HarmonyPatch(typeof(Part))]
    public class PartStatsPatch
    {
        // 存储部件的燃烧参数
        public static System.Collections.Generic.Dictionary<Part, BurnParameters> partBurnParams = 
            new System.Collections.Generic.Dictionary<Part, BurnParameters>();
            


        [HarmonyPatch("DrawPartStats")]
        [HarmonyPostfix]
        public static void DrawPartStats_Postfix(Part __instance, Part[] allParts, StatsMenu drawer, PartDrawSettings settings)
        {
                                    // 只在构建模式下显示燃烧参数
                        if (!settings.build)
                        {
                            return;
                        }

            try
            {
                                        // 获取或创建部件的燃烧参数
                        if (!partBurnParams.ContainsKey(__instance))
                        {
                            partBurnParams[__instance] = new BurnParameters();
                            
                            // 检查是否有保存的burn数据需要加载
                            LoadSavedBurnData(__instance, partBurnParams[__instance]);
                        }

                        var burnParams = partBurnParams[__instance];

                // 绘制燃烧角度滑块
                drawer.DrawSlider(
                    -500, // 较低优先级，显示在底部
                    () => $"Burn Angle: {burnParams.BurnAngle:F0}°",
                    () => "0° - 360°",
                    () => burnParams.BurnAngle / 360f, // 转换为0-1范围
                    (value, fromInput) => {
                        float newAngle = Mathf.Round((value * 360f) / 15f) * 15f;
                        foreach (var part in allParts) {
                            if (!partBurnParams.ContainsKey(part))
                                partBurnParams[part] = new BurnParameters();
                            partBurnParams[part].BurnAngle = newAngle;
                            ApplyBurnEffect(part, partBurnParams[part]);
                        }
                    },
                    (updateAction) => {
                        burnParams.OnBurnAngleChanged += updateAction;
                    },
                    (updateAction) => {
                        burnParams.OnBurnAngleChanged -= updateAction;
                    },
                    false // startUndoStep参数
                );

                // 绘制燃烧强度滑块
                drawer.DrawSlider(
                    -501, // 更低优先级，显示在燃烧角度下方
                    () => $"Burn Intensity: {burnParams.BurnIntensity:F2}",
                    () => "0.00 - 1.50",
                    () => burnParams.BurnIntensity / 1.5f, // 转换为0-1范围
                    (value, fromInput) => {
                        float newIntensity = Mathf.Round((value * 1.5f) / 0.05f) * 0.05f;
                        foreach (var part in allParts) {
                            if (!partBurnParams.ContainsKey(part))
                                partBurnParams[part] = new BurnParameters();
                            partBurnParams[part].BurnIntensity = newIntensity;
                            ApplyBurnEffect(part, partBurnParams[part]);
                        }
                    },
                    (updateAction) => {
                        burnParams.OnBurnIntensityChanged += updateAction;
                    },
                    (updateAction) => {
                        burnParams.OnBurnIntensityChanged -= updateAction;
                    },
                    false // startUndoStep参数
                );
            }
            catch (System.Exception)
            {
            
            }
        }
        
                // 加载保存的burn数据
        private static void LoadSavedBurnData(Part part, BurnParameters burnParams)
        {
            try
            {
                // 检查部件是否有BurnMark组件
                if (part.burnMark != null && part.burnMark.burn != null)
                {
                    // 直接读取burn数据
                    burnParams.BurnAngle = part.burnMark.burn.angle;
                    burnParams.BurnIntensity = part.burnMark.burn.intensity;
                    
                    // 应用效果
                    ApplyBurnEffect(part, burnParams);
                }
            }
            catch (System.Exception)
            {
                
            }
        }
        
        // 应用燃烧效果到部件
        private static void ApplyBurnEffect(Part part, BurnParameters burnParams)
        {
            try
            {
                // 只有当angle和intensity不全为0时才应用效果
                if (burnParams.BurnAngle == 0f && burnParams.BurnIntensity == 0f)
                {
                    // 清除燃烧效果
                    if (part.burnMark != null)
                    {
                        part.burnMark.SetOpacity(0f, true);
                    }
                    return;
                }
                
                // 确保部件有BurnMark组件
                if (part.burnMark == null)
                {
                    part.burnMark = part.gameObject.AddComponent<SFS.Parts.Modules.BurnMark>();
                    part.burnMark.Initialize();
                }
                
                // 计算燃烧方向（基于角度）
                float angleRad = burnParams.BurnAngle * Mathf.Deg2Rad;
                Vector2 burnDirection = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
                
                // 应用燃烧效果
                part.burnMark.SetBurn(
                    burnDirection,           // 燃烧方向
                    part.transform,          // 位置上下文
                    burnParams.BurnIntensity, // 燃烧强度
                    new global::Line2[0],   // 顶部表面（空数组）
                    new global::Line2[0],   // 底部表面（空数组）
                    1f                      // 不透明度
                );
                
                part.burnMark.ApplyEverything();
            }
            catch (System.Exception)
            {

            }
        }
    }

    // 燃烧参数数据类
    public class BurnParameters
    {
        private float burnAngle = 0f;
        private float burnIntensity = 0f;
        
        // 事件系统，用于UI更新
        public System.Action OnBurnAngleChanged;
        public System.Action OnBurnIntensityChanged;
        
        public float BurnAngle 
        { 
            get => burnAngle;
            set 
            {
                if (burnAngle != value)
                {
                    burnAngle = value;
                    OnBurnAngleChanged?.Invoke();
                }
            }
        }
        
        public float BurnIntensity 
        { 
            get => burnIntensity;
            set 
            {
                if (burnIntensity != value)
                {
                    burnIntensity = value;
                    OnBurnIntensityChanged?.Invoke();
                }
            }
        }
    }
}