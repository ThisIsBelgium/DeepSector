namespace DeepSector
{
    class Program
    {    
        static void Main(string[] args)
        {
            //asynicing run method
            Bot deepsector = new Bot();
            deepsector.RunBotAsync().GetAwaiter().GetResult();
        }
    }
}
        
