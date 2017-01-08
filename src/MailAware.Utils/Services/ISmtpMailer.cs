using System.Threading.Tasks;

namespace MailAware.Utils.Services
{
    public interface ISmtpMailer
    {
        #region Properties
        
        bool IgnoreSslCertificates { get; set; }
        
        #endregion
        
        Task<bool> ConnectAsync(string hostName, int hostPort = 0, string username = null,
            string password = null);

        Task<bool> ConnectAsync(string hostName, string username = null, string password = null);

        Task<bool> SendMessageAsync(string subject, string body, string fromAddress, string[] recipients);

        Task<bool> DisconnectAsync();
    }
}
