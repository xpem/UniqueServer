namespace BaseModels
{
    public record BaseResp
    {
        public bool Success { get; set; } = true;

        public object? Content { get; init; }

        public Error? Error { get; init; }

        public ErrorCode ErrorCode { get; init; }

        public BaseResp(object? content)
        {
            if (content is ErrorCode) throw new Exception("Retorno de erro deve Ter uma mensagem válida, content:" + content);

            Success = true;
            Content = content;
        }

        public BaseResp(ErrorCode errorCode, string errorMessage)
        {
            Success = false;
            Content = null;
            ErrorCode = errorCode;
            Error = new Error { Message = errorMessage };
        }
    }

    public record Error
    {
        public required string Message { get; init; }
    }

    public enum ErrorCode
    {
        UserEmailPasswordLoginType = 0,
        GoogleAuthNullEmailOrName = 1,
        InvalidObject = 2,
        TryCreateExistingUser = 3,
        SendEmailError = 4,
        InvalidUserPasswordLogin = 5,
        InvalidPasswordConfirmation = 6,
        ExistingObject = 7,
        ErrorCreatingObject = 8,
        TryDeleteSystemDefaultObject = 9,
        InvalidId = 10,
        TryDeleteObjectWithDependencies = 11,
        ErrorUpdatingObject = 12,
        ErrorDeletingObject = 13,
        InvalidPage = 14,
        ExistingIndex = 15,
    }
}