using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.UnityConverters.Math;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public static class SaveJsonUtility
{
    /// <summary>
    /// Return fully qualified path to save file in SimSettings directory.
    /// </summary>
    public static string GetFullSavePath(string fileName)
    {
        // Path: [Project Root]/SimSettings/
        string directoryPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "SimSettings");

        // Ensure the directory exists so you don't get an IO error
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        return Path.Combine(directoryPath, fileName);
    }

    /// <summary>
    /// Return save path for a given transform, using its name as the file name.
    /// </summary>
    public static string GetSavePathForTransform(Transform transform)
    {
        string fileName = $"{transform.name.ToLower().Replace(" ", "_")}.json";
        return GetFullSavePath(fileName);
    }

    /// <summary>
    /// Check if the given file name is valid (non-null and ends with .json).
    /// </summary>
    public static bool IsValidFileName(string fileName)
    {
        return fileName != null && fileName.EndsWith(".json");
    }

    public static void SetJsonDefaultSettings()
    {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>
            {
                new Vector3Converter(),
                new ControllerCfgConverter(),
                new SensorCfgConverter(),
                new CompactArrayConverter(),
            },
            Formatting = Formatting.Indented,
            ContractResolver = new BaseFirstContractResolver(),
            DefaultValueHandling = DefaultValueHandling.Populate,
            NullValueHandling = NullValueHandling.Ignore
        };
    }
}

public class BaseFirstContractResolver : DefaultContractResolver
{
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var props = base.CreateProperties(type, memberSerialization);

        // Sort so that base class properties come first
        return props
            .OrderBy(p => GetInheritanceDepth(p.DeclaringType))
            .ThenBy(p => p.Order ?? 0)
            .ToList();
    }

    protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);

        // Ignore properties inherited from UnityEngine.Object
        if (member.DeclaringType == typeof(UnityEngine.Object) ||
            member.DeclaringType == typeof(UnityEngine.ScriptableObject))
        {
            property.ShouldSerialize = _ => false;
        }

        return property;
    }

    private int GetInheritanceDepth(Type type)
    {
        int depth = 0;
        while (type.BaseType != null)
        {
            depth++;
            type = type.BaseType;
        }
        return depth;
    }
}