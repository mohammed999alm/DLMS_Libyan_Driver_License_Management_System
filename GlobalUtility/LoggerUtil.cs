//using System;
//using System.Diagnostics;
//using System.IO;
//using Microsoft.AspNetCore.Mvc;
//using NLog;


//namespace GlobalUtility
//{
//    public static class LoggerUtil
//    {

//        private static string serverPath = Directory.GetCurrentDirectory();

//        private static readonly string logsDirPath = Path.Combine(serverPath, "Logs");

//        private static readonly string logsFilePath = Path.Combine(logsDirPath, "LogFile.txt");

//        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

//        private static readonly Logger dbLogger = LogManager.GetLogger("DatabaseLogger");
//        private static readonly Logger validationLogger = LogManager.GetLogger("ValidationLogger");
//        private static readonly Logger transactionLogger = LogManager.GetLogger("TransactionLogger");

//        static LoggerUtil()
//        {
//            if (!Directory.Exists(logsDirPath)) { Directory.CreateDirectory(logsDirPath); }

//            //if (!File.Exists(logsFilePath)) {  File.Create(logsFilePath); }

//            //var config = new NLog.Config.LoggingConfiguration();

//            //var logFile = new NLog.Targets.FileTarget("logFile") { FileName = logsFilePath };

//            //config.AddRule(LogLevel.Error, LogLevel.Fatal, logFile);


//            //var target = LogManager.Configuration.AllTargets.ToList();


//            //LogManager.Setup().LoadConfigurationFromFile(logsDirPath);


//            //if (LogManager.Configuration != null)
//            //{
//            //    //if (LogManager.Configuration.Variables.ContainsKey("logDir"))
//            //    //{
//            //     Console.WriteLine($"Resolved LogDirectory: {LogManager.Configuration.Variables["logDir"]}");
//            //     LogManager.Configuration.Variables["logDir"] = logsDirPath;

//            //     LogManager.ReconfigExistingLoggers();

//            //     Console.WriteLine($"Resolved LogDirectory: {LogManager.Configuration.Variables["logDir"]}");

//            //    //}
//            //}
//        }
//        public static void SetTheLogMessage(Exception ex, string message)
//        {
//            logger.Error(ex, message);
//        }


//        public static bool IncludeStackTrace = false;
//        public static void LogError(Exception ex, string className, string methodName, Dictionary<string, object>? parameters = null)
//        {
//            var logEvent = new LogEventInfo(LogLevel.Error, logger.Name, $"Exception in {className}.{methodName}");
//            logEvent.Exception = ex;
//            logEvent.Properties["Class"] = className;
//            logEvent.Properties["Method"] = methodName;

//            if (parameters != null)
//            {
//                foreach (var kvp in parameters)
//                {
//                    logEvent.Properties[$"Param_{kvp.Key}"] = kvp.Value?.ToString() ?? "null";
//                }
//            }


//            if (!IncludeStackTrace)
//                logEvent.Exception = new Exception(ex.GetType().Name + ": " + ex.Message);

//            logger.Log(logEvent);
//        }

//        public static void LogValidationWarning(string className, string methodName, string message, Dictionary<string, object>? parameters = null)
//        {
//            var logEvent = new LogEventInfo(LogLevel.Warn, validationLogger.Name, message);
//            logEvent.Properties["Class"] = className;
//            logEvent.Properties["Method"] = methodName;

//            if (parameters != null)
//            {
//                foreach (var kvp in parameters)
//                {
//                    logEvent.Properties[$"Param_{kvp.Key}"] = kvp.Value?.ToString() ?? "null";
//                }
//            }

//            validationLogger.Log(logEvent);
//        }

//    }
//}


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Newtonsoft.Json;

namespace GlobalUtility
{
    public static class LoggerUtil
    {
        private static readonly string serverPath = Directory.GetCurrentDirectory();
        private static readonly string logsDirPath = Path.Combine(serverPath, "Logs");

        private static readonly Logger dbLogger = LogManager.GetLogger("DatabaseLogger");
        private static readonly Logger transactionLogger = LogManager.GetLogger("TransactionLogger");

        public static bool IncludeStackTrace = false;

        static LoggerUtil()
        {
            if (!Directory.Exists(logsDirPath))
                Directory.CreateDirectory(logsDirPath);

            Console.WriteLine($"[{DateTime.Now}] Logger initialized. Logs directory: {logsDirPath}");
        }

        /// <summary>
        /// Logs database exceptions with class, method, and optional parameters.
        /// </summary>
        public static void LogError(Exception ex, string className, string methodName, Dictionary<string, object>? parameters = null)
        {
            var logEvent = new LogEventInfo(LogLevel.Error, dbLogger.Name, $"Exception in {className}.{methodName}");
            logEvent.Exception = IncludeStackTrace ? ex : new Exception($"{ex.GetType().Name}: {ex.Message}");
            logEvent.Properties["Class"] = className;
            logEvent.Properties["Method"] = methodName;

            if (parameters != null)
            {
                foreach (var kvp in parameters)
                {
                    logEvent.Properties[$"Param_{kvp.Key}"] = kvp.Value?.ToString() ?? "null";
                }
            }

            dbLogger.Log(logEvent);
        }

        /// <summary>
        /// Logs a transaction with actor, entity name, and before/after JSON states.
        /// </summary>
        public static void LogTransaction(string username, string entityName, object? oldState, object? newState, string className, string methodName)
        {
            var logEvent = new LogEventInfo(LogLevel.Info, transactionLogger.Name, $"Transaction on {entityName} by {username}");
            logEvent.Properties["Class"] = className;
            logEvent.Properties["Method"] = methodName;
            logEvent.Properties["User"] = username;
            logEvent.Properties["Entity"] = entityName;

            string oldJson = oldState != null ? JsonConvert.SerializeObject(oldState, Formatting.Indented) : "null";
            string newJson = newState != null ? JsonConvert.SerializeObject(newState, Formatting.Indented) : "null";

            logEvent.Message = $"Transaction by user = {username}{Environment.NewLine}" +
                               $"Entity = {entityName}{Environment.NewLine}" +
                               $"Old State =>{Environment.NewLine}{oldJson}{Environment.NewLine}" +
                               $"New State =>{Environment.NewLine}{newJson}";

            transactionLogger.Log(logEvent);
        }

      
    }
}