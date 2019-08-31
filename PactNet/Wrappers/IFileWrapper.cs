namespace PactNet.Wrappers
{
    internal interface IFileWrapper
    {
        void Delete(string path);
        bool Exists(string path);
        string ReadAllText(string path);
        void WriteAllText(string path, string contents);
    }
}
