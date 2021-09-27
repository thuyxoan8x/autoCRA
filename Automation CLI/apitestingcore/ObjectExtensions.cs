namespace EPiServer.Automation.APITestingCore
{
    /// <summary>
    /// Extensions for object
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Get value by property
        /// </summary>
        /// <typeparam name="T">type of returned value</typeparam>
        /// <param name="obj">object calls this method</param>
        /// <param name="propertyName">property name</param>
        /// <returns>value with proper type</returns>
        public static T GetValue<T>(this object obj, string propertyName)
        {
            var value = obj.GetType().GetProperty(propertyName).GetValue(obj, null);
            return (T)value;
        }

        /// <summary>
        /// Set value for property
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="obj">object calls this method</param>
        /// <param name="propertyName">property name</param>
        /// <param name="value">value</param>
        public static void SetValue<T>(this object obj, string propertyName, T value)
        {
            obj.GetType().GetProperty(propertyName).SetValue(obj, value);
        }
    }
}
