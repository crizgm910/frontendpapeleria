namespace PapeleriaDB.Messages
{
    public class ServiciosActualizadosMessage
    {
        public int TotalCount { get; }
        public ServiciosActualizadosMessage(int totalCount)
        {
            TotalCount = totalCount;
        }
    }
}
