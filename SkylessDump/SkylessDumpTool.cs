using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityAssetLib;
using UnityAssetLib.Serialization;
using UnityAssetLib.Types;

namespace SkylessDump
{
    public class SkylessDumpTool
    {
        private static readonly DataType[] types = new[]
        {
            new DataType("areas",     "Area"    ),
            new DataType("bargains",  "Bargain" ),
            new DataType("domiciles", "Domicile"),
            new DataType("events",    "Event"   ),
            new DataType("exchanges", "Exchange"),
            new DataType("personas",  "Persona" ),
            new DataType("prospects", "Prospect"),
            new DataType("qualities", "Quality" ),
            new DataType("settings",  "Setting" ),
        };

        private readonly SkylessSerializer skylessSerializer;

        public event Action<string> LogCallback;

        public SkylessDumpTool(SkylessSerializer serializer)
        {
            this.skylessSerializer = serializer;
        }

        public void ExportAllToJson(string resourceAssetsPath, string outputDirectory)
        {
            using (AssetsFile file = AssetsFile.Open(resourceAssetsPath))
            {
                var assetSerializer = new UnitySerializer(file);

                foreach (DataType dType in types)
                {
                    LogCallback?.Invoke(dType.AssetName);
                    AssetInfo info = file.GetAssetByName(dType.AssetName);

                    var textAsset = assetSerializer.Deserialize<TextAsset>(info);
                    var obj = skylessSerializer.DeserializeBinary(dType.TypeName, textAsset.m_Data);

                    string jsonPath = Path.Combine(outputDirectory, $"{dType.AssetName}.json");
                    skylessSerializer.SerializeJson(jsonPath, obj);
                }
            }
        }

        public void ImportAllJsonToAsset(string inputAssetsPath, string jsonDirectory, string outputAssetsPath)
        {
            using (AssetsFile file = AssetsFile.Open(inputAssetsPath))
            {
                var assetSerializer = new UnitySerializer(file);

                foreach (DataType dType in types)
                {
                    AssetInfo info = file.GetAssetByName(dType.AssetName);
                    string jsonPath = Path.Combine(jsonDirectory, $"{dType.AssetName}.json");
                    LogCallback?.Invoke(dType.AssetName);

                    var obj = skylessSerializer.DeserializeJson(dType.TypeName, jsonPath);
                    var binaryData = skylessSerializer.SerializeBinary(dType.TypeName, obj);

                    var textAsset = new TextAsset();
                    textAsset.m_Name = dType.AssetName;
                    textAsset.m_Data = binaryData;

                    var assetData = assetSerializer.Serialize(textAsset);

                    file.ReplaceAsset(info.pathID, assetData);
                }

                file.Save(outputAssetsPath);
            }
        }

        public class DataType
        {
            public DataType() { }

            public DataType(string assetName, string typeName)
            {
                AssetName = assetName;
                TypeName = typeName;
            }

            public string AssetName { get; set; }
            public string TypeName { get; set; }
        }
    }
}
