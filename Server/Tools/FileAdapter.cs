namespace blazorSBIFS.Server.Tools
{
    public abstract class FileAdapter
    {
        public abstract string GetAllTextFromFile(string path);
        public abstract void WriteTextToFile(string path, string text);
        public abstract void AppendTextToFile(string path, string text);
    }
}
