using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SkylessDump
{

    /// <summary>
    /// Sunless Skies data file serializer. Requires original Assembly-CSharp.dll to work.
    /// Used reflection to avoid messy dependencies.
    /// </summary>
    public class SkylessSerializer
    {
        private const string ASSEMBLY_NAME = "Assembly-CSharp.dll";

        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings()
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            Formatting = Formatting.Indented
        };

        /// <summary>
        ///     Assembly-CSharp.dll Assembly object.
        /// </summary>
        private static Assembly asm;

        /// <summary>
        ///     Initialize serializer usig Assembly-CSharp.dll path.
        /// </summary>
        /// <param name="assemblyPath">Path to Assembly-CSharp.dll</param>
        public SkylessSerializer(string assemblyPath)
        {
            if (asm == null)
                asm = Assembly.LoadFrom(assemblyPath);
        }

        public IList DeserializeBinary(string typeName, BinaryReader br)
        {
            var serializerType = asm.GetType($"BinarySerializer.BinarySerializer_{typeName}") ??
                throw new ArgumentException($"Invalid type name {typeName} : BinarySerializer.BinarySerializer_{typeName} does not exist"); ;

            var method = serializerType.GetMethod("DeserializeCollection") ??
                throw new ArgumentException($"Failed to get DeserializeCollection method from {serializerType.FullName}");

            return (IList) method.Invoke(null, new object[] { br });
        }

        public IList DeserializeBinary(string typeName, byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader br = new BinaryReader(ms))
            {
                return DeserializeBinary(typeName, br);
            }
        }

        public void SerializeBinary(string typeName, BinaryWriter bw, IEnumerable data)
        {
            var serializerType = asm.GetType($"BinarySerializer.BinarySerializer_{typeName}") ??
                throw new ArgumentException($"Invalid type name {typeName} : BinarySerializer.BinarySerializer_{typeName} does not exist"); ;

            var method = serializerType.GetMethod("SerializeCollection") ??
                throw new ArgumentException($"Failed to get SerializeCollection method from {serializerType.FullName}");

            method.Invoke(null, new object[] { bw, data });
        }

        public byte[] SerializeBinary(string typeName, IEnumerable data)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                SerializeBinary(typeName, bw, data);
                return ms.ToArray();
            }
        }

        public IList DeserializeJson(string typeName, string path)
        {
            var objectType = asm.GetType($"Failbetter.Core.{typeName}");
            var listType = typeof(List<>).MakeGenericType(objectType);
            
            JsonSerializer jsonSerializer = JsonSerializer.Create(_settings);
            object result;

            using (FileStream fs = File.OpenRead(path))
            using (StreamReader sr = new StreamReader(fs))
            using (JsonTextReader jtr = new JsonTextReader(sr))
            {
                result = jsonSerializer.Deserialize(jtr, listType);
            }

            return (IList) result;
        }

        public void SerializeJson(string path, object data)
        {
            using (FileStream fs = File.Create(path))
            using (StreamWriter sw = new StreamWriter(fs))
            using (JsonTextWriter jtw = new JsonTextWriter(sw))
            {
                JsonSerializer.Create(_settings).Serialize(jtw, data);
            }
        }
    }
}
