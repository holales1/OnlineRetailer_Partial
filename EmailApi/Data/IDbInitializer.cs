namespace EmailApi.Data
{
    public interface IDbInitializer
    {
        void Initialize(EmailApiContext context);
    }
}
