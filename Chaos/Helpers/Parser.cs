using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Chaos.Helpers
{
    public static class Parser
    {
        public static Stream ToStream(this string @this)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(@this);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static T ParseXML<T>(this string @this, string rootElement) where T : class
        {
            XmlRootAttribute root = new XmlRootAttribute();
            root.ElementName = rootElement;
            root.IsNullable = true;

            var reader = XmlReader.Create(@this.Trim().ToStream(), new XmlReaderSettings()
            {
                ConformanceLevel = ConformanceLevel.Document
            });
            return new XmlSerializer(typeof(T), root).Deserialize(reader) as T;
        }

        public static T ParseJSON<T>(this string @this) where T : class
        {
            return JsonConvert.DeserializeObject<T>(@this.Trim());
        }
    }
}
