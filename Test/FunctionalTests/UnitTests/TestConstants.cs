using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using UnitTests.GrainInterfaces;

namespace UnitTests
{
    internal static class TestConstants
    {
        public static readonly SafeRandom random = new SafeRandom();

        public static readonly TimeSpan InitTimeout =
            Debugger.IsAttached ? TimeSpan.FromMinutes(10) : TimeSpan.FromMinutes(1);

        private const string DataConnectionString0 =
            "UseDevelopmentStorage=true";
        private const string DataConnectionString1 =
             "UseDevelopmentStorage=true";
        private const string DataConnectionString2 =
             "UseDevelopmentStorage=true";
        private const string DataConnectionString3 =
             "UseDevelopmentStorage=true";

        private static readonly string[] DataConnectionStrings =
        {
            DataConnectionString0,
            DataConnectionString1,
            DataConnectionString2,
            DataConnectionString3
        };
        private static readonly int ConnectionToUse = random.Next(DataConnectionStrings.Length);

        public static string DataConnectionString { get { return DataConnectionStrings[ConnectionToUse]; } }

        private const string SimpleGrainNamePrefix = "UnitTests.Grains.SimpleG";

        public static ISimpleGrain GetSimpleGrain(IGrainFactory grainFactory)
        {
            return grainFactory.GetGrain<ISimpleGrain>(random.Next(), SimpleGrainNamePrefix);
        }

        public static ISimpleGrain GetSimpleGrain(IGrainFactory grainFactory, long grainId)
        {
            return grainFactory.GetGrain<ISimpleGrain>(grainId, SimpleGrainNamePrefix);
        }

        public static FileInfo GetDbFileLocation(DirectoryInfo dbDir)
        {
            return GetDbFileLocation(dbDir, "TestDb.mdf");
        }
        public static FileInfo GetDbFileLocation(DirectoryInfo dbDir, string dbFileName)
        {
            if (!dbDir.Exists) throw new FileNotFoundException("DB directory " + dbDir.FullName + " does not exist.");

            string dbDirPath = dbDir.FullName;
            FileInfo dbFile = new FileInfo(Path.Combine(dbDirPath, "TestDb.mdf"));
            Console.WriteLine("DB file location = {0}", dbFile.FullName);

            // Make sure we can write to local copy of the DB file.
            MakeDbFileWriteable(dbFile);

            return dbFile;
        }

        public static string GetSqlConnectionString(TestContext context)
        {
            string dbFileName = @"TestDb.mdf";
            string dbDirPath =
#if DEBUG
                // TestRunDirectory=C:\Depot\Orleans\Code\Main\OrleansV4\TestResults\Deploy_jthelin 2014-08-17 11_53_46
                Path.Combine(context.TestRunDirectory, @"..\..\UnitTests\Data");
#else
                context.DeploymentDirectory;
#endif
            return GetSqlConnectionString(new DirectoryInfo(dbDirPath), dbFileName);
        }
        public static string GetSqlConnectionString(DirectoryInfo dbDir, string dbFileName)
        {
            FileInfo dbFile = GetDbFileLocation(dbDir, dbFileName);

            Console.WriteLine("DB directory = {0}", dbDir.FullName);
            Console.WriteLine("DB file = {0}", dbFile.FullName);

            string connectionString = string.Format(
                @"Data Source=(LocalDB)\v11.0;"
                + @"AttachDbFilename={0};"
                + @"Integrated Security=True;"
                + @"Connect Timeout=30",
                dbFile.FullName);

            Console.WriteLine("SQL Connection String = {0}", ConfigUtilities.RedactConnectionStringInfo(connectionString));
            return connectionString;
        }

        private static void MakeDbFileWriteable(FileInfo dbFile)
        {
            // Make sure we can write to the directory containing the DB file.
            FileInfo dbDirFile = new FileInfo(dbFile.Directory.FullName);
            if (dbDirFile.IsReadOnly)
            {
                Console.WriteLine("Making writeable directory containing DB file {0}", dbDirFile.FullName);
                dbDirFile.IsReadOnly = false;
            }
            else
            {
                Console.WriteLine("Directory containing DB file is writeable {0}", dbDirFile.FullName);
            }

            // Make sure we can write to local copy of the DB file.
            if (dbFile.IsReadOnly)
            {
                Console.WriteLine("Making writeable DB file {0}", dbFile.FullName);
                dbFile.IsReadOnly = false;
            }
            else
            {
                Console.WriteLine("DB file is writeable {0}", dbFile.FullName);
            }
        }

        internal static string DumpTestContext(TestContext context)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(@"TestName={0}", context.TestName).AppendLine();
            sb.AppendFormat(@"FullyQualifiedTestClassName={0}", context.FullyQualifiedTestClassName).AppendLine();
            sb.AppendFormat(@"CurrentTestOutcome={0}", context.CurrentTestOutcome).AppendLine();
            sb.AppendFormat(@"DeploymentDirectory={0}", context.DeploymentDirectory).AppendLine();
            sb.AppendFormat(@"TestRunDirectory={0}", context.TestRunDirectory).AppendLine();
            sb.AppendFormat(@"TestResultsDirectory={0}", context.TestResultsDirectory).AppendLine();
            sb.AppendFormat(@"ResultsDirectory={0}", context.ResultsDirectory).AppendLine();
            sb.AppendFormat(@"TestRunResultsDirectory={0}", context.TestRunResultsDirectory).AppendLine();
            sb.AppendFormat(@"Properties=[ ");
            foreach (var key in context.Properties.Keys)
            {
                sb.AppendFormat(@"{0}={1} ", key, context.Properties[key]);
            }
            sb.AppendFormat(@" ]").AppendLine();
            return sb.ToString();
        }
    }
}
