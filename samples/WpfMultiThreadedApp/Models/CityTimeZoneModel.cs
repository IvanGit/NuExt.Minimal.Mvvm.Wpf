using Minimal.Mvvm;

namespace WpfMultiThreadedApp.Models
{
    public partial class CityTimeZoneModel : BindableBase
    {
        public CityTimeZoneModel() 
        {
        }

        [Notify]
        private string _name = null!;

        [Notify]
        private DateTime _localTime;
    }
}
