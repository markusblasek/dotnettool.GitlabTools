namespace GitLabTools.Commandline;
public interface IValidatableCommandlineArgument
{
    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="ArgumentValidationException">will be thrown if validation fails</exception>
    public void Validate();
}
