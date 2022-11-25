using System.Collections.Concurrent;

namespace Emerita
{
    interface ICommand
    {
        public void Execute();
    }

    interface IGoOption
    {
        public UciGoOption Option { get; }

    }
    interface ICommandReceiver
    {
        public IProducerConsumerCollection<ICommand> Commands { get; }
        public IProducerConsumerCollection<string> Responses { get; }
        public void Debug(bool state);
        public void Go();

    }
}