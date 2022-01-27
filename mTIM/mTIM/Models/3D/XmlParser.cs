using System;
using System.Collections.Generic;

namespace mTIM.Models
{
    public class XmlParser
    {
        public XmlParser()
        {
            currentLevel = 0;
            bracketStart = -1;
            currentPos = 0;
            enableNamespaceRemoving = false;
        }

        ~XmlParser()
        {
        }

        /// <summary>Initializes the xml parser.</summary>
        /// <param name="str">The string to use.</param>    
        public void Init(string str, bool copyString = false)
        {
            Init(str.ToCharArray(), str.Length, copyString);
        }

        public void Init(byte[] _buffer, long _length, bool copyData = false)
        {
            buffer = System.Text.Encoding.UTF8.GetString(_buffer).ToCharArray();
            length = _length;
            currentLevel = 0;
        }

        /// <summary>Initializes the xml parser.</summary>
        /// <param name="_buffer">The buffer to use.</param>
        /// <param name="_length">The length of the buffer.</param>
        public void Init(char[] _buffer, long _length, bool copyData = false)
        {
            buffer = _buffer;
            length = _length;
            currentLevel = 0;
        }

        /// <summary>Test if the end of file is reached.</summary>
        /// <returns>True if end of file is reached.</returns>
        public bool IsEOF()
        {
            return currentPos >= length;
        }

        /// <summary>Parses the next tag..</summary>
        /// <param name="tagInfo">The tag info.</param>
        /// <returns>True if parsing the tag was successful..</returns>
        public bool ParseTag(ref TagInfo tagInfo)
        {
            tagInfo.parser = this;

            AssumeBrackets(false);
            while (currentPos < length && buffer[currentPos] != '<')
            {
                currentPos++;
            }

            if (currentPos >= length)
            {
                return false;
            }

            bracketStart = currentPos;

            if (buffer[currentPos + 1] == '/')
            {
                tagInfo.tagType = TagType.TT_CLOSE;
                tagInfo.bracketStart = bracketStart;
                tagInfo.level = currentLevel;
                currentPos++;
                currentLevel--;
            }
            else
            {
                currentLevel++;
                tagInfo.tagType = TagType.TT_OPEN;
                tagInfo.bracketStart = bracketStart;
                tagInfo.level = currentLevel;
            }

            currentPos++;
            long startIndex = currentPos;
            //int endNameIndex = -1;

            bool endWhile = false;
            while (!endWhile)
            {
                if (currentPos >= length)
                {
                    return false;
                }
                switch (buffer[currentPos])
                {
                    case ':':
                        if (enableNamespaceRemoving)
                        {
                            startIndex = currentPos + 1;
                        }
                        break;
                    case ' ':
                        //endNameIndex = currentPos;
                        endWhile = true;
                        break;
                    case '>':
                        //endNameIndex = currentPos;
                        bracketStart = -1;
                        endWhile = true;
                        break;
                    case '/':
                        //endNameIndex = currentPos;
                        endWhile = true;
                        tagInfo.tagType = TagType.TT_OPEN_CLOSE;
                        tagInfo.bracketStart = bracketStart;
                        tagInfo.level = currentLevel + 1;
                        break;
                }
                currentPos++;
            }
            tagInfo.name = new StringPart();
            tagInfo.attributes = new StringPart();
            tagInfo.name.buffer = buffer[startIndex].ToString();
            tagInfo.name.bufferLength = currentPos - startIndex - 1;
            tagInfo.attributes.buffer = null;
            tagInfo.attributes.bufferLength = 0;

            if (bracketStart != -1)
            {
                bool quotes = false;
                long startAttributes = currentPos;
                while (currentPos < length)
                {
                    switch (buffer[currentPos])
                    {
                        case '>':
                            bracketStart = -1;
                            tagInfo.attributes.buffer = buffer[startAttributes].ToString();
                            tagInfo.attributes.bufferLength = currentPos - startAttributes;
                            currentPos++;
                            return true;
                        case '"':
                            quotes = !quotes;
                            break;
                        case '/':
                            if (!quotes)
                            {
                                bracketStart = -1;
                                tagInfo.attributes.buffer = buffer[startAttributes].ToString();
                                tagInfo.attributes.bufferLength = currentPos - startAttributes;
                                tagInfo.tagType = TagType.TT_OPEN_CLOSE;
                                currentPos++;
                                while (currentPos < length && buffer[currentPos] != '>')
                                {
                                    currentPos++;
                                }
                                currentPos++;
                                return true;
                            }
                            break;
                    }
                    currentPos++;
                }
                return false;
            }
            return true;
        }


