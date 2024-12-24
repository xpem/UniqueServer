namespace BaseModels.Configs
{
    public record SendEmailKeys(string host, string senderEmail, string senderPassword, string url);

    public record EncryptKeys(string passwordHash, string saltKey, string viKey);

}
