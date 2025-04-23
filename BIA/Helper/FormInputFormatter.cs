using Microsoft.AspNetCore.Mvc.Formatters;
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;


namespace BIA.Helper
{
    /// <summary>
    /// The input formatter for multipart/form-data or x-www-form-urlencoded request bodies.
    /// </summary>
    public class FormInputFormatter : TextInputFormatter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormInputFormatter"/> class.
        /// </summary>
        public FormInputFormatter()
        {
            this.SupportedEncodings.Add(UTF8EncodingWithoutBOM);
            this.SupportedEncodings.Add(UTF16EncodingLittleEndian);

            this.SupportedMediaTypes.Add("multipart/form-data");
            this.SupportedMediaTypes.Add("application/json");
            this.SupportedMediaTypes.Add("application/x-www-form-urlencoded");
        }

        /// <inheritdoc/>
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            object _model = null;

            try
            {
                if (context.HttpContext.Request.HasFormContentType)
                {
                    Dictionary<string, string> _form = new();
                    foreach (string _key in context.HttpContext.Request.Form.Keys)
                    {
                        _form.Add(_key, context.HttpContext.Request.Form[_key]);
                    }

                    string _json = JsonSerializer.Serialize(_form);
                    Type _type = context.ModelType;
                    _model = JsonSerializer.Deserialize(_json, _type, new JsonSerializerOptions
                    {
                        AllowTrailingCommas = true,
                        PropertyNameCaseInsensitive = true,
                        Converters = 
                        { 
                            new IntStringConverter(), 
                            new LongStringConverter(), 
                            new DoubleStringConverter(), 
                            new BoolStringConverter(), 
                            new NullStringConverter(), 
                            new NullableIntStringConverter(),
                            new StringNullableConverter(),
                            new DecimalStringConverter(),
                            new NullableDecimalStringConverter()
                        }
                    });
                }
            }
            catch (JsonException _ex)
            {
                var _path = _ex.Path ?? string.Empty;
                var _modelStateException = new InputFormatterException(_ex.Message, _ex);
                context.ModelState.TryAddModelError(_path, _modelStateException, context.Metadata);
                return await InputFormatterResult.FailureAsync();
            }
            catch (Exception _ex)
            {
                context.ModelState.TryAddModelError(string.Empty, _ex, context.Metadata);
                return await InputFormatterResult.FailureAsync();
            }

            if (_model == null && !context.TreatEmptyInputAsDefaultValue)
            {
                return await InputFormatterResult.FailureAsync();
            }

            return await InputFormatterResult.SuccessAsync(_model);
        }
    }
    public class IntStringConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String && int.TryParse(reader.GetString(), out int result))
            {
                return result;
            }

            return reader.GetInt32();
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
    public class LongStringConverter : JsonConverter<long>
    {
        public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String && long.TryParse(reader.GetString(), out long result))
            {
                return result;
            }

            return reader.GetInt64();
        }

        public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    public class DoubleStringConverter : JsonConverter<double>
    {
        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String && double.TryParse(reader.GetString(), out double result))
            {
                return result;
            }

            return reader.GetDouble();
        }

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
    public class BoolStringConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String && bool.TryParse(reader.GetString(), out bool result))
            {
                return result;
            }

            return reader.GetBoolean();
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
    public class NullStringConverter : JsonConverter<object>
    {
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }
            else if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString();
            }
            else
            {
                return reader.GetInt32();
            }
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            writer.WriteNullValue();
        }
    }

    public class NullableIntStringConverter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String && reader.ValueSpan.Length <= 0)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.String && int.TryParse(reader.GetString(), out int result))
            {
                return result;
            }

            return reader.GetInt32();
        }

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
    public class StringNullableConverter : JsonConverter<string?>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            return reader.GetString();
        }

        public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        {
            if (value != null)
            {
                writer.WriteStringValue(value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }

    public class DecimalStringConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String && decimal.TryParse(reader.GetString(), out decimal result))
            {
                return result;
            }

            return reader.GetDecimal();
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
    public class NullableDecimalStringConverter : JsonConverter<decimal?>
    {
        public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String && reader.ValueSpan.Length <= 0)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.String && decimal.TryParse(reader.GetString(), out decimal result))
            {
                return result;
            }

            return reader.GetDecimal();
        }

        public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value.ToString());
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }


}