        public bool ParseTag(TagInfo tagInfo)
        {
            tagInfo.parser = this;

            AssumeBrackets(false);
            while (currentPos < length && buffer[currentPos] != '<')
            {
                currentPos++;
            }

            if (currentPos >= length)
            {
                return false;
            }

            bracketStart = currentPos;

            if (buffer[currentPos + 1] == '/')
            {
                tagInfo.tagType = TagType.TT_CLOSE;
                tagInfo.bracketStart = bracketStart;
                tagInfo.level = currentLevel;
                currentPos++;
                currentLevel--;
            }
            else
            {
                currentLevel++;
                tagInfo.tagType = TagType.TT_OPEN;
                tagInfo.bracketStart = bracketStart;
                tagInfo.level = currentLevel;
            }

            currentPos++;
            long startIndex = currentPos;
            //int endNameIndex = -1;

            bool endWhile = false;
            while (!endWhile)
            {
                if (currentPos >= length)
                {
                    return false;
                }
                switch (buffer[currentPos])
                {
                    case ':':
                        if (enableNamespaceRemoving)
                        {
                            startIndex = currentPos + 1;
                        }
                        break;
                    case ' ':
                        //endNameIndex = currentPos;
                        endWhile = true;
                        break;
                    case '>':
                        //endNameIndex = currentPos;
                        bracketStart = -1;
                        endWhile = true;
                        break;
                    case '/':
                        //endNameIndex = currentPos;
                        endWhile = true;
                        tagInfo.tagType = TagType.TT_OPEN_CLOSE;
                        tagInfo.bracketStart = bracketStart;
                        tagInfo.level = currentLevel + 1;
                        break;
                }
                currentPos++;
            }
            tagInfo.name = new StringPart();
            tagInfo.attributes = new StringPart();
            tagInfo.name.buffer = buffer[startIndex].ToString();
            tagInfo.name.bufferLength = currentPos - startIndex - 1;
            tagInfo.attributes.buffer = null;
            tagInfo.attributes.bufferLength = 0;

            if (bracketStart != -1)
            {
                bool quotes = false;
                long startAttributes = currentPos;
                while (currentPos < length)
                {
                    switch (buffer[currentPos])
                    {
                        case '>':
                            bracketStart = -1;
                            tagInfo.attributes.buffer = buffer[startAttributes].ToString();
                            tagInfo.attributes.bufferLength = currentPos - startAttributes;
                            currentPos++;
                            return true;
                        case '"':
                            quotes = !quotes;
                            break;
                        case '/':
                            if (!quotes)
                            {
                                bracketStart = -1;
                                tagInfo.attributes.buffer = buffer[startAttributes].ToString();
                                tagInfo.attributes.bufferLength = currentPos - startAttributes;
                                tagInfo.tagType = TagType.TT_OPEN_CLOSE;
                                currentPos++;
                                while (currentPos < length && buffer[currentPos] != '>')
                                {
                                    currentPos++;
                                }
                                currentPos++;
                                return true;
                            }
                            break;
                    }
                    currentPos++;
                }
                return false;
            }
            return true;
        }


