﻿using DMT;
using Harmony;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SphereII_RandomSize
{
    private static string AdvFeatureClass = "AdvancedZombieFeatures";
    private static string Feature = "RandomSize";

    public static class RandomSizeHelper
    {
        public static bool AllowedRandomSize(EntityAlive entity)
        {
            bool bRandomSize = false;

            if (entity is EntityZombie)
            {
                AdvLogging.DisplayLog(AdvFeatureClass, " Random Size: Is A Zombie. Random size is true");
                bRandomSize = true;
            }
            EntityClass entityClass = EntityClass.list[entity.entityClass];
            if (entityClass.Properties.Values.ContainsKey("RandomSize"))
                bRandomSize = StringParsers.ParseBool(entityClass.Properties.Values["RandomSize"], 0, -1, true);

            AdvLogging.DisplayLog(AdvFeatureClass, "Entity: " + entity.DebugName + " Random Size:  " + bRandomSize);
            return bRandomSize;
        }
    }

    // <property name="RandomSizes" value="1.2,1.2,1.4" />
    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("CopyPropertiesFromEntityClass")]
    public class SphereII_EntityAlive_CopyPropertiesFromEntityClass
    {

        public static void Postfix(ref EntityAlive __instance)
        {
            // Check if this feature is enabled.
            if (Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
            {
                if (__instance is EntityPlayerLocal)
                    return;

                if (RandomSizeHelper.AllowedRandomSize(__instance))
                {
                    // This is the distributed random heigh multiplier. Add or adjust values as you see fit. By default, it's just a small adjustment.
                    float[] numbers = new float[9] { 0.7f, 0.8f, 0.9f, 0.9f, 1.0f, 1.0f, 1.0f, 1.1f, 1.2f };
                    System.Random random = new System.Random();

                    int randomIndex = random.Next(0, numbers.Length);
                    float flScale = numbers[randomIndex];

                    AdvLogging.DisplayLog(AdvFeatureClass, " Random Size: " + flScale);
                    __instance.Buffs.AddCustomVar("RandomSize", flScale);
                    // scale down the zombies, or upscale them
                    __instance.gameObject.transform.localScale = new Vector3(flScale, flScale, flScale);
                }

                // Check if there's random ranges
                EntityClass entityClass = __instance.EntityClass;
                if (entityClass.Properties.Values.ContainsKey("RandomSizes"))
                {
                    List<float> Ranges = new List<float>();
                    float flScale = 1f;
                    foreach (string text in entityClass.Properties.Values["RandomSizes"].Split(new char[] { ',' }))
                        Ranges.Add(StringParsers.ParseFloat(text));

                    System.Random random = new System.Random();
                    int randomIndex = random.Next(0, Ranges.Count);
                    flScale = Ranges[randomIndex];
                    AdvLogging.DisplayLog(AdvFeatureClass, " Random Size: " + flScale);
                    __instance.Buffs.AddCustomVar("RandomSize", flScale);

                    // scale down the zombies, or upscale them
                    __instance.gameObject.transform.localScale = new Vector3(flScale, flScale, flScale);

                }

            }
        }

    }

    // Read Helper to make sure the size of the zombies are distributed properly
    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("Read")]
    public class SphereII_EntityAlive_Read
    {
        public static void Postfix(EntityAlive __instance, BinaryReader _br)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return;

            try
            {
                if (RandomSizeHelper.AllowedRandomSize(__instance))
                {

                    float flScale = _br.ReadSingle();
                    AdvLogging.DisplayLog(AdvFeatureClass, " Read Size: " + flScale);

                    __instance.gameObject.transform.localScale = new Vector3(flScale, flScale, flScale);
                }
            }
            catch (Exception)
            {

            }



        }
    }

    // Write Helper to make sure the size of the zombies are distributed properly
    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("Write")]
    public class SphereII_EntityAlive_Write
    {
        public static void Postfix(EntityAlive __instance, BinaryWriter _bw)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return;
            try
            {
                if (RandomSizeHelper.AllowedRandomSize(__instance))
                {
                    float flScale = __instance.gameObject.transform.localScale.x;
                    AdvLogging.DisplayLog(AdvFeatureClass, " Write Size: " + flScale);
                    _bw.Write(flScale);
                }

            }
            catch (Exception)
            {

            }

        }

    }

}

