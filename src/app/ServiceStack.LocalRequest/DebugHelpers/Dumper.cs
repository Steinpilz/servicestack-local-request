using ServiceStack.LocalRequest.Contracts;
using System;
using System.Text;

namespace ServiceStack.LocalRequest.Debug
{
    abstract class Dumper
    {
        private readonly SimpleHttpRequest request;
        private StringBuilder output;

        protected int TabSize = 2;

        int tabLevel = 0;

        public Dumper()
        {
            this.output = new StringBuilder();
        }

        public string Dump()
        {
            this.output.Clear();

            DumpImpl();

            return this.output.ToString();
        }

        protected abstract void DumpImpl();

        protected void Tab(Action action)
        {
            tabLevel++;
            action();
            tabLevel--;
        }

        protected void AppendLine(string line)
        {
            this.output.Append(new string(' ', TabSize * tabLevel));
            this.output.AppendLine(line);
        }
    }
}
