using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NmsDotnet.Utils
{
    public class MyJsonConveter : JsonConverter
    {
        public override bool CanWrite { get { return false; } }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if ((bool)value == true)
            {
                writer.WriteValue("Y");
            }
            else
            {
                writer.WriteValue("N");
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;

            if (value == null || String.IsNullOrWhiteSpace(value.ToString()))
            {
                return false;
            }

            if ("Y".Equals((string)value, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(String) || objectType == typeof(Boolean))
            {
                return true;
            }
            return false;
        }
    }
}