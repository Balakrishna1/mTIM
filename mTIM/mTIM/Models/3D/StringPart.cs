using System;
using System.Collections.Generic;
using mTIM.Models;

namespace mTIM.Models
{
    public class StringPart
    {
        /// <summary>Constructor.</summary>
        public StringPart()
        {
            buffer = null;
            bufferLength = 0;
        }

        public StringPart(string _data, long _bufferlength)
        {
            this.buffer = _data;
            this.bufferLength = _bufferlength;
        }

        /// <summary>Constructor.</summary>
        /// <param name="_data">The pointer to the start of the string.</param>
        /// <param name="_length">The length of the string.</param>
        public StringPart(char _data, int _length)
        {
            buffer = _data.ToString();
            bufferLength = _length;
        }

        /// <summary>Constructor.</summary>
        /// <param name="string">The string to create stringpart from.</param>
        public StringPart(String str)
        {
            buffer = str;
            bufferLength = str.Length;
        }

        /// <summary>Constructor.</summary>
        /// <param name="string">The string to create stringpart from.</param>
        //public StringPart(Utf8String str)
        //{

        //}

        /// <summary>Constructor.</summary>
        /// <param name="_data">The pointer to the start of the string.</param>
        public StringPart(char _data)
        {
            buffer = _data.ToString();
            bufferLength = 0;
        }
        /// <summary>Destructor.</summary>
        ~StringPart()
        {
        }

        /// <summary>Clears the string part.</summary>
        public void Clear()
        {
            buffer = null;
            bufferLength = 0;
        }

        /// <summary>Tries to parse the int value.</summary>
        /// <param name="val">The int value if true is returned.</param>
        /// <returns>True if parsing the int was successful.</returns>
        bool TryParse(UInt64 data)
        {
            int val = 0;
            if (bufferLength == 0)
                return true;

            int index = 0;
            while (index < bufferLength && char.IsWhiteSpace(buffer[index]))
                index++;

            int multiplier = 1;
            if (buffer[index] == '-')
            {
                multiplier = -1;
                index++;
            }

            while (index < bufferLength)
            {
                char ch = buffer[index];
                if (ch >= '0' && ch <= '9')
                {
                    val = val * 10 + ((int)ch - (int)'0');
                    index++;
                }
                else if (ch == ' ')
                    return true;
                else
                    return false;
            }
            val = val * multiplier;
            data = (UInt64)val;
            return true;

        }
        /// <summary>Tries to parse the int value.</summary>
        /// <param name="val">The int value if true is returned.</param>
        /// <returns>True if parsing the int was successful.</returns>
        bool TryParse(UInt16 val)
        {
            return false;
        }
        /// <summary>Tries to parse the int value.</summary>
        /// <param name="val">The int value if true is returned.</param>
        /// <returns>True if parsing the int was successful.</returns>
        bool TryParse(Int32 val)
        {
            val = 0;
            if (bufferLength == 0)
                return true;
            int index = 0;
            while (index < bufferLength && char.IsWhiteSpace(buffer[index]))
                index++;

            while (index < bufferLength)
            {
                char ch = buffer[index];
                if (ch >= '0' && ch <= '9')
                {
                    val = val * 16 + ((int)ch - (int)'0');
                    index++;
                }
                else if (ch >= 'a' && ch <= 'f')
                {
                    val = val * 16 + (10 + (int)ch - (int)'a');
                    index++;
                }
                else if (ch >= 'A' && ch <= 'F')
                {
                    val = val * 16 + (10 + (int)ch - (int)'A');
                    index++;
                }
                else if (ch == ' ')
                    return true;
                else
                    return false;
            }
            // val = val;
            return true;

        }

