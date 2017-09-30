namespace DemoService
{
    public delegate void StatusUpdated(string status);

    public interface IDemoService
    {
        event StatusUpdated StatusUpdatedEvent;
        string GetTime(string format);
    }
}
