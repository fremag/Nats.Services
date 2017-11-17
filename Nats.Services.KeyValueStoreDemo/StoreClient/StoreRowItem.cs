namespace StoreClient
{
    public interface StoreRowItem<T_Value>
    {
        void Update(T_Value value);
    }
}
