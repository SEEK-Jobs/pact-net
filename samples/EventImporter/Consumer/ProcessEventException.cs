using System;

namespace Consumer
{
    public class ProcessEventException : Exception
    {
        public ProcessEventException() : base("An error occurred during the processing of the events")
        {
        }
    }
}
