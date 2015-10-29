using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using LogQuery.DataAccess.Log;
using LogQuery.DataAccess.Database;
using LogQuery.DataAccess.Configuration;
using LogQuery.Properties;

namespace LogQuery.Logic
{
    public class ParallelLogQueryEngine : LogQueryEngine
    {
        public ParallelLogQueryEngine(string name, string connectionString, string[] logFiles, string[] logConfiguraitonFiles)
            : base(name, connectionString, logFiles, logConfiguraitonFiles)
        {

        }

        public override void Start()
        {
            try
            {
                Task.WaitAll(StartAsync());
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("AggregateException: '{0}'. Exception: {1}", ex.Message, ex.Flatten());

                foreach (var inner in ex.InnerExceptions)
                {
                    Console.WriteLine("InnerAggregateException: '{0}'. Exception: {1}", inner.Message, inner);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: '{0}'. Exception: {1}", ex.Message, ex);
            }
        }

        public async Task StartAsync()
        {
            var configuration = await GetLogConfiguraitonsAsync(LogConfigurationFiles);

            Console.WriteLine("Finished reading configuration");

            var setTask = CreateDataSetAsync(Name, configuration);

            var schemaTask = setTask.ContinueWith((set) =>
            {
                Console.WriteLine("Finished creating dataset");

                CreateSchema(set.Result);

                Console.WriteLine("Finished Creating Schemas.");
            });
            
            
            //var schemaTastContinuation = schemaTask.ContinueWith((t) => Console.WriteLine("Finished creating schema"));

            var resultTask = ParseLogsAsync(LogFiles, configuration);

            var resultTaskContinuation = resultTask.ContinueWith((t) => Console.WriteLine("Finished parsing"));

            Console.WriteLine("Waiting for tasks");
            Task.WaitAll(setTask, resultTask, schemaTask);
            Console.WriteLine("Finished waiting");

            Console.WriteLine("Filling DataSet");
            await FillDataSetAsync(setTask.Result, resultTask.Result);

            Console.WriteLine("Saving results to Db.");
            await SaveToDatabaseAsync(setTask.Result);

            Console.WriteLine("Finished saving results to DB.");
        }

        protected virtual Task SaveToDatabaseAsync(DataSet set)
        {
            return Task.Factory.StartNew(() => SaveToDatabase(set));
        }

        protected virtual Task FillDataSetAsync(DataSet set, Dictionary<LogPattern, List<KeyValuePair<LogLineContext, List<KeyValuePair<LogPatternMember, string>>>>> parsedResults)
        {
            return Task.Factory.StartNew(() => FillDataSet(set, parsedResults));
        }

        protected virtual Task<Dictionary<LogPattern, List<KeyValuePair<LogLineContext, List<KeyValuePair<LogPatternMember, string>>>>>> ParseLogsAsync(string[] logFiles, IEnumerable<LogPatternConfiguration> configurations)
        {
            return Task.Factory.StartNew(() => ParseLogs(logFiles, configurations));
        }

        protected virtual Task CreateSchemaAsync(DataSet set)
        {
            return Task.Factory.StartNew(() => CreateSchema(set));
        }

        protected virtual Task<DataSet> CreateDataSetAsync(string name, IEnumerable<LogPatternConfiguration> configuarations)
        {
            return Task.Factory.StartNew(() => CreateDataSet(name, configuarations));
        }

        protected virtual Task<IEnumerable<LogPatternConfiguration>> GetLogConfiguraitonsAsync(IEnumerable<string> configurationPaths)
        {
            return Task.Factory.StartNew(() => GetLogConfiguraitons(configurationPaths));
        }
    }
}
