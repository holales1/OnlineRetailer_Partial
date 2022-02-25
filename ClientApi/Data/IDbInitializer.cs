namespace ClientApi.Data
{
    public interface IDbInitializer
    {
        void Initialize(ClientApiContext context);
    }
}
