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

            var setTask = CreateDataSetAsync(Name, configuration);
            
            var schemaTask = setTask.ContinueWith((set) =>
            {
                Task.WaitAll(CreateSchemaAsync(set.Result));
            });

            var resultTask = ParseLogsAsync(LogFiles, configuration);

            Console.WriteLine("Waiting for tasks");
            Task.WaitAll(setTask, resultTask, schemaTask);
            Console.WriteLine("Finished waiting");

            Console.WriteLine("Filling DataSet");
            await FillDataSetAsync(setTask.Result, resultTask.Result);

            Console.WriteLine("Saving results to Db.");
            await SaveToDatabaseAsync(setTask.Result);

            Console.WriteLine("Finished saving results to DB.");
        }

        protected async virtual Task SaveToDatabaseAsync(DataSet set)
        {
            await Task.Factory.StartNew(() => SaveToDatabase(set));
        }

        protected async virtual Task FillDataSetAsync(DataSet set, Dictionary<LogPattern, List<KeyValuePair<LogLineContext, List<KeyValuePair<LogPatternMember, string>>>>> parsedResults)
        {
            await Task.Factory.StartNew(() => FillDataSet(set, parsedResults));
        }

        protected async virtual Task<Dictionary<LogPattern, List<KeyValuePair<LogLineContext, List<KeyValuePair<LogPatternMember, string>>>>>> ParseLogsAsync(string[] logFiles, IEnumerable<LogPatternConfiguration> configurations)
        {
            return await Task.Factory.StartNew(() => ParseLogs(logFiles, configurations));
        }

        protected async virtual Task CreateSchemaAsync(DataSet set)
        {
            await Task.Factory.StartNew(() => CreateSchema(set));
        }

        protected async virtual Task<DataSet> CreateDataSetAsync(string name, IEnumerable<LogPatternConfiguration> configuarations)
        {
            return await Task.Factory.StartNew(() => CreateDataSet(name, configuarations));
        }

        protected async virtual Task<IEnumerable<LogPatternConfiguration>> GetLogConfiguraitonsAsync(IEnumerable<string> configurationPaths)
        {
            return await Task.Factory.StartNew(() => GetLogConfiguraitons(configurationPaths));
        }
    }
}