        /// <summary>Tries to parse the int value.</summary>
        /// <param name="val">The int value if true is returned.</param>
        /// <returns>True if parsing the int was successful.</returns>
        bool TryParse(long val)
        {
            val = 0;
            if (bufferLength == 0)
                return true;

            int index = 0;
            while (index < bufferLength && char.IsWhiteSpace(buffer[index]))
                index++;

            int multiplier = 1;
            if (buffer[index] == '-')
            {
                multiplier = -1;
                index++;
            }

            while (index < bufferLength)
            {
                char ch = buffer[index];
                if (ch >= '0' && ch <= '9')
                {
                    val = val * 10 + ((int)ch - (int)'0');
                    index++;
                }
                else if (ch == ' ')
                    return true;
                else
                    return false;
            }
            val = val * multiplier;
            return true;
        }

        /// <summary>Tries to parse the int value.</summary>
        /// <param name="val">The int value if true is returned.</param>
        /// <returns>True if parsing the int was successful.</returns>
        public bool TryParse(ref UInt16 val)
        {
            UInt64 v = new UInt64();
            if (TryParse(v))
            {
                val = (UInt16)v;
                return true;
            }
            return false;
        }



        /// <summary>Tries to parse the int value.</summary>
        /// <param name="val">The int value if true is returned.</param>
        /// <returns>True if parsing the int was successful.</returns>
        bool TryParse(sbyte data)
        {
            var val = 0;
            if (bufferLength == 0)
                return true;

            int index = 0;
            while (index < bufferLength && char.IsWhiteSpace(buffer[index]))
                index++;

            int multiplier = 1;
            if (buffer[index] == '-')
            {
                multiplier = -1;
                index++;
            }
            else if (buffer[index] == '+')
                index++;

            while (index < bufferLength)
            {
                char ch = buffer[index];
                if (ch >= '0' && ch <= '9')
                {
                    val = ((int)val) * 10 + ((int)ch - (int)'0');
                    index++;
                }
                else if (ch == ' ')
                    return true;
                else
                    return false;
            }
            val = val * multiplier;
            data = (sbyte)val;
            return true;

        }
        /// <summary>Tries to parse the u8 value.</summary>
        /// <param name="val">The u8 value if true is returned.</param>
        /// <returns>True if parsing the u8 was successful.</returns>
        bool TryParse(UInt32 val)
        {
            UInt64 v = new UInt64();
            if (TryParse(v))
            {
                val = (UInt32)v;
                return true;
            }
            return false;

        }

        /// <summary>Tries to parse the u8 value.</summary>
        /// <param name="val">The u8 value if true is returned.</param>
        /// <returns>True if parsing the u8 was successful.</returns>
        public bool TryParse(ref UInt32 val)
        {
            UInt64 v = new UInt64();
            if (TryParse(v))
            {
                val = (UInt32)v;
                return true;
            }
            return false;
        }