        /// <summary>Jumps to a certain tag.</summary>
        /// <param name="tagInfo">The tag info.</param>
        /// <returns>True if the node was found.</returns>
        public bool ParseTag(String name, TagInfo tagInfo, bool currentScopeOnly)
        {
            int scopeCounter = 0;
            while (ParseTag(tagInfo))
            {
                switch (tagInfo.tagType)
                {
                    case TagType.TT_OPEN:
                        scopeCounter++;
                        break;
                    case TagType.TT_CLOSE:
                        scopeCounter--;
                        break;
                    case TagType.TT_OPEN_CLOSE:
                        break;
                    default:
                        break;
                }

                if (scopeCounter < 0 && currentScopeOnly)
                {
                    return false;
                }

                if (tagInfo.name == new StringPart(name))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>Parses the value.</summary>
        /// <param name="content">The content.</param>
        /// <returns>True if successful.</returns>
        /// 
        static bool warned = false;
        public bool ParseValue(TagInfo tagInfo, ref StringPart content)
        {
            content.bufferLength = 0;

            if (tagInfo.tagType == TagType.TT_OPEN_CLOSE)
            {
                return true;
            }

            //ASSERT(tagInfo.tagType == TagType.TT_OPEN, "Invalid tagType!");

            bool inTag = false;
            int depth = 0;
            bool quotes = false;
            long startIndex = currentPos;

            AssumeBrackets(false);
            while (currentPos < length)
            {
                switch (buffer[currentPos])
                {
                    case '"':
                        if (inTag) // Gallneukirchen App VeranstaltungsId="430100080" <string xmlns="urn:eTourist:i18n" xml:lang="de- DE">Titel: "Das tiefe Blech spielt auf.&lt;br /&gt;Konzert der Klasse Martin Dumphart.< / string>
                        {
                            quotes = !quotes;
                        }
                        else
                        {
                            if (!warned)
                            {
                                warned = true;
                            }
                        }
                        break;
                    case '/':
                        if (!quotes)
                        {
                            if (buffer[currentPos + 1] == '>')
                            {
                                depth--;
                            }
                        }
                        break;
                    case '>':
                        if (!quotes)
                        {
                            inTag = false;
                        }
                        break;
                    case '<':
                        inTag = true;
                        long endIndex = currentPos;
                        if (buffer[currentPos + 1] == '/')
                        {
                            if (depth == 0)
                            {
                                content.buffer = buffer[startIndex].ToString();
                                content.bufferLength = endIndex - startIndex;

                                TagInfo ti = new TagInfo();
                                bool ret = ParseTag(ti);
                                //ASSERT(ti.tagType == TT_CLOSE, "Invalid tagType!");
                                return ret;
                            }
                            else
                            {
                                depth--;
                            }
                        }
                        else
                        {
                            depth++;
                        }
                        break;
                };
                currentPos++;
            }

            return false;
        }


        /// <summary>Parses the value.</summary>
        /// <param name="content">The content.</param>
        /// <returns>True if successful.</returns>
        public bool ParseValue(String name, StringPart content, bool currentScopeOnly)
        {
            TagInfo tagInfo = new TagInfo();
            if (!ParseTag(name, tagInfo, currentScopeOnly))
            {
                return false;
            }
            return ParseValue(tagInfo, ref content);
        }


        public bool GetSubParser(String name, XmlParser outParser, bool currentScopeOnly)
        {
            StringPart content = new StringPart();
            if (ParseValue(name, content, currentScopeOnly))
            {
                outParser.Init(content.GetBuffer()?.ToCharArray(), content.Length());
                outParser.EnableNamespaceRemoving(this.enableNamespaceRemoving);
                return true;
            };
            return false;
        }

        public bool GetSubParser(TagInfo tagInfo, XmlParser outParser)
        {
            StringPart content = new StringPart();
            if (ParseValue(tagInfo, ref content))
            {
                outParser.Init(content.GetBuffer().ToCharArray(), content.Length());
                outParser.EnableNamespaceRemoving(this.enableNamespaceRemoving);
                return true;
            };
            return false;
        }


        public StringPart GetSubString(int startIndex, int length)
        {
            return new StringPart(buffer[startIndex].ToString(), length);
        }

        public static void ParseAttributes(StringPart attributes, List<AttributeInfo> targetAttributes)
        {
            targetAttributes.Clear();

            if (attributes.bufferLength == 0)
            {
                return;
            }
            {
                Helper helper = new Helper(attributes, targetAttributes);
                helper.ParseAttributes();
            }
        }


        public void JumpToStart()
        {
            currentPos = 0;
            bracketStart = -1;
            currentLevel = 0;
        }
        public void JumpToEnd()
        {
            currentPos = length;
            bracketStart = -1;
            currentLevel = 0;
        }

        public long GetCurrentPosition()
        {
            return currentPos;
        }

        public void EnableNamespaceRemoving(bool flag)
        {
            enableNamespaceRemoving = flag;
        }


        public char[] buffer;
        public long length;
        public long currentPos;
        public long bracketStart;
        public long currentLevel;
        public bool enableNamespaceRemoving;

        /// <summary>Assume within our out of brackets.</summary>
        /// <param name="flag">The assumed state.</param>
        public void AssumeBrackets(bool flag)
        {
            if (flag && bracketStart == -1)
            {
                //ASSERT(false, "Out of brackets!");
            }
            else if (!flag && bracketStart != -1)
            {
                //ASSERT(false, "Within brackets");
            }
        }


    }

    public struct AttributeInfo
    {
       public StringPart name;
       public StringPart value;
    }

    public enum TagType
    {
        TT_OPEN,
        TT_CLOSE,
        TT_OPEN_CLOSE,
        TT_COUNT
    }

    public class Helper
    {
        public AttributeParsingState state = AttributeParsingState.APS_READ_NAME;
        public AttributeInfo attribute = new AttributeInfo();
        public int startIndex = 0;
        public int index = 0;
        public readonly StringPart attributes;
        public List<AttributeInfo> targetAttributes;

        public Helper(StringPart _attributes, List<AttributeInfo> _targetAttributes)
        {
            this.attributes = _attributes;
            this.targetAttributes = _targetAttributes;
        }

        public void StartSearchValue()
        {
            attribute.name = attributes.SubString(startIndex, index - startIndex);
            attribute.name.TrimLeft();
            attribute.name.TrimRight();
            state = AttributeParsingState.APS_SEARCH_VALUE_START;
        }

        public void StartReadValue()
        {
            state = AttributeParsingState.APS_READ_VALUE;
            startIndex = index + 1;
        }

        public void StartReadName()
        {
            attribute.value = attributes.SubString(startIndex, index - startIndex);
            attribute.value.TrimLeft();
            attribute.value.TrimRight();
            state = AttributeParsingState.APS_READ_NAME;
            targetAttributes.Add(attribute);
            startIndex = index + 1;
        }

        public void ParseAttributes()
        {
            while (index < attributes.bufferLength)
            {
                char ch = attributes.buffer[index];
                switch (ch)
                {
                    case '=':
                        {
                            if (state == AttributeParsingState.APS_READ_NAME)
                            {
                                StartSearchValue();
                            }
                        }
                        break;
                    case '\"':
                        {
                            switch (state)
                            {
                                case AttributeParsingState.APS_SEARCH_VALUE_START:
                                    StartReadValue();
                                    break;
                                case AttributeParsingState.APS_READ_VALUE:
                                    StartReadName();
                                    break;
                                default:
                                    //ASSERT(false, "Not handled so far!");
                                    break;
                            }
                        }
                        break;
                    case ' ':
                    case '>':
                        if (state == AttributeParsingState.APS_READ_VALUE_WITHOUT)
                        {
                            StartReadName();
                        }
                        break;
                    default:
                        if (state == AttributeParsingState.APS_SEARCH_VALUE_START)
                        {
                            startIndex = index;
                            state = AttributeParsingState.APS_READ_VALUE_WITHOUT;
                        }
                        break;
                }
                index++;
            }
            if (state == AttributeParsingState.APS_READ_VALUE_WITHOUT)
            {
                StartReadName();
            }
        }
    }

    public enum AttributeParsingState
    {
        APS_READ_NAME,
        APS_SEARCH_VALUE_START,
        APS_READ_VALUE,
        APS_READ_VALUE_WITHOUT,
        APS_COUNT
    };


    public struct TagInfo
    {
        public StringPart name;
        public TagType tagType;
        public StringPart attributes;
        public long level;
        public long bracketStart;
        public XmlParser parser;

        public void ParseAttributes(List<AttributeInfo> targetAttributes)
        {
            XmlParser.ParseAttributes(attributes, targetAttributes);
        }

        public bool ParseAttribute(String attributeName, ref AttributeInfo attributeInfo)
        {
            if (attributes.bufferLength == 0)
            {
                return false;
            }

            List<AttributeInfo> targetAttributes = new List<AttributeInfo>();
            ParseAttributes(targetAttributes);
            for (int i = 0; i < targetAttributes.Count; i++)
            {
                if (targetAttributes[i].name.EqualsNoCase(new StringPart(attributeName)))
                {
                    attributeInfo = targetAttributes[i];
                    return true;
                }
            }
            return false;
        }

        XmlParser GetSubParser()
        {
            return null;
        }
        String ParseValue()
        {
            return string.Empty;
        }
    }
}
