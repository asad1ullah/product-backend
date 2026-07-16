namespace ProductApi.Exceptions;

public class ImageValidationException : Exception
{
    public ImageValidationException(string message) : base(message)
    {
    }
}
