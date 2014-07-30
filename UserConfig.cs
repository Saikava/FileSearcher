using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;



namespace FileSearcher
{
    public class UserConfigData
    {
        private Int32 m_locationX = 100;
        private Int32 m_locationY = 100;
        private Int32 m_width = 528;
        private Int32 m_height = 551;

        private String m_searchDir = "C:\\";
        private Boolean m_includeSubDirsChecked = true;
        private String m_fileName = "*.*";
        private DateTime m_newerThanDateTime = new DateTime(2012, 1, 1, 0, 0, 0);
        private DateTime m_olderThanDateTime = new DateTime(2012, 1, 1, 0, 0, 0);
        private String m_containingText = "";
        private Boolean m_asciiChecked = true;
        private String m_delimeter = ";";
        private String m_resultsFilePath = "";

        public Int32 LocationX
        {
            get { return m_locationX; }
            set { m_locationX = value; }
        }

        public Int32 LocationY
        {
            get { return m_locationY; }
            set { m_locationY = value; }
        }

        public Int32 Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        public Int32 Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        public int WindowState { get; set; }


        public String SearchDir
        {
            get { return m_searchDir; }
            set { m_searchDir = value; }
        }

        public Boolean IncludeSubDirsChecked
        {
            get { return m_includeSubDirsChecked; }
            set { m_includeSubDirsChecked = value; }
        }

        public String FileName
        {
            get { return m_fileName; }
            set { m_fileName = value; }
        }

        public bool NewerThanChecked { get; set; }

        public DateTime NewerThanDateTime
        {
            get { return m_newerThanDateTime; }
            set { m_newerThanDateTime = value; }
        }

        public bool OlderThanChecked { get; set; }

        public DateTime OlderThanDateTime
        {
            get { return m_olderThanDateTime; }
            set { m_olderThanDateTime = value; }
        }

        public bool ContainingChecked { get; set; }

        public String ContainingText
        {
            get { return m_containingText; }
            set { m_containingText = value; }
        }

        public Boolean AsciiChecked
        {
            get { return m_asciiChecked; }
            set { m_asciiChecked = value; }
        }

        public bool UnicodeChecked { get; set; }

        public String Delimeter
        {
            get { return m_delimeter; }
            set { m_delimeter = value; }
        }

        public String ResultsFilePath
        {
            get { return m_resultsFilePath; }
            set { m_resultsFilePath = value; }
        }
    }


    public class UserConfig
    {
        private static readonly String m_path = Path.Combine(Application.UserAppDataPath, "UserConfig.txt");
        private static readonly UserConfigData m_configData = new UserConfigData();

        public static UserConfigData Data
        {
            get { return m_configData; }
        }

        public static Boolean Load()
        {
            var success = false;

            try
            {
                // Zeilen aus der Datei "Config.txt" lesen:
                var lines = new List<String>();
                var fileStream = new FileStream(m_path, FileMode.Open, FileAccess.Read);
                var streamReader = new StreamReader(fileStream);
                while (streamReader.Peek() >= 0)
                {
                    var line = streamReader.ReadLine();
                    lines.Add(line);
                }
                streamReader.Close();
                fileStream.Close();

                // Properties mit Werten belegen:
                var propertyInfos = m_configData.GetType().GetProperties();
                if (propertyInfos.Length == lines.Count)
                {
                    for (var i = 0; i < propertyInfos.Length; i++)
                    {
                        var propertyInfo = propertyInfos[i];
                        var line = lines[i];
                        Object value = null;
                        switch (propertyInfo.PropertyType.Name)
                        {
                            case "String":
                                value = line;
                                break;
                            case "Int32":
                                value = Convert.ToInt32(line, CultureInfo.InvariantCulture);
                                break;
                            case "Boolean":
                                value = Convert.ToBoolean(line, CultureInfo.InvariantCulture);
                                break;
                            case "DateTime":
                                value = Convert.ToDateTime(line, CultureInfo.InvariantCulture);
                                break;
                            default:
                                break;
                        }
                        propertyInfo.SetValue(m_configData, value, null);
                    }

                    success = true;
                }
            }
            catch (Exception)
            {
            }

            return success;
        }

        public static Boolean Save()
        {
            var success = false;

            try
            {
                if (File.Exists(m_path))
                {
                    // Schreibschutz aufheben:
                    var attributes = File.GetAttributes(m_path);
                    if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        var newAttributes = attributes ^ FileAttributes.ReadOnly;
                        File.SetAttributes(m_path, newAttributes);
                    }
                    // Datei löschen:
                    File.Delete(m_path);
                }
                var fileStream = new FileStream(m_path, FileMode.Create, FileAccess.Write);
                var streamWriter = new StreamWriter(fileStream);

                try
                {
                    foreach (var propertyInfo in m_configData.GetType().GetProperties())
                    {
                        var line = "";
                        var obj = propertyInfo.GetValue(m_configData, null);
                        switch (propertyInfo.PropertyType.Name)
                        {
                            case "String":
                                line = (String)obj;
                                break;
                            case "Int32":
                                var i = (Int32)obj;
                                line = i.ToString(CultureInfo.InvariantCulture);
                                break;
                            case "Boolean":
                                var b = (Boolean)obj;
                                line = b.ToString(CultureInfo.InvariantCulture);
                                break;
                            case "DateTime":
                                var dt = (DateTime)obj;
                                line = dt.ToString(CultureInfo.InvariantCulture);
                                break;
                            default:
                                break;
                        }
                        streamWriter.WriteLine(line);
                    }

                    success = true;
                }
                catch (Exception)
                {
                }

                streamWriter.Close();
                fileStream.Close();
            }
            catch (Exception)
            {
            }

            return success;
        }
    }
}
