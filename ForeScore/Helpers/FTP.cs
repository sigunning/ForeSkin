using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;

namespace ForeScore.Helpers
{
    public class FTP
    {

        
        public async Task<FtpStatus> UploadFile(string host, string user, string password, string source, string dest)
        {
            var token = new CancellationToken();
            using (var conn = new FtpClient(host, user, password))
            {

                conn.EncryptionMode = FtpEncryptionMode.Implicit;
                
                conn.ValidateAnyCertificate = true;
                try
                {
                    await conn.ConnectAsync(token);

                    return await conn.UploadFileAsync(source, dest);
                }
                catch (Exception ex)
                {
                    return FtpStatus.Failed;
                }
            }



            

        }
    }
}
