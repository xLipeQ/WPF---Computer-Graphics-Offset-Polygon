using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1
{
    public enum Mode
    {
        Undefined = -1,
        Create = 0,
        Edit = 1,
        Otoczka = 2
    }

    public static class ModeHelpers
    {
        public static Mode StringToWorkState(string str)
        {
            switch(str.ToLower())
            {
                default:
                    return Mode.Undefined;
                case "create":
                    return Mode.Create;
                case "edit":
                    return Mode.Edit;
                case "otoczka":
                    return Mode.Otoczka;
            }
        }

        public static string WorkStateToString(Mode work)
        {
            return work.ToString();
        }
    }
}
