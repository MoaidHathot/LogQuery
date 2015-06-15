﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LogQuery.DataAccess.Configuration;

namespace LogQuery.DataAccess.Log
{
    public class LogParser
    {
        private ILogReader _logReader;
        private LogPatternConfiguration _patternConfiguration;

        public LogParser(ILogReader reader, LogPatternConfiguration patternConfiguration)
        {
            _logReader = reader;
            _patternConfiguration = patternConfiguration;
        }

        public Dictionary<LogPattern, List<List<KeyValuePair<LogPatternMember, string>>>> ParseLog()
        {
            var results = new Dictionary<LogPattern, List<List<KeyValuePair<LogPatternMember, string>>>>();

            foreach (var line in _logReader.Lines)
            {
                foreach (var pattern in _patternConfiguration.Patterns)
                {
                    var match = Regex.Match(line, pattern.RegularExpression);

                    if (match.Success)
                    {
                        if (!results.ContainsKey(pattern))
                        {
                            results[pattern] = new List<List<KeyValuePair<LogPatternMember, string>>>();
                        }

                        var list = results[pattern];

                        var row = new List<KeyValuePair<LogPatternMember, string>>();

                        foreach (var member in pattern.Members)
                        {
                            row.Add(new KeyValuePair<LogPatternMember, string>(member, match.Groups[member.Index].Value));
                        }

                        list.Add(row);
                    }
                }
            }

            return results;
        }
    }
}
