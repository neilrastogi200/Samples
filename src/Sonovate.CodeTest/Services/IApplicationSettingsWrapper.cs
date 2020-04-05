namespace Sonovate.CodeTest.Services
{
    public interface IApplicationSettingsWrapper
    {
        string this[string key] { get; set; }
    }
}