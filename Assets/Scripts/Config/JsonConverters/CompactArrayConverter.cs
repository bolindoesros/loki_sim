using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Text;

public class CompactArrayConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(double[]) ||
               objectType == typeof(float[]) ||
               objectType == typeof(int[]);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        // Build the entire array as a single raw string
        StringBuilder sb = new();
        sb.Append("[");

        if (value is double[] doubleArray)
        {
            for (int i = 0; i < doubleArray.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(FormatCompactNumber(doubleArray[i]));
            }
        }
        else if (value is float[] floatArray)
        {
            for (int i = 0; i < floatArray.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(FormatCompactNumber(floatArray[i]));
            }
        }
        else if (value is int[] intArray)
        {
            for (int i = 0; i < intArray.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(intArray[i].ToString(CultureInfo.InvariantCulture));
            }
        }

        sb.Append("]");

        // Write the entire formatted array as a single raw value
        writer.WriteRawValue(sb.ToString());
    }

    private string FormatCompactNumber(double value)
    {
        // Write 0 as "0" instead of "0.0"
        if (value == 0.0)
        {
            return "0";
        }
        else
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }

    public override bool CanRead => false;

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}