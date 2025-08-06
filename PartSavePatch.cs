using HarmonyLib;
using UnityEngine;
using SFS.Parts;
using SFS.Parts.Modules;
using System.Collections.Generic;

namespace BurnEditor
{
    // 补丁PartSave构造函数，在保存时写入自定义burns字段
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
                    
                    // 只有当angle和intensity不全为0时才写入burns字段
                    if (burnParams.BurnAngle != 0f || burnParams.BurnIntensity != 0f)
                    {
                        // 使用现有的BurnMark.BurnSave结构
                        __instance.burns = new BurnMark.BurnSave
                        {
                            angle = burnParams.BurnAngle,
                            intensity = burnParams.BurnIntensity,
                            x = 0.3f, // 默认值
                            top = "",
                            bottom = ""
                        };
                    }
                    else
                    {
                        // 如果angle和intensity全为0，则不写入burns字段
                    }
                }
            }
            catch (System.Exception)
            {
                
            }
        }
    }
} 