using System;

namespace LightControl.Core.Utils
{
    public static class ActionUtils
    {
        /// <summary>
        /// Attempts to do an action and logs an exception if it fails without throwing the exception.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="message">Optional message to log.</param>
        public static void TryDoAction(Action action, string message = null)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Logger.Log(typeof(ActionUtils), LogLevel.Error, message, ex);
            }
        }
    }
}
