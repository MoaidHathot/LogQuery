using System;
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

        private RegexOptions GenerateRegexOptions(LogPattern pattern)
        {
            var options = RegexOptions.None;

            if (!pattern.IsCaseSensitive)
            {
                options |= RegexOptions.IgnoreCase;
            }

            options |= (pattern.IsSingleLine ? RegexOptions.Singleline : RegexOptions.Multiline);

            return options;
        }

        public Dictionary<LogPattern, List<KeyValuePair<LogLineContext, List<KeyValuePair<LogPatternMember, string>>>>> ParseLog()
        {
            var results = new Dictionary<LogPattern, List<KeyValuePair<LogLineContext, List<KeyValuePair<LogPatternMember, string>>>>>();

            foreach (var line in _logReader.Lines)
            {
                foreach (var pattern in _patternConfiguration.Patterns)
                {
                    var options = GenerateRegexOptions(pattern);

                    var match = Regex.Match(line.Message, pattern.RegularExpression, options);

                    if (match.Success)
                    {
                        if (!results.ContainsKey(pattern))
                        {
                            results[pattern] = new List<KeyValuePair<LogLineContext, List<KeyValuePair<LogPatternMember, string>>>>();
                        }

                        var list = results[pattern];

                        var row = new KeyValuePair<LogLineContext, List<KeyValuePair<LogPatternMember, string>>>(line, new List<KeyValuePair<LogPatternMember, string>>());

                        foreach (var member in pattern.Members)
                        {
                            row.Value.Add(new KeyValuePair<LogPatternMember, string>(member, match.Groups[member.Index].Value));
                            //row.Add(new KeyValuePair<LogPatternMember, string>(member, match.Groups[member.Index].Value));
                        }

                        list.Add(row);
                    }
                }
            }

            return results;
        }
    }
}
