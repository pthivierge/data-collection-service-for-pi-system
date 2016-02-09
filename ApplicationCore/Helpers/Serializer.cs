using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace FDS.Core.Helpers
{
    public static class Serializer
    {
        public static void SerializeObject(string filename, Object objectToSerialize)
        {
            using (Stream stream = File.Open(filename, FileMode.OpenOrCreate))
            {
                var bFormatter = new BinaryFormatter();
                bFormatter.Serialize(stream, objectToSerialize);
                stream.Close();
            }
        }

        public static object DeSerializeObject(string filename)
        {
            object objectToSerialize;
            using (Stream stream = File.Open(filename, FileMode.Open))
            {
                var bFormatter = new BinaryFormatter();
                objectToSerialize = bFormatter.Deserialize(stream);
                stream.Close();
                return objectToSerialize;
            }
        }
    }
}
