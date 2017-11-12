using System;

namespace SchemaExplorer
{
    internal class SqlProductInfo
    {
        public int MajorVersion { get; set; }

        public string ProductVersion { get; set; }

        public string Edition { get; set; }

        internal SqlProductInfo(string productVersion, string edition)
        {
            ProductVersion = productVersion;
            InitMajorVersion(productVersion);
            Edition = edition;
        }

        public bool IsSql2000 { get { return MajorVersion == 8; } }

        public bool IsSql2005OrNewer { get { return MajorVersion >= 9; } }

        public bool IsSqlAzure { get { return Edition.Contains("Azure"); } }

        private void InitMajorVersion(string productVersion)
        {
            MajorVersion = 0;
            if (productVersion.Length > 0)
            {
                int num = productVersion.IndexOf('.');
                if (num > 0)
                    try { MajorVersion = int.Parse(productVersion.Substring(0, num)); }
                    catch (Exception ex)
                    {
                        MajorVersion = 0;
                    }
            }
        }
    }
}
