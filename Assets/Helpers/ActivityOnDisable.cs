namespace Inside.Flow
{
    public class ActivityOnDisable : Activity
    {
        private void OnDisable()
        {
            Execute();
        }
    }
}