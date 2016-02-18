namespace Logic
{
    public class OutputSettings
    {
        public string Path { get; }
        public int Size {get;}

        public OutputSettings(string path, int size = 1)
        {
            Path = path;
            Size = size;
        }
    }
}