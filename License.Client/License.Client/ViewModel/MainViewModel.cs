using GalaSoft.MvvmLight;
using License.Base;
using License.Client.Model;
using System;
using System.Windows;
using System.Xml.Serialization;

namespace License.Client.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;

        /// <summary>
        /// The <see cref="WelcomeTitle" /> property's name.
        /// </summary>
        public const string WelcomeTitlePropertyName = "WelcomeTitle";

        private string _welcomeTitle = string.Empty;

        /// <summary>
        /// Gets the WelcomeTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WelcomeTitle
        {
            get
            {
                return _welcomeTitle;
            }

            set
            {
                if (_welcomeTitle == value)
                {
                    return;
                }

                _welcomeTitle = value;
                RaisePropertyChanged(WelcomeTitlePropertyName);
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            try
            {
                License.Base.License license1 = GetLicense(@".\license.lic");
                License.Base.License license = License.Base.License.GetLicense();
            } catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
                Environment.Exit(0);
            }

            _dataService = dataService;
            _dataService.GetData(
                (item, error) =>
                {
                    if (error != null)
                    {
                        // Report error here
                        return;
                    }

                    WelcomeTitle = item.Title;
                });
            
        }

        public License.Base.License GetLicense(string licenseFile)
        {
            License.Base.License result;
            try
            {
                if (!System.IO.File.Exists(licenseFile))
                {
                    throw new LicenseInvalidException("没有找到程序授权信息,请联系开发者!");
                }
                Security security = new Security();
                string encodedString = System.IO.File.ReadAllText(licenseFile);
                string s = security.DecodeString(encodedString);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(License.Base.License));
                License.Base.License license = (License.Base.License)xmlSerializer.Deserialize(new System.IO.StringReader(s));
                if (!this.VerifyLicense(license))
                {
                    throw new LicenseInvalidException("许可无效,License Invalid!");
                }
                result = license;
            } catch
            {
                throw new LicenseInvalidException("许可无效,License Invalid!");
            }
            return result;
        }
        private bool VerifyLicense(License.Base.License lic)
        {
            Security security = new Security();
            string hash = security.GetHash(lic.GetHashString());
            bool flag = security.SignatureDeformatter(hash, lic.Signature);
            return flag;
        }
        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}