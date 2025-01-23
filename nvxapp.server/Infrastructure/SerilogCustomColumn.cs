using NpgsqlTypes;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.PostgreSQL;
using System.Text.RegularExpressions;
using System.Text;

namespace nvxapp.server.Infrastructure
{
   
    public class SimpleString_ColumnWriter : ColumnWriterBase
    {
        public string Name { get; }

        public PropertyWriteMethod WriteMethod { get; }

        public string? Format { get; }

        public SimpleString_ColumnWriter(string propertyName, PropertyWriteMethod writeMethod = PropertyWriteMethod.ToString, NpgsqlDbType dbType = NpgsqlDbType.Text, string? format = null, int? columnLength = null)
            : base(dbType, columnLength)
        {
            Name = propertyName;
            WriteMethod = writeMethod;
            Format = format;
        }

        public override object? GetValue(LogEvent logEvent, IFormatProvider? formatProvider = null)
        {

            if (logEvent.MessageTemplate != null)
            {

                if (!logEvent.Properties.ContainsKey(Name))
                {
                    return DBNull.Value;
                }

                switch (WriteMethod)
                {
                    case PropertyWriteMethod.Raw:
                        return GetPropertyValue(logEvent.Properties[Name]);
                    case PropertyWriteMethod.Json:
                        {
                            JsonValueFormatter jsonValueFormatter = new JsonValueFormatter();
                            StringBuilder stringBuilder = new StringBuilder();
                            using (StringWriter output = new StringWriter(stringBuilder))
                            {
                                jsonValueFormatter.Format(logEvent.Properties[Name], output);
                            }

                            return stringBuilder.ToString();
                        }
                    default:
                        return logEvent.Properties[Name].ToString(Format, formatProvider);
                }

            }

            return DBNull.Value;



        }

        private object GetPropertyValue(LogEventPropertyValue logEventProperty)
        {

            ScalarValue? scalarValue = logEventProperty as ScalarValue;
            if (scalarValue != null)
            {
                return scalarValue.ToString();
            }

            return logEventProperty;
        }
    }

    public class MethodName_ColumnWriter : ColumnWriterBase
    {
        public string Name { get; }

        public PropertyWriteMethod WriteMethod { get; }

        public string? Format { get; }

        public MethodName_ColumnWriter(string propertyName, PropertyWriteMethod writeMethod = PropertyWriteMethod.ToString, NpgsqlDbType dbType = NpgsqlDbType.Text, string? format = null, int? columnLength = null)
            : base(dbType, columnLength)
        {
            Name = propertyName;
            WriteMethod = writeMethod;
            Format = format;
        }

        public override object GetValue(LogEvent logEvent, IFormatProvider? formatProvider = null)
        {

            if (logEvent.MessageTemplate != null)
            {

                if (!logEvent.Properties.ContainsKey(Name))
                {
                    return DBNull.Value;
                }

                switch (WriteMethod)
                {
                    case PropertyWriteMethod.Raw:
                        return GetPropertyValue(logEvent.Properties[Name]);
                    case PropertyWriteMethod.Json:
                        {
                            JsonValueFormatter jsonValueFormatter = new JsonValueFormatter();
                            StringBuilder stringBuilder = new StringBuilder();
                            using (StringWriter output = new StringWriter(stringBuilder))
                            {
                                jsonValueFormatter.Format(logEvent.Properties[Name], output);
                            }

                            return stringBuilder.ToString();
                        }
                    default:
                        return logEvent.Properties[Name].ToString(Format, formatProvider);
                }

            }

            return DBNull.Value;



        }

        private object GetPropertyValue(LogEventPropertyValue logEventProperty)
        {
            string pattern = "<(.*?)>";
            ScalarValue? scalarValue = logEventProperty as ScalarValue;
            if (scalarValue != null && scalarValue.Value != null )
            {
                string sValue= scalarValue.Value.ToString()??"";
                
                Match match = Regex.Match(sValue, pattern);

                if (match.Success)
                {
                    string result = match.Groups[1].Value;
                    return result;
                }
                else
                {
                    return scalarValue.Value;
                }


            }

            return logEventProperty;
        }
    }

    public class Call_state_ColumnWriter : ColumnWriterBase
    {

        public PropertyWriteMethod WriteMethod { get; }

        public string? Format { get; }

        public Call_state_ColumnWriter(PropertyWriteMethod writeMethod = PropertyWriteMethod.ToString, NpgsqlDbType dbType = NpgsqlDbType.Text, string? format = null, int? columnLength = null)
            : base(dbType, columnLength)
        {
            WriteMethod = writeMethod;
            Format = format;
        }

        public override object GetValue(LogEvent logEvent, IFormatProvider? formatProvider = null)
        {

            if (logEvent.MessageTemplate != null)
            {
                if (logEvent.MessageTemplate.ToString().Contains("END Call:") ||
                    logEvent.MessageTemplate.ToString().Contains("END Call(ExecuteAction ERR):"))
                    return 2;
                else if (logEvent.MessageTemplate.ToString().Contains("Call:"))
                    return 1;
                else
                    return 0;
            }

            return 0;
        }

    }

}
