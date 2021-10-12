namespace EPiServer.Automation.APITestingCore
{
    /// <summary>
    /// A base class for any Helper.
    /// </summary>
    /// <typeparam name="T">The type to initiate the object instance.</typeparam>
    public abstract class HelperBase<T> where T : class, new()
    {
        private static volatile T instance;
        private static readonly object syncRoot = new object();

        /// <summary>
        /// Gets the current singleton instance of this Helper.
        /// </summary>
        public static T Current
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new T();
                        }
                    }
                }
                return instance;
            }
        }
    }
}