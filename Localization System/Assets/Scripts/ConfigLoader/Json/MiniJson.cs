using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

public static class MiniJson
{
    public static object Deserialize(string json)
    {
        if (json == null) return null;
        return Parser.Parse(json);
    }

    // Замініть тільки внутрішній клас Parser

    private class Parser : IDisposable
    {
        private const string WORD_BREAK = "{}[],:\"";
        private StringReader _json;

        private Parser(string json)
        {
            _json = new StringReader(json);
        }

        public static object Parse(string json)
        {
            using (var parser = new Parser(json))
            {
                return parser.ParseValue();
            }
        }

        public void Dispose()
        {
            _json.Dispose();
            _json = null;
        }

        private Dictionary<string, object> ParseObject()
        {
            var table = new Dictionary<string, object>();
            _json.Read(); // Eat '{'

            while (true)
            {
                switch (NextToken)
                {
                    case TOKEN.NONE: return null;
                    case TOKEN.COMMA:
                        _json.Read(); // Eat ','
                        continue;
                    case TOKEN.CURLY_CLOSE:
                        _json.Read(); // Eat '}'
                        return table;
                    default:
                        string name = ParseString();
                        if (name == null) return null;

                        if (NextToken != TOKEN.COLON) return null;
                        _json.Read(); // Eat ':'

                        table[name] = ParseValue();
                        if (table[name] == null && NextToken != TOKEN.NULL) return null;
                        break;
                }
            }
        }

        private List<object> ParseArray()
        {
            var array = new List<object>();
            _json.Read(); // Eat '['

            while (true)
            {
                switch (NextToken)
                {
                    case TOKEN.NONE: return null;
                    case TOKEN.COMMA:
                        _json.Read(); // Eat ','
                        continue;
                    case TOKEN.SQUARE_CLOSE:
                        _json.Read(); // Eat ']'
                        return array;
                    default:
                        object value = ParseValue();
                        if (value == null && NextToken != TOKEN.NULL) return null;
                        array.Add(value);
                        break;
                }
            }
        }

        private object ParseValue()
        {
            switch (NextToken)
            {
                case TOKEN.STRING: return ParseString();
                case TOKEN.NUMBER: return ParseNumber();
                case TOKEN.CURLY_OPEN: return ParseObject();
                case TOKEN.SQUARE_OPEN: return ParseArray();
                case TOKEN.TRUE:
                    _json.Read();
                    _json.Read();
                    _json.Read();
                    _json.Read(); // true
                    return true;
                case TOKEN.FALSE:
                    _json.Read();
                    _json.Read();
                    _json.Read();
                    _json.Read();
                    _json.Read(); // false
                    return false;
                case TOKEN.NULL:
                    _json.Read();
                    _json.Read();
                    _json.Read();
                    _json.Read(); // null
                    return null;
                case TOKEN.NONE: return null;
            }

            return null;
        }

        private string ParseString()
        {
            var s = new StringBuilder();
            _json.Read(); // Eat '"'

            bool parsing = true;
            while (parsing)
            {
                if (_json.Peek() == -1) break;
                char c = NextChar;
                switch (c)
                {
                    case '"':
                        parsing = false;
                        break;
                    case '\\':
                        if (_json.Peek() == -1)
                        {
                            parsing = false;
                            break;
                        }

                        c = NextChar;
                        switch (c)
                        {
                            case '"':
                            case '\\':
                            case '/':
                                s.Append(c);
                                break;
                            case 'b':
                                s.Append('\b');
                                break;
                            case 'f':
                                s.Append('\f');
                                break;
                            case 'n':
                                s.Append('\n');
                                break;
                            case 'r':
                                s.Append('\r');
                                break;
                            case 't':
                                s.Append('\t');
                                break;
                            case 'u':
                                var hex = new char[4];
                                for (int i = 0; i < 4; i++) hex[i] = NextChar;
                                s.Append((char)Convert.ToInt32(new string(hex), 16));
                                break;
                        }

                        break;
                    default:
                        s.Append(c);
                        break;
                }
            }

            return s.ToString();
        }

        private object ParseNumber()
        {
            string number = NextWord;
            if (number.IndexOf('.') != -1 || number.IndexOf('e') != -1 || number.IndexOf('E') != -1)
            {
                double.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedDouble);
                return parsedDouble;
            }

            long.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out long parsedInt);
            return parsedInt;
        }

        private void EatWhitespace()
        {
            while (" \t\n\r".IndexOf((char)_json.Peek()) != -1)
            {
                _json.Read();
            }
        }

        private char PeekChar => (char)_json.Peek();
        private char NextChar => (char)_json.Read();

        private string NextWord
        {
            get
            {
                var word = new StringBuilder();
                while (WORD_BREAK.IndexOf(PeekChar) == -1)
                {
                    word.Append(NextChar);
                }

                return word.ToString();
            }
        }

        private TOKEN NextToken
        {
            get
            {
                EatWhitespace();
                if (_json.Peek() == -1) return TOKEN.NONE;
                switch (PeekChar)
                {
                    case '{': return TOKEN.CURLY_OPEN;
                    case '}': return TOKEN.CURLY_CLOSE;
                    case '[': return TOKEN.SQUARE_OPEN;
                    case ']': return TOKEN.SQUARE_CLOSE;
                    case ',': return TOKEN.COMMA;
                    case '"': return TOKEN.STRING;
                    case ':': return TOKEN.COLON;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '-': return TOKEN.NUMBER;
                }

                switch (NextWord)
                {
                    case "false": return TOKEN.FALSE;
                    case "true": return TOKEN.TRUE;
                    case "null": return TOKEN.NULL;
                }

                return TOKEN.NONE;
            }
        }

        private enum TOKEN
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
}