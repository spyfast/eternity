﻿using System.Text.RegularExpressions;

namespace Eternity.Utils
{
    public class LinksParser
    {
        internal struct TargetData
        {
            public enum TypeTarget
            {
                User,
                Chat,
                Wall,
                Unknown
            }
            public readonly TypeTarget Type;
            public string Id1, Id2;

            public TargetData(TypeTarget type, string id1, string id2)
            {
                Type = type;
                Id1 = id1;
                Id2 = id2;
            }

            public TargetData(TypeTarget type)
            {
                Type = type;
                Id1 = Id2 = null;
            }
        }

        internal static TargetData Parse(string link)
        {
            Match patternChat = new Regex("im\\?sel=c([0-9]+)").Match(link);
            Match patternPm = new Regex("im\\?sel=([0-9]+)").Match(link);
            Match patternWall = new Regex("wall(-?[0-9]+)_([0-9]+)").Match(link);

            if (patternChat.Success)
            {
                return new TargetData(TargetData.TypeTarget.Chat, patternChat.Groups[1].Value, "0");
            }

            if (patternPm.Success)
            {
                return new TargetData(TargetData.TypeTarget.User, patternPm.Groups[1].Value, "0");
            }

            if (patternWall.Success)
            {
                return new TargetData(TargetData.TypeTarget.Wall, patternWall.Groups[1].Value, patternWall.Groups[2].Value);
            }

            return new TargetData(TargetData.TypeTarget.Unknown);
        }
    }
}
