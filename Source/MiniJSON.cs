using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public static class MiniJSON
{
    public static object Deserialize(string json)
    {
        if (json == null) return null;
        return Parser.Parse(json);
    }

    public static string Serialize(object obj)
    {
        return Serializer.Serialize(obj);
    }

    sealed class Parser : IDisposable
    {
        const string WORD_BREAK = "{}[],:\"";

        StringReader json;

        Parser(string jsonString)
        {
            json = new StringReader(jsonString);
        }

        public static object Parse(string jsonString)
        {
            using (var instance = new Parser(jsonString))
            {
                return instance.ParseValue();
            }
        }

        public void Dispose()
        {
            json.Dispose();
            json = null;
        }

        Dictionary<string, object> ParseObject()
        {
            Dictionary<string, object> table = new Dictionary<string, object>();
            json.Read(); // skip '{'

            while (true)
            {
                switch (NextToken)
                {
                    case TOKEN.CURLY_CLOSE: return table;
                    case TOKEN.COMMA: continue;
                    default:
                        string name = ParseString();
                        if (NextToken != TOKEN.COLON) return null;
                        json.Read();
                        table[name] = ParseValue();
                        break;
                }
            }
        }

        List<object> ParseArray()
        {
            List<object> array = new List<object>();
            json.Read(); // skip '['

            while (true)
            {
                TOKEN next = NextToken;
                if (next == TOKEN.SQUARE_CLOSE) break;
                if (next == TOKEN.COMMA) continue;
                array.Add(ParseValue());
            }

            return array;
        }

        object ParseValue()
        {
            switch (NextToken)
            {
                case TOKEN.STRING: return ParseString();
                case TOKEN.NUMBER: return ParseNumber();
                case TOKEN.CURLY_OPEN: return ParseObject();
                case TOKEN.SQUARE_OPEN: return ParseArray();
                case TOKEN.TRUE: json.Read(); json.Read(); json.Read(); json.Read(); return true;
                case TOKEN.FALSE: json.Read(); json.Read(); json.Read(); json.Read(); json.Read(); return false;
                case TOKEN.NULL: json.Read(); json.Read(); json.Read(); json.Read(); return null;
                default: return null;
            }
        }

        string ParseString()
        {
            StringBuilder s = new StringBuilder();
            json.Read(); // skip '"'

            while (true)
            {
                if (json.Peek() == -1) break;
                char c = NextChar;
                if (c == '"') break;

                if (c == '\\')
                {
                    if (json.Peek() == -1) break;
                    c = NextChar;
                    if (c == '"') s.Append('"');
                    else if (c == '\\') s.Append('\\');
                    else if (c == '/') s.Append('/');
                    else if (c == 'b') s.Append('\b');
                    else if (c == 'f') s.Append('\f');
                    else if (c == 'n') s.Append('\n');
                    else if (c == 'r') s.Append('\r');
                    else if (c == 't') s.Append('\t');
                    else if (c == 'u')
                    {
                        char[] hex = new char[4];
                        for (int i = 0; i < 4; i++) hex[i] = NextChar;
                        s.Append((char)Convert.ToInt32(new string(hex), 16));
                    }
                }
                else s.Append(c);
            }

            return s.ToString();
        }

        object ParseNumber()
        {
            string number = NextWord;
            if (number.Contains(".")) return double.Parse(number, System.Globalization.CultureInfo.InvariantCulture);
            return int.Parse(number);
        }

        void EatWhitespace()
        {
            while (char.IsWhiteSpace(PeekChar)) json.Read();
        }

        char PeekChar { get { return Convert.ToChar(json.Peek()); } }
        char NextChar { get { return Convert.ToChar(json.Read()); } }
        string NextWord
        {
            get
            {
                StringBuilder word = new StringBuilder();
                while (!IsWordBreak(PeekChar) && json.Peek() != -1) word.Append(NextChar);
                return word.ToString();
            }
        }

        TOKEN NextToken
        {
            get
            {
                EatWhitespace();
                if (json.Peek() == -1) return TOKEN.NONE;

                char c = PeekChar;
                if (c == '{') return TOKEN.CURLY_OPEN;
                if (c == '}') { json.Read(); return TOKEN.CURLY_CLOSE; }
                if (c == '[') return TOKEN.SQUARE_OPEN;
                if (c == ']') { json.Read(); return TOKEN.SQUARE_CLOSE; }
                if (c == ',') { json.Read(); return TOKEN.COMMA; }
                if (c == '"') return TOKEN.STRING;
                if (c == ':') { json.Read(); return TOKEN.COLON; }
                if (char.IsDigit(c) || c == '-') return TOKEN.NUMBER;

                string word = NextWord;
                if (word == "false") return TOKEN.FALSE;
                if (word == "true") return TOKEN.TRUE;
                if (word == "null") return TOKEN.NULL;

                return TOKEN.NONE;
            }
        }

        bool IsWordBreak(char c)
        {
            return char.IsWhiteSpace(c) || WORD_BREAK.IndexOf(c) != -1;
        }

        enum TOKEN
        {
            NONE,
            CURLY_OPEN,
            CURLY_CLOSE,
            SQUARE_OPEN,
            SQUARE_CLOSE,
            COLON,
            COMMA,
            STRING,
            NUMBER,
            TRUE,
            FALSE,
            NULL
        }
    }

    sealed class Serializer
    {
        StringBuilder builder = new StringBuilder();

        public static string Serialize(object obj)
        {
            var instance = new Serializer();
            instance.SerializeValue(obj);
            return instance.builder.ToString();
        }

        void SerializeValue(object value)
        {
            if (value == null) builder.Append("null");
            else if (value is string) SerializeString((string)value);
            else if (value is bool) builder.Append((bool)value ? "true" : "false");
            else if (value is IList) SerializeArray((IList)value);
            else if (value is IDictionary) SerializeObject((IDictionary)value);
            else if (value is char) SerializeString(value.ToString());
            else builder.Append(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture));
        }

        void SerializeObject(IDictionary obj)
        {
            builder.Append('{');
            bool first = true;
            foreach (object e in obj.Keys)
            {
                if (!first) builder.Append(',');
                SerializeString(e.ToString());
                builder.Append(':');
                SerializeValue(obj[e]);
                first = false;
            }
            builder.Append('}');
        }

        void SerializeArray(IList array)
        {
            builder.Append('[');
            bool first = true;
            foreach (object obj in array)
            {
                if (!first) builder.Append(',');
                SerializeValue(obj);
                first = false;
            }
            builder.Append(']');
        }

        void SerializeString(string str)
        {
            builder.Append('"');
            foreach (var c in str)
            {
                if (c == '"') builder.Append("\\\"");
                else if (c == '\\') builder.Append("\\\\");
                else if (c == '\b') builder.Append("\\b");
                else if (c == '\f') builder.Append("\\f");
                else if (c == '\n') builder.Append("\\n");
                else if (c == '\r') builder.Append("\\r");
                else if (c == '\t') builder.Append("\\t");
                else builder.Append(c);
            }
            builder.Append('"');
        }
    }
}
