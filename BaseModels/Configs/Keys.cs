using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseModels.Configs
{
    public record SendEmailKeys(string host, string senderEmail, string senderPassword, string url);

    public record EncryptKeys(string passwordHash, string saltKey, string viKey);

}
