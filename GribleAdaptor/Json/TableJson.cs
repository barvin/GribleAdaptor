namespace GribleAdaptor.Json
{
    class TableJson
    {
        public TableType Type { get; set; }
        public string ClassName { get; set; }
        public bool ShowUsage { get; set; }
        public bool ShowWarning { get; set; }
        public Key[] Keys { get; set; }
        public string[][] Values { get; set; }
    }
}
