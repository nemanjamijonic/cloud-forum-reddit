using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationWorkerRole.HealthHelper
{
    public class LoadAdminEmails
    {
        public static List<string> LoadEmails()
        {
            var adminEmails = new List<string>();
            var relativeFilePath = @"..\..\..\..\..\..\AdminsEmailConfigurator\bin\Debug\AdminEmails.txt";
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = Path.GetFullPath(Path.Combine(baseDirectory, relativeFilePath));

            Trace.TraceInformation($"Base directory: {baseDirectory}");
            Trace.TraceInformation($"Calculated file path: {filePath}");

            if (File.Exists(filePath))
            {
                adminEmails.AddRange(File.ReadAllLines(filePath));
            }
            else
            {
                Trace.TraceWarning($"Admin email file not found: {filePath}");
            }

            return adminEmails;
        }
    }
}