        /// <summary>Tries to parse the float value.</summary>
        /// <param name="val">The float value if true is returned.</param>
        /// <returns>True if parsing the float was successful.</returns>
        bool TryParse(float val)
        {
            double db = 0;
            if (TryParse(db))
            {
                if (db >= Math.Min(db, val) && db <= Math.Max(db,val))
                {
                    val = (float)db;
                    return true;
                }
                return false;
            }
            return false;

        }
        /// <summary>Tries to parse the double value.</summary>
        /// <param name="val">The double value if true is returned.</param>
        /// <returns>True if parsing the double was successful.</returns>
        bool TryParse(double val)
        {
            val = 0.0f;
            int exponent = 0;
            int negativ = 0;
            int index = 0;
            double p10 = 0.0;
            int numDigits = 0;
            int numDecimals = 0;
            int n = 0;

            // Skip leading whitespace
            while (index < bufferLength && char.IsWhiteSpace(buffer[index]))
                index++;
            if (index >= bufferLength)
                return true;

            switch (buffer[index])
            {
                case 'n':
                    val = Single.NaN;
                    break;
                case '-':
                    negativ = 1;
                    index++;
                    break;
                case '+':
                    index++;
                    break;
            }

            // Process string of digits
            while (index < bufferLength && char.IsDigit(buffer[index]))
            {
                val = val * 10 + (buffer[index] - '0');
                index++;
                numDigits++;
            }
            if (index >= bufferLength)
            {
                if (negativ <= 0)
                    val = -val;
                return true;
            }

            // Process decimal part
            if (buffer[index] == '.')
            {
                index++;

                while (index < bufferLength && char.IsDigit(buffer[index]))
                {
                    val = val * 10 + (buffer[index] - '0');
                    index++;
                    numDigits++;
                    numDecimals++;
                }

                exponent -= numDecimals;
            }

            if (numDigits == 0)
            {
                return true;
            }

            // correct the sign
            if (negativ <=0)
                val = -val;

            // Process an exponent string
            if (index < bufferLength && (buffer[index] == 'e' || buffer[index] == 'E'))
            {
                // Handle optional sign
                negativ = 0;
                index++;
                switch (buffer[index])
                {
                    case '-':
                        negativ = 1;
                        index++;
                        break;
                    case '+':
                        index++;
                        break;
                }

                // Process string of digits
                n = 0;
                while (index < bufferLength && char.IsDigit(buffer[index]))
                {
                    n = n * 10 + (buffer[index] - '0');
                    index++;
                }

                if (negativ <=0)
                    exponent -= n;
                else
                    exponent += n;
            }

            if (exponent < float.MinValue || exponent > float.MaxValue)
            {
                val = Single.PositiveInfinity;
                return false;
            }

            // Scale the result
            p10 = 10;
            n = exponent;
            if (n < 0) n = -n;
            while (n > 0)
            {
                if (n == 1)
                {
                    if (exponent < 0)
                        val /= p10;
                    else
                        val *= p10;
                }
                n >>= 1;
                p10 *= p10;
            }

            if (val == Single.PositiveInfinity)
                return false;
            return true;

        }
        /// <summary>Tries to parse the bool value.</summary>
        /// <param name="val">The int value if true is returned.</param>
        /// <returns>True if parsing the int was successful.</returns>
        bool TryParse(bool val)
        {
            int valInt = 0;
            if (TryParse(valInt))
            {
                val = valInt != 0;
                return true;
            }
            else if (EqualsNoCase(new StringPart("false")))
                val = false;
            else if (EqualsNoCase(new StringPart("true")))
                val = true;
            else
                return false;
            return true;

        }
        /// <summary>Tries to parse an u32 value from a hex string.</summary>
        /// <param name="val">The u32 value.</param>
        /// <returns>True if successful.</returns>
        bool TryParseHex(int val)
        {
            val = 0;
            if (bufferLength == 0)
                return true;
            int index = 0;
            while (index < bufferLength && char.IsWhiteSpace(buffer[index]))
                index++;

            while (index < bufferLength)
            {
                char ch = buffer[index];
                if (ch >= '0' && ch <= '9')
                {
                    val = val * 16 + ((int)ch - (int)'0');
                    index++;
                }
                else if (ch >= 'a' && ch <= 'f')
                {
                    val = val * 16 + (10 + (int)ch - (int)'a');
                    index++;
                }
                else if (ch >= 'A' && ch <= 'F')
                {
                    val = val * 16 + (10 + (int)ch - (int)'A');
                    index++;
                }
                else if (ch == ' ')
                    return true;
                else
                    return false;
            }
            // val = val;
            return true;

        }


        /// <summary>Test if the string part starts with a certain character.</summary>
        /// <param name="ch">The character to test with.</param>
        /// <returns>True if the string starts with the given character.</returns>
        bool StartsWith(char ch)
        {
            return bufferLength > 0 && buffer[0] == ch;
        }

        /// <summary>Test if the string part starts with a certain character.</summary>
        /// <param name="str">The str to test with.</param>
        /// <returns>True if the string starts with the given character.</returns>
        public bool StartsWith(string str)
        {
            if (str == null)
            {
                return bufferLength == 0;
            }

            int strlength = str.Length;
            if (strlength > bufferLength)
            {
                return false;
            }

            for (int i = 0; i < strlength; i++)
            {
                if (str[i] != buffer[i])
                {
                    return false;
                }
            }
            return true;
        }

