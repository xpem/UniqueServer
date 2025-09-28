namespace BaseModels
{
    public record BaseResponse
    {
        public bool Success { get; set; } = true;

        public object? Content { get; init; }

        public Error? Error { get; init; }

        public ErrorCode ErrorCode { get; init; }

        public BaseResponse(object? content)
        {
            if (content is ErrorCode) throw new Exception("Retorno de erro deve Ter uma mensagem válida, content:" + content);

            Success = true;
            Content = content;
        }

        public BaseResponse(ErrorCode errorCode, string errorMessage)
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
        #region userErrors
        UserEmailPasswordLoginType = 0,
        GoogleAuthNullEmailOrName = 1,
        InvalidObject = 2,
        TryCreateExistingUser = 3,
        SendEmailError = 4,
        InvalidUserPasswordLogin = 5,
        InvalidPasswordConfirmation = 6,
        #endregion
    }
}