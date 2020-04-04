using System.Collections.Generic;

namespace Sonovate.CodeTest.Services
{
    public class ApplicationWrapper : IApplicationWrapper
    {
        private static IDictionary<string, string> Settings { get; set; }

        public string this[string key]
        {
            get => Settings == null ? Application.Settings[key] : Settings[key];
            set
            {
                if (Settings == null)
                {
                    Settings = new Dictionary<string, string>();
                }

                Settings[key] = value;
            }
        }
    }

    public interface IApplicationWrapper
    {
        string this[string key] { get; set; }
    }
}