        bool StartsWith(StringPart stringPart)
        {
            if (stringPart.bufferLength > bufferLength)
            {
                return false;
            }

            for (int i = 0; i < stringPart.bufferLength; i++)
            {
                if (buffer[i] != stringPart.buffer[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>Test if the string part ends with a certain character.</summary>
        /// <param name="ch">The character to test with.</param>
        /// <returns>True if the string ends with the given character.</returns>
        bool EndsWith(char ch)
        {
            return bufferLength > 0 && buffer[(int)bufferLength - 1] == ch;
        }

        /// <summary>Test if the string part ends with a certain character.</summary>
        /// <param name="ch">The character to test with.</param>
        /// <returns>True if the string ends with the given character.</returns>
        public bool EndsWith(string str)
        {
            if (str == null)
            {
                return bufferLength == 0;
            }
            int strlength = str.Length;
            if (strlength > bufferLength)
            {
                return false;
            }
            for (int i = 0; i < strlength; i++)
            {
                if (str[i] != buffer[(int)bufferLength - strlength + i])
                {
                    return false;
                }
            }
            return true;
        }



        /// <summary>Searches a character from left to right in a string.</summary>
        /// <param name="character">The character to search.</param>
        /// <param name="start">The start index to search from.</param>
        /// <returns>The index of the character or -1 if not found.</returns>
        public int Find(char character, int startIndex = 0)
        {
            for (int i = startIndex; i < bufferLength; i++)
            {
                if (buffer[i] == character)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>Searches a character from left to right in a string.</summary>
        /// <param name="character">The character to search.</param>
        /// <param name="start">The start index to search from.</param>
        /// <returns>The index of the character or -1 if not found.</returns>
        public int FindFirst(char character, int start)
        {
            for (int i = start; i < bufferLength; i++)
            {
                if (buffer[i] == character)
                {
                    return i;
                }
            }
            return -1;
        }


        /// <summary>Searches a character from left to right in a string.</summary>
        /// <param name="character">The character to search.</param>
        /// <param name="start">The start index to search from.</param>
        /// <returns>The index of the character or -1 if not found.</returns>
        public int FindFirst(String val, int start)
        {
            int stringLength = val.Length;
            if (stringLength == 0 || bufferLength == 0)
            {
                return -1;
            }
            for (int i = start; i < bufferLength - stringLength + 1; i++)
            {
                bool found = true;
                for (int j = 0; j < stringLength; j++)
                {
                    if (buffer[i + j] != val[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return i;
                }
            }
            return -1;
        }


        /// <summary>Creates a sub string from a string.</summary>
        /// <param name="start">The start index of the sub string.</param>
        /// <param name="_length">The length of the sub string.</param>
        /// <returns>The subString.</returns>
        public StringPart SubString(long start, long _length)
        {
            if (bufferLength == 0 || start >= bufferLength)
            {
                return new StringPart();
            }
            if (_length < 0)
            {
                _length = bufferLength - start;
            }
            if (_length <= 0)
            {
                return new StringPart();
            }
            return new StringPart(buffer + start, _length);
        }


        /// <summary>Removes all spaces on the left side.</summary>
        public void TrimLeft()
        {
            while (bufferLength > 0 && (buffer[0] == ' ' || buffer[0] == '\t'))
            {
                bufferLength--;
            }

        }

        /// <summary>Removes all spaces on the right side.</summary>
        public void TrimRight()
        {
            while (bufferLength > 0 && (buffer[(int)bufferLength - 1] == ' ' || buffer[(int)bufferLength - 1] == '\t'))
            {
                bufferLength--;
            }
        }

        public void Trim()
        {
            TrimLeft();
            TrimRight();
        }

        private bool Operator(string str)
        {
            if (str == null)
            {
                return bufferLength == 0;
            }

            for (int i = 0; i < bufferLength; i++)
            {
                if (buffer[i] != str[i])
                {
                    return false;
                }
            }

            return str[(int)bufferLength] == 0;
        }


        /// <summary>Splits the current string into several items.</summary>
        /// <param name="splitCharacters">The characters to use for splitting.</param>
        /// <param name="items">The array to take the result items.</param>
        public void Split(char splitCharacter, List<StringPart> items)
        {
            string splitStart = buffer;
            int splitLength = 0;

            int index = 0;
            while (index < bufferLength)
            {
                if (buffer[index] == splitCharacter)
                {
                    if (splitLength > 0)
                    {
                        items.Add(new StringPart(splitStart, splitLength));
                    }
                    index++;
                    splitStart = buffer[index].ToString();
                    splitLength = 0;
                }
                else
                {
                    index++;
                    splitLength++;
                }
            }

            if (splitLength > 0)
            {
                items.Add(new StringPart(splitStart, splitLength));
            }
        }


        /// <summary>Splits the current string into several items.</summary>
        /// <param name="splitCharacters">The characters to use for splitting.</param>
        /// <param name="items">The array to take the result items.</param>
        public void SplitByCharacters(String splitCharacters, List<StringPart> items)
        {
            string splitStart = buffer;
            int splitLength = 0;

            int index = 0;
            while (index < bufferLength)
            {
                bool isSplit = false;
                for (int j = 0; j < splitCharacters.Length; j++)
                {
                    char ch = splitCharacters[j];
                    if (buffer[index] == ch)
                    {
                        isSplit = true;
                    }
                }

                if (isSplit)
                {
                    if (splitLength > 0)
                    {
                        items.Add(new StringPart(splitStart, splitLength));
                    }
                    index++;
                    splitStart = buffer[index].ToString();
                    splitLength = 0;
                }
                else
                {
                    index++;
                    splitLength++;
                }
            }

            if (splitLength > 0)
            {
                items.Add(new StringPart(splitStart, splitLength));
            }
        }


        /// <summary>Splits the current string into several items.</summary>
        /// <param name="splitString">The string to use for splitting.</param>
        /// <param name="items">The array to take the result items.</param>
        public void SplitByString(StringPart splitString, List<StringPart> items)
        {
            string splitStart = buffer;
            int splitLength = 0;

            //int l2 = splitString.bufferLength-1;

            long index = 0;
            while (index <= bufferLength - splitString.bufferLength)
            {
                if (SubString(index, splitString.bufferLength).Equals(splitString))
                {
                    if (splitLength > 0)
                    {
                        items.Add(new StringPart(splitStart, splitLength));
                    }
                    index += splitString.Length();
                    splitStart = buffer[(int)index].ToString();
                    splitLength = 0;
                }
                else
                {
                    index += 1;
                    splitLength++;
                }
            }

            if (splitLength > 0)
            {
                items.Add(new StringPart(splitStart, splitLength + splitString.bufferLength - 1));
            }
        }


        /// <summary>Returns the length of the string.</summary>
        /// <returns>The length of the string.</returns>
        public long Length()
        {
            return bufferLength;
        }
        /// <summary>Returns the length of the string.</summary>
        /// <returns>The length of the string.</returns>
        long TextLength()
        {
            return bufferLength;
        }
        /// <summary>Returns the length of the string.</summary>
        /// <returns>The length of the string.</returns>
        long BufferLength()
        {
            return bufferLength;
        }

        /// <summary>Searches the given character within the string.</summary>
        /// <param name="character">The character to search for.</param>
        /// <param name="startIndex">The startIndex.</param>
        /// <returns>The index or -1 if not found.</returns>
        public int Find(string character, int startIndex)
        {
            for (int i = startIndex; i < bufferLength; i++)
            {
                if (buffer[i].Equals(character))
                {
                    return i;
                }
            }
            return -1;
        }


        /// <summary>Tests if the given character exists within the string.</summary>
        /// <param name="character">The character to search.</param>
        bool Contains(char character)
        {
            return Find(character) != -1;
        }

        /// <summary>Equality operator.</summary>
        /// <param name="string">The string to test with.</param>
        /// <returns>True if both strings are equal.</returns>
        //bool operator ==(const StringPart& string) const;
        /// <summary>Inequality operator.</summary>
        /// <param name="string">The string to test with.</param>
        /// <returns>True if both strings are not equal.</returns>
        //bool operator !=(const StringPart& string) const;

        /// <summary>Assignment operator.</summary>
        /// <param name="string">The string to assign.</param>
        /// <returns>The assigned string.</returns>
        //StringPart  operator=(const StringPart& string);

        /// <summary>Equality operator.</summary>
        /// <param name="string">The string to test with.</param>
        /// <returns>True if both strings are equal.</returns>
        //bool operator ==(const char* string) const;
        /// <summary>Inequality operator.</summary>
        /// <param name="string">The string to test with.</param>
        /// <returns>True if both strings are not equal.</returns>
        //bool operator !=(const char* string) const;

        /// <summary>Equality operator.</summary>
        /// <param name="string">The string to test with.</param>
        /// <returns>True if both strings are equal.</returns>
        //bool operator ==(const String& string) const;
        /// <summary>Inequality operator.</summary>
        /// <param name="string">The string to test with.</param>
        /// <returns>True if both strings are not equal.</returns>
        //bool operator !=(const String& string) const;

        /// <summary>Returns the buffer.</summary>
        /// <returns>The buffer.</returns>
        public string GetBuffer()
        {
            return buffer;
        }
        /// <summary>Returns the buffer.</summary>
        /// <returns>The buffer.</returns>
        //char GetBuffer()
        //{
        //    return buffer;
        //}

        /// <summary>Tests if the string is equal to another string.</summary>
        /// <param name="stringPart">The string part to test with.</param>
        /// <returns>True if the strings are equal.</returns>
        public bool Equals(StringPart stringPart)
        {
            if (bufferLength != stringPart.bufferLength)
            {
                return false;
            }
            if (buffer == stringPart.buffer)
            {
                return true;
            }
            for (int i = 0; i < bufferLength; i++)
            {
                if (buffer[i] != stringPart.buffer[i])
                {
                    return false;
                }
            }
            return true;
        }


        /// <summary>Tests if the string is equal to another string.</summary>
        /// <param name="stringPart">The string part to test with.</param>
        /// <returns>True if the strings are equal.</returns>
        public bool Equals(char text)
        {
            return Equals(new StringPart(text));
        }

        /// <summary>Tests if the string is equal to another string without consideration of case differences.</summary>
        /// <param name="stringPart">The string part to test with.</param>
        /// <returns>True if the strings are equal.</returns>
        public bool EqualsNoCase(StringPart stringPart)
        {
            if (bufferLength != stringPart.bufferLength)
            {
                return false;
            }
            if (buffer == stringPart.buffer)
            {
                return true;
            }
            for (int i = 0; i < bufferLength; i++)
            {
                if (char.ToLower(buffer[i]) != char.ToLower(stringPart.buffer[i]))
                {
                    return false;
                }
            }
            return true;
        }


        /// <summary>Tests if the string is equal to another string without consideration of case differences.</summary>
        /// <param name="stringPart">The string part to test with.</param>
        /// <returns>True if the strings are equal.</returns>
        bool EqualsNoCase(char text)
        {
            return EqualsNoCase(new StringPart(text));
        }

        /// <summary>Calculates the hash value from a string.</summary>
        /// <returns>The strign to calculate hash value from.</returns>
        public int CalcHash()
        {
            // djb2 hash
            int hash = 5381;
            for (int i = 0; i < bufferLength; i++)
            {
                char c = buffer[i];
                hash = ((hash << 5) + hash) + (int)c; // hash*33 + c
            }
            return hash;
        }

        public string buffer;
        public long bufferLength;
    }
}
