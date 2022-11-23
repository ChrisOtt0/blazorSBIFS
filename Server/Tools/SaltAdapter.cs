namespace blazorSBIFS.Server.Tools
{
    public class SaltAdapter : TextFile
    {
        private readonly string saltPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Documents\\SBIFS\\salt.txt";

        public string GetSalt()
        {
            return base.GetAllTextFromFile(saltPath);
        }
    }
}
