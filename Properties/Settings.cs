using System.Configuration;

namespace PapeleriaDB.Properties
{
    internal sealed partial class Settings : ApplicationSettingsBase
    {
        private static Settings defaultInstance = ((Settings)(ApplicationSettingsBase.Synchronized(new Settings())));

        public static Settings Default
        {
            get
            {
                return defaultInstance;
            }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("False")]
        public bool RecordarUsuario
        {
            get
            {
                return ((bool)(this["RecordarUsuario"]));
            }
            set
            {
                this["RecordarUsuario"] = value;
            }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("")]
        public string UsuarioGuardado
        {
            get
            {
                return ((string)(this["UsuarioGuardado"]));
            }
            set
            {
                this["UsuarioGuardado"] = value;
            }
        }
    }
}
