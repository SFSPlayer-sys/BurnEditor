using HarmonyLib;
using UnityEngine;
using SFS.Parts;
using SFS.Parts.Modules;
using SFS.UI;
using SFS.Input;

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
            if (!settings.build)
            {
                return;
            }

            try
            {
                Part displayPart = __instance;
                if (allParts != null && allParts.Length > 0)
                {
                    displayPart = allParts[0];
                    foreach (var p in allParts)
                    {
                        if (p.GetInstanceID() < displayPart.GetInstanceID())
                            displayPart = p;
                    }
                }
                //刷新参数
                if (!partBurnParams.ContainsKey(displayPart))
                    partBurnParams[displayPart] = new BurnParameters();
                LoadSavedBurnData(displayPart, partBurnParams[displayPart]);
                var burnParams = partBurnParams[displayPart];

                // 绘制燃烧角度滑块
                drawer.DrawSlider(
                    -500, 
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
                    -501, 
                    () => $"Burn Intensity: {burnParams.BurnIntensity:F2}",
                    () => "0.00 - 1.50",
                    () => burnParams.BurnIntensity / 1.5f,
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

                // 绘制 X 滑条（0.00 - 1.00）
                drawer.DrawSlider(
                    -502,
                    () => $"Burn X: {burnParams.X:F2}",
                    () => "0.00 - 2.00",
                    () => Mathf.InverseLerp(0f, 2f, burnParams.X),
                    (value, fromInput) => {
                        float newX = Mathf.Round(Mathf.Lerp(0f, 2f, value) / 0.01f) * 0.01f;
                        newX = Mathf.Clamp(newX, 0f, 2f);
                        foreach (var part in allParts) {
                            if (!partBurnParams.ContainsKey(part))
                                partBurnParams[part] = new BurnParameters();
                            partBurnParams[part].X = newX;
                            ApplyBurnEffect(part, partBurnParams[part]);
                        }
                    },
                    (updateAction) => { burnParams.OnXChanged += updateAction; },
                    (updateAction) => { burnParams.OnXChanged -= updateAction; },
                    false
                );

                // 显示并编辑 Top 文本
                drawer.DrawButton(
                    -503,
                    () => $"Burn Top: {(string.IsNullOrEmpty(burnParams.Top) ? "<empty>" : burnParams.Top)}",
                    () => "Edit",
                    () => {
                        try
                        {
                            Menu.textInput.Open(
                                "Cancel",
                                "OK",
                                (values) => {
                                    if (values != null && values.Length > 0)
                                    {
                                        string newTop = values[0] ?? string.Empty;
                                        foreach (var part in allParts) {
                                            if (!partBurnParams.ContainsKey(part))
                                                partBurnParams[part] = new BurnParameters();
                                            // 更新
                                            partBurnParams[part].Top = newTop;
                                            // 应用效果
                                            ApplyBurnEffect(part, partBurnParams[part]);
                                        }
                                    }
                                    else
                                    {
                                        // 取消输入
                                        foreach (var part in allParts) {
                                            if (!partBurnParams.ContainsKey(part))
                                                partBurnParams[part] = new BurnParameters();
                                            partBurnParams[part].Top = string.Empty;
                                            ApplyBurnEffect(part, partBurnParams[part]);
                                        }
                                    }
                                },
                                CloseMode.Stack,
                                TextInputMenu.Element("Top (text)", burnParams.Top ?? string.Empty)
                            );
                        }
                        catch { }
                    },
                    () => true,
                    (updateAction) => { burnParams.OnTopChanged += updateAction; },
                    (updateAction) => { burnParams.OnTopChanged -= updateAction; }
                );

                // 显示并编辑 Bottom 文本
                drawer.DrawButton(
                    -504,
                    () => $"Burn Bottom: {(string.IsNullOrEmpty(burnParams.Bottom) ? "<empty>" : burnParams.Bottom)}",
                    () => "Edit",
                    () => {
                        try
                        {
                            Menu.textInput.Open(
                                "Cancel",
                                "OK",
                                (values) => {
                                    if (values != null && values.Length > 0)
                                    {
                                        string newBottom = values[0] ?? string.Empty;
                                        foreach (var part in allParts) {
                                            if (!partBurnParams.ContainsKey(part))
                                                partBurnParams[part] = new BurnParameters();
                                            // 更新
                                            partBurnParams[part].Bottom = newBottom;
                                            // 立即应用效果
                                            ApplyBurnEffect(part, partBurnParams[part]);
                                        }
                                    }
                                    else
                                    {
                                        // 取消输入
                                        foreach (var part in allParts) {
                                            if (!partBurnParams.ContainsKey(part))
                                                partBurnParams[part] = new BurnParameters();
                                            partBurnParams[part].Bottom = string.Empty;
                                            ApplyBurnEffect(part, partBurnParams[part]);
                                        }
                                    }
                                },
                                CloseMode.Stack,
                                TextInputMenu.Element("Bottom (text)", burnParams.Bottom ?? string.Empty)
                            );
                        }
                        catch { }
                    },
                    () => true,
                    (updateAction) => { burnParams.OnBottomChanged += updateAction; },
                    (updateAction) => { burnParams.OnBottomChanged -= updateAction; }
                );
            }
            catch (System.Exception)
            {
            }
        }
                // 加载保存的燃烧数据
        private static void LoadSavedBurnData(Part part, BurnParameters burnParams)
        {
            try
            {
                // 检查部件是否有BurnMark组件
                if (part.burnMark != null && part.burnMark.burn != null)
                {
                    // 读取燃烧数据
                    burnParams.BurnAngle = part.burnMark.burn.angle;
                    burnParams.BurnIntensity = part.burnMark.burn.intensity;
                    burnParams.X = part.burnMark.burn.x;
                    burnParams.Top = EncodeSurfaces(part.burnMark.burn.topSurfaces);
                    burnParams.Bottom = EncodeSurfaces(part.burnMark.burn.bottomSurfaces);
                    
                    // 应用效果
                    ApplyBurnEffect(part, burnParams);
                }
                else
                {
                    // 如果没有燃烧数据，重置参数
                    burnParams.BurnAngle = 0f;
                    burnParams.BurnIntensity = 0f;
                    burnParams.X = 0.3f;
                    burnParams.Top = string.Empty;
                    burnParams.Bottom = string.Empty;
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
                // 当angle和intensity不为0时应用燃烧效果
                if (burnParams.BurnAngle == 0f && burnParams.BurnIntensity == 0f)
                {
                    // 清除燃烧效果
                    if (part.burnMark != null)
                    {
                        part.burnMark.SetOpacity(0f, true);
                        // 清除燃烧数据
                        part.burnMark.burn = null;
                    }
                    return;
                }
                
                // 确保部件有BurnMark组件
                if (part.burnMark == null)
                {
                    part.burnMark = part.gameObject.AddComponent<SFS.Parts.Modules.BurnMark>();
                    part.burnMark.Initialize();
                }
                
                // 计算燃烧方向
                float angleRad = burnParams.BurnAngle * Mathf.Deg2Rad;
                Vector2 burnDirection = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    
                var save = new BurnMark.BurnSave
                {
                    angle = burnParams.BurnAngle,
                    intensity = burnParams.BurnIntensity,
                    x = burnParams.X,
                    top = NormalizeSurfaceString(burnParams.Top),
                    bottom = NormalizeSurfaceString(burnParams.Bottom)
                };
                part.burnMark.burn = save.FromSave();
                part.burnMark.SetOpacity(1f, true);
                part.burnMark.ApplyEverything();
            }
            catch (System.Exception)
            {

            }
        }
        private static string EncodeSurfaces(global::Line2[] surfaces)
        {
            if (surfaces == null || surfaces.Length == 0) return string.Empty;
            System.Text.StringBuilder sb = new System.Text.StringBuilder(surfaces.Length * 16);
            for (int i = 0; i < surfaces.Length; i++)
            {
                var line = surfaces[i];
                sb.Append(Mathf.RoundToInt(line.start.x * 100f)).Append(',');
                sb.Append(Mathf.RoundToInt(line.start.y * 100f)).Append(',');
                sb.Append(Mathf.RoundToInt(line.end.x * 100f)).Append(',');
                sb.Append(Mathf.RoundToInt(line.end.y * 100f)).Append(',');
            }
            return sb.ToString();
        }
        private static string NormalizeSurfaceString(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            return text.Replace("\n", string.Empty).Replace("\r", string.Empty).Trim();
        }
    }

    // 燃烧参数数据类
    public class BurnParameters
    {
        private float burnAngle = 0f;
        private float burnIntensity = 0f;
        private float x = 0.3f;
        private string top = "";
        private string bottom = "";
        
        public System.Action OnBurnAngleChanged;
        public System.Action OnBurnIntensityChanged;
        public System.Action OnXChanged;
        public System.Action OnTopChanged;
        public System.Action OnBottomChanged;
        
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

        public float X
        {
            get => x;
            set
            {
                if (x != value)
                {
                    x = value;
                    OnXChanged?.Invoke();
                }
            }
        }

        public string Top
        {
            get => top;
            set
            {
                string newValue = value ?? string.Empty;
                if (top != newValue)
                {
                    top = newValue;
                    OnTopChanged?.Invoke();
                }
                else
                {
                    OnTopChanged?.Invoke();
                }
            }
        }

        public string Bottom
        {
            get => bottom;
            set
            {
                string newValue = value ?? string.Empty;
                if (bottom != newValue)
                {
                    bottom = newValue;
                    OnBottomChanged?.Invoke();
                }
                else
                {
                    OnBottomChanged?.Invoke();
                }
            }
        }
    }
}