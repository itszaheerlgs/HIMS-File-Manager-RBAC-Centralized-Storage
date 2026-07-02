namespace UPLOADER
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Load persisted config (or defaults) before any DB call
            DbConfig.Current = AppConfig.Load();

            var login = new LoginForm();
            if (login.ShowDialog() == DialogResult.OK && login.LoggedInUser != null)
            {
                Application.Run(new FileManagerForm(login.LoggedInUser));
            }
        }
    }
}
