using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using LogQuery;
using System.Data;
using LogQuery.DataAccess;
using LogQuery.DataAccess.Database;
using LogQuery.DataAccess.Configuration;
using LogQuery.DataAccess.Log;
using LogQuery.Logic;
using LogQuery.Properties;

namespace LogQueryVisualStudio
{
    class Program
    {
        //public const string TestConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\LogDB.mdf;Integrated Security=True";
        public const string TestConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;Integrated Security=True";
        public const string TestDirectory = @"E:\Files\logs\LogQuery\";
        public static readonly string[] TestConfigurations = { @"E:\Files\logs\LogQuery\LogConfiguration.lqc" };
        static void Main(string[] args)
        {
            //AppDomain.CurrentDomain.SetData("DataDirectory", @"D:\DBs");
            TestEngine();
            //ExportImportConfiguration(TestConfigurations.First(), LogConstants.EndOfLine);
            //ExportImportConfiguration(@"D:\LogQuery\LogConfiguration.lqc", LogConstants.EndOfText);
            //TestManuall();

            //var text = System.IO.File.ReadAllText(@"D:\Tinkoff\Test\Snippet.lqc");
            //var text = System.IO.File.ReadLines(@"D:\Tinkoff\Test\Snippet.lqc").ToArray();

            //var lines = text.Split(("\u0003").ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select( l => l.TrimStart());

            Console.WriteLine("Press enter to quit.");
            Console.ReadLine();
        }

        static void TestEngine()
        {
            var allWatch = Stopwatch.StartNew();

            //var engine = new LogQueryEngine("Engine", TestConnectionString, FromDirectory(TestDirectory), TestConfigurations);
            //engine.Start();

            //allWatch.Stop();
            //Console.WriteLine("Finished. Elapsed: {0}", allWatch.Elapsed);

            //allWatch.Restart();
            var parallelEngine = new ParallelLogQueryEngine("Engine", TestConnectionString, FromDirectory(TestDirectory), TestConfigurations);
            parallelEngine.Start();

            allWatch.Stop();
            Console.WriteLine("Finished. Elapsed: {0}", allWatch.Elapsed);
        }

        public static string[] FromDirectory(string directoryName)
        {
            return System.IO.Directory.GetFiles(directoryName).Select(f => new System.IO.FileInfo(f)).OrderBy(info => info.LastWriteTime).Select(info => info.FullName).ToArray();
        }

