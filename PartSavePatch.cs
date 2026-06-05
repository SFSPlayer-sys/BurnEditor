using HarmonyLib;
using UnityEngine;
using SFS.Parts;
using SFS.Parts.Modules;
using System.Collections.Generic;

namespace BurnEditor
{
    [HarmonyPatch(typeof(PartSave))]
    public class PartSavePatch
    {
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(new System.Type[] { typeof(Part) })]
        [HarmonyPostfix]
        public static void PartSave_Constructor_Postfix(PartSave __instance, Part part)
        {
            try
            {
                // 检查是否有BurnParameters
                if (PartStatsPatch.partBurnParams.ContainsKey(part))
                {
                    var burnParams = PartStatsPatch.partBurnParams[part];
                    
                    bool hasMeaningfulData =
                        burnParams.BurnAngle != 0f ||
                        burnParams.BurnIntensity != 0f ||
                        Mathf.Abs(burnParams.X - 0.3f) > 0.0001f ||
                        !string.IsNullOrEmpty(burnParams.Top) ||
                        !string.IsNullOrEmpty(burnParams.Bottom);
                    if (hasMeaningfulData)
                    {
                        // 使用现有的BurnMark.BurnSave结构
                        __instance.burns = new BurnMark.BurnSave
                        {
                            angle = burnParams.BurnAngle,
                            intensity = burnParams.BurnIntensity,
                            x = burnParams.X,
                            top = burnParams.Top ?? string.Empty,
                            bottom = burnParams.Bottom ?? string.Empty
                        };
                    }
                    else
                    {
                        __instance.burns = null;
                    }
                }
            }
            catch (System.Exception)
            {
                
            }
        }
    }
} 