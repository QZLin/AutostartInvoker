using Microsoft.Win32;
using System.Runtime.InteropServices;

internal class Program
{
    public static class Utils
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool CreateProcess(
            string? lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string? lpCurrentDirectory,
            [In] ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        public static void CreateProcessFromCommandLine(string commandLine)
        {
            var si = new STARTUPINFO();
            var pi = new PROCESS_INFORMATION();

            CreateProcess(
                null,
                commandLine,
                IntPtr.Zero,
                IntPtr.Zero,
                false,
                0,
                IntPtr.Zero,
                null,
                ref si,
                out pi);
        }

        //Refer to autoruns-explorer
        public static Dictionary<string, string> RegRun()
        {
            Dictionary<string, string> runs = new Dictionary<string, string>();
            var run = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            var run2 = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            if (run != null)
                foreach (var name in run.GetValueNames())
                    runs.Add(name, $"{run.GetValue(name)}");
            if (run2 != null)
                foreach (var name in run2.GetValueNames())
                    runs.Add(name, $"{run2.GetValue(name)}");
            return runs;
        }

        public static Dictionary<string, string> AppFolder()
        {
            Dictionary<string, string> runs = new Dictionary<string, string>();
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var runPath = Path.Join(appData, @"Microsoft\Windows\Start Menu\Programs\Startup");
            //Console.WriteLine($"{dir} {runDir}");
            foreach (var fullPath in Directory.GetFiles(runPath, "*.lnk"))
            {
                string filename = fullPath[(fullPath.LastIndexOf('\\') + 1)..];
                //Console.WriteLine($"{filename} {""}");
                runs.Add(filename, fullPath);
            }

            return runs;
        }

        public static void Invoker(string args) => CreateProcessFromCommandLine(args);
    }


    private static void Main(string[] args)
    {
        string match = args.Length < 1 ? "" : args[0];
        Console.WriteLine($">>>{match}");
        var dict = new Dictionary<string, string>();
        var matched = new Dictionary<string, string>();
        foreach (var (key, value) in Utils.RegRun())
            dict.Add(key, value);
        foreach (var (key, value) in Utils.AppFolder())
            dict.Add(key, value);

        foreach (var (key, value) in dict)
        {
            if (key.IndexOf(match, StringComparison.Ordinal) != 0) continue;
            Console.WriteLine($"{key}:{value}");
            matched.Add(key, value);
        }


        if (matched.Count > 1)
            Console.WriteLine($"Matches: {matched.Count}");
        else if (matched.Count == 1)
        {
            foreach (var (key, value) in matched)
            {
                Console.WriteLine($"Invoking [{key}]...");
                Console.WriteLine($"{value}");
                Utils.Invoker(value);
            }
        }
        else if (matched.Count == 0)
            Console.WriteLine("No matched entry found");
    }
}