namespace SQLServerObjectMaker
{
    public class Log
    {
        private string log = string.Empty;
        internal void AppendLine(string text) => log += $"\n{text}";

        public override string ToString() => log;
    }
}
