namespace Checkers
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            AppSession.Music.ApplySettings(AppSession.Settings);
            Application.ApplicationExit += (_, _) => AppSession.Music.Dispose();
            Application.Run(new Form1());
        }
    }
}