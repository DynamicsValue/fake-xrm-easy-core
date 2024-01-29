namespace FakeXrmEasy.Core.CommercialLicense
{
    /// <summary>
    /// Returns info about the current user
    /// </summary>
    public interface IUserReader
    {
        /// <summary>
        /// Gets the current username
        /// </summary>
        /// <returns></returns>
        string GetCurrentUserName();
    }
    
    internal class UserReader: IUserReader
    {
        public string GetCurrentUserName()
        {
            return System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        }
    }
}