        static void TestManuall()
        {
            var configurationPath = @"D:\Tinkoff\Test\LogConfiguration.lqc";
            var logsDirectory = @"D:\Tinkoff\Test";

            var allWatch = Stopwatch.StartNew();
            var watch = new Stopwatch();

            var imported = ExportImportConfiguration(configurationPath, LogConstants.EndOfText);

            Console.WriteLine("Creating dataset...");
            watch.Restart();
            var set = CreateDataSet(imported);
            watch.Stop();
            Console.WriteLine("Dataset is created. Elapsed: {0}", watch.Elapsed);

            Console.WriteLine("Generating creation query...");
            watch.Restart();
            var queryGenerator = new SqlServerQueryGenerator();
            var query = queryGenerator.GenerateCreateQuery(outputFileName: @"S:\testDB.mdf", set: set, createTablesIfNotExists: false);
            watch.Stop();
            Console.WriteLine("Query is generated. Elapsed: {0}", watch.Elapsed);

            Console.WriteLine("Creating schema...");
            watch.Restart();
            var driver = new ManualDatabaseDriver(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\LogDB.mdf;Integrated Security=True");
            driver.CretaeSchema(@"S:\testDB.mdf", set);
            watch.Stop();
            Console.WriteLine("Schema is created. Elapsed: {0}", watch.Elapsed);

            var reader = LogReader.FromDirectory(logsDirectory);
            var parser = new LogParser(reader, new[] { imported });

            Console.WriteLine("Parsing log...");
            watch.Restart();
            var parsedResults = parser.ParseLog();
            watch.Stop();
            Console.WriteLine("Log is parsed. Elapsed: {0}", watch.Elapsed);

            Console.WriteLine("Structuring results...");
            watch.Restart();
            FillDataSet(set, parsedResults);
            watch.Stop();
            Console.WriteLine("Results are structured. Elapsed: {0}", watch.Elapsed);

            Console.WriteLine("Saving results to db...");
            watch.Restart();
            driver.Save(set);
            watch.Stop();
            Console.WriteLine("results are saved to db. Elapsed: {0}", watch.Elapsed);

            allWatch.Stop();
            Console.WriteLine("Finished. Elapsed: {0}", allWatch.Elapsed);

            PrintStatistics(set);
        }

        static LogPatternConfiguration ExportImportConfiguration(string path, string delimiter)
        {
            
            var manager = new ConfigurationManager<LogPatternConfiguration>(new ConfigurationXmlSerializer<LogPatternConfiguration>());

            Console.WriteLine("Exporting data...");
            var watch = Stopwatch.StartNew();
            var exported = GetTestPatternConfiguartion(delimiter);
            manager.Export(path, exported);
            watch.Stop();
            Console.WriteLine("Data is exported. Elapsed: {0}", watch.Elapsed);

            Console.WriteLine("Importing data...");
            watch.Restart();
            var imported = manager.Import(path);
            watch.Stop();
            Console.WriteLine("Data is imported. Elapsed: {0}", watch.Elapsed);

            return imported;
        }

        static void PrintStatistics(DataSet set)
        {
            foreach (DataTable table in set.Tables)
            {
                Console.WriteLine("Table {0} with '{1}' rows", table.TableName, table.Rows.Count);
            }
        }

        static void FillDataSet(DataSet set, Dictionary<LogPattern, List<KeyValuePair<LogLineContext, List<KeyValuePair<LogPatternMember, string>>>>> results)
        {
            var uniqueId = 0;

            foreach (var result in results)
            {
                var table = set.Tables[result.Key.Name];

                foreach (var pairs in result.Value)
                {
                    var list = new List<object>(pairs.Value.Select(p => AdaptActualType(p)).ToArray());
                    list.Add(--uniqueId);
                    list.Add(pairs.Key.LogFile);
                    list.Add(pairs.Key.GlobalFileLine);
                    list.Add(pairs.Key.LogFileLine);

                    table.Rows.Add(list.ToArray());
                }
            }
        }

        static object AdaptActualType(KeyValuePair<LogPatternMember, string> pair)
        {
            if (pair.Key.Type.Equals(typeof(int)))
            {
                return int.Parse(pair.Value);
            }
            else if (pair.Key.Type.Equals(typeof(DateTime)))
            {
                return DateTime.ParseExact(pair.Value, "yyyy-MM-dd HH:mm:ss,fff", System.Globalization.CultureInfo.InvariantCulture);
            }
            if (pair.Key.Type.Equals(typeof(string)))
            {
                return pair.Value;
            }
            if (pair.Key.Type.Equals(typeof(long)))
            {
                return long.Parse(pair.Value);
            }
            if (pair.Key.Type.Equals(typeof(bool)))
            {
                return bool.Parse(pair.Value);
            }

            return pair.Value;
        }

        static DataSet CreateDataSet(LogPatternConfiguration configuration)
        {
            DataSet set = new DataSet(configuration.Name);
            set.Namespace = "LogQuery_" + set.DataSetName + "_" + DateTime.Now.ToString().Replace('/', '_').Replace(':', '_').Replace(" ", "_");

            foreach (var pattern in configuration.Patterns)
            {
                var table = set.Tables.Add(pattern.Name);

                table.Namespace = configuration.Name;

                foreach (var member in pattern.Members)
                {
                    var column = table.Columns.Add(member.Name, member.Type);
                    column.AllowDBNull = false;
                }

                var idColumn = table.Columns.Add("__LogQ_ID__", typeof(int));
                idColumn.AllowDBNull = false;
                idColumn.AutoIncrement = true;
                idColumn.Unique = true;

                var fileColumn = table.Columns.Add("__LogQ_File__", typeof(string));
                fileColumn.AllowDBNull = false;
                fileColumn.Unique = false;

                var globalLineColumn = table.Columns.Add("__LogQ_GlobalLineIndex__", typeof(long));
                globalLineColumn.AllowDBNull = false;
                globalLineColumn.Unique = true;

                var lineColumn = table.Columns.Add("__LogQ_LineIndex__", typeof(long));
                lineColumn.AllowDBNull = false;
                lineColumn.Unique = true;
            }

            return set;
        }

        static LogPatternConfiguration GetTestPatternConfiguartion(string delimiter)
        {
            return new LogPatternConfiguration("TestConfiguration", delimiter,

                new[]{

                    new LogPattern(
                        "StartedInteractions",
                        @"([0-9]{4}-[0-9]{2}-[0-9]{2} [0-9]{2}:[0-9]{2}:[0-9]{2},[0-9]{3})\s*\|\s+[a-zA-Z]+\s+\|\s+([0-9a-zA-Z,# \-]+)\s+\|\s+\|\s+\|\s+RTIConnect\s*\|\s+CompleteDetails\.StartSegment: ---Start Segment\. \(segmentID = ([0-9]+), completeID = ([0-9]+)\)",
                        true, true,
                        new []{ 
                            new LogPatternMember(1, "LocalStartTime", typeof(DateTime)), 
                            new LogPatternMember(2, "Thread", typeof(string)), 
                            new LogPatternMember(3, "InteractionID", typeof(long)), 
                            new LogPatternMember(4, "ContactID", typeof(long))
                        }
                    ),
                    new LogPattern(
                        "UpdateAuthenticationLevel",
                        @"([0-9]{4}-[0-9]{2}-[0-9]{2} [0-9]{2}:[0-9]{2}:[0-9]{2},[0-9]{3})\s*\|\s+[a-zA-Z]+\s+\|\s+([0-9a-zA-Z,# \-]+)\s+\|\s+\|\s+\|\s+RTIConnect\s*\|\s+RealTimeClientFacade\.UpdateAuthenticationLevel: --->>\[UpdateAuthenticationLevel\(Level: '([a-zA-Z]+)', info='Complete ID :'([0-9]+)',Interaction ID :'([0-9]+)',SiteID :'[0-9]+''\)\]\[User='UserID: '([0-9]+)'",
                        true, true,
                        new []{ 
                            new LogPatternMember(1, "LocalStartTime", typeof(DateTime)), 
                            new LogPatternMember(2, "Thread", typeof(string)), 
                            new LogPatternMember(3, "Level", typeof(string)), 
                            new LogPatternMember(4, "CompleteID", typeof(long)), 
                            new LogPatternMember(5, "InteractionID", typeof(long)),
                            new LogPatternMember(6, "User", typeof(int))
                        }
                    ),
                    new LogPattern(
                        "StartAuthenticationRequestStarted",
                        @"([0-9]{4}-[0-9]{2}-[0-9]{2} [0-9]{2}:[0-9]{2}:[0-9]{2},[0-9]{3})\s*\|\s+[a-zA-Z]+\s+\|\s+([0-9a-zA-Z,# \-]+)\s+\|\s+\|\s+\|\s+RTIConnect\s*\|\s+Wrapper\.FacadeMethod:\s+-+>>\[WCFClientFacade\.StartAuthentication\]\(Args: \[User:\s+'([0-9]+)'\], CustomerID\s+=\s+'(-?[0-9]+)', InteractionInfo = 'Complete ID\s*:\s*'([0-9]+)',\s*Interaction ID\s*:\s*'([0-9]+)'",
                        true, true,
                        new []{ 
                            new LogPatternMember(1, "LocalStartTime", typeof(DateTime)), 
                            new LogPatternMember(2, "Thread", typeof(string)), 
                            new LogPatternMember(3, "User", typeof(int)),
                            new LogPatternMember(4, "CustomerID", typeof(long)), 
                            new LogPatternMember(5, "CompleteID", typeof(long)), 
                            new LogPatternMember(6, "InteractionID", typeof(long))
                        }
                    ),
                    new LogPattern(
                        "StartAuthenticationRequestEnded",
                        @"([0-9]{4}-[0-9]{2}-[0-9]{2} [0-9]{2}:[0-9]{2}:[0-9]{2},[0-9]{3})\s*\|\s+[a-zA-Z]+\s+\|\s+([0-9a-zA-Z,# \-]+)\s+\|\s+\|\s+\|\s+RTIConnect\s*\|\s+RealTimeClientFacade\.StartAuthentication: ---<<\[StartAuthentication\(info='Complete ID :'([0-9]+)',Interaction ID :'([0-9]+)',SiteID :'[0-9]+'', CustomerId='([0-9]+)'\)\]\[User='UserID: '([0-9]+)'",
                        true, true,
                        new []{ 
                            new LogPatternMember(1, "LocalStartTime", typeof(DateTime)), 
                            new LogPatternMember(2, "Thread", typeof(string)), 
                            new LogPatternMember(3, "CompleteID", typeof(long)), 
                            new LogPatternMember(4, "InteractionID", typeof(long)),
                            new LogPatternMember(5, "CustomerID", typeof(long)), 
                            new LogPatternMember(6, "User", typeof(int))
                        }
                    )
                }
            );
        }
    }
}
