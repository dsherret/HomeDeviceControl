namespace LightControl.Communication.Common
{
    /// <summary>
    /// Used for sending a primitive via json.
    /// </summary>
    public class ValueObject<T>
    {
        public ValueObject(T value = default)
        {
            Value = value;
        }

        public T Value { get; set; }
    }
}
