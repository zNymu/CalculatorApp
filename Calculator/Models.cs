using System;
using System.IO;
using System.Xml.Serialization;

namespace CalculatorApp.Models
{
    public class CalculatorSettings
    {
        public bool IsDigitGroupingEnabled { get; set; } = true;
        public bool IsStandardMode { get; set; } = true;
        public int CurrentBase { get; set; } = 10;
        public bool UseOperationOrder { get; set; } = false;


        private static readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CalculatorApp",
            "settings.xml");

        public static CalculatorSettings Load()
        {
            try
            {
                string directory = Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (File.Exists(SettingsFilePath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(CalculatorSettings));
                    using (FileStream stream = new FileStream(SettingsFilePath, FileMode.Open))
                    {
                        return (CalculatorSettings)serializer.Deserialize(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }

            return new CalculatorSettings();
        }

        public void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                XmlSerializer serializer = new XmlSerializer(typeof(CalculatorSettings));
                using (FileStream stream = new FileStream(SettingsFilePath, FileMode.Create))
                {
                    serializer.Serialize(stream, this);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }
    }
}