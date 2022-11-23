namespace blazorSBIFS.Server.Tools
{
    public class TextFile : FileAdapter
    {
        public override string GetAllTextFromFile(string path)
        {
            return File.ReadAllText(path);
        }

        public override void WriteTextToFile(string path, string text)
        {
            File.WriteAllText(path, text);
        }

        public override void AppendTextToFile(string path, string text)
        {
            File.AppendAllText(path, text);
        }
    }
}
