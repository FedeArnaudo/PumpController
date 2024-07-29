namespace PumpController
{
    internal interface IShowInfo
    {
        string ShowInfo();
    }
    public class ProtocoloInfo : IShowInfo
    {
        public string ShowInfo()
        {
            string info = $"El protocolo indica la cantidad maxima\n" +
                          $"de surtidores que puede tener la estacion.\n" +
                          $"Iniciar con 16, en su defecto, con 32.";
            return info;
        }
    }
    public class IdControladorInfo : IShowInfo
    {
        public string ShowInfo()
        {
            string info = $"La IP se debe consultar con la Estación\n";
            return info;
        }
    }
}
