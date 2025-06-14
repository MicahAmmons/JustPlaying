using PlayingAround.Data.SaveData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers.DayManager
{
    public static class DayCycleManager
    {
        private static DayCycleSaveData _dayData;
        private static float _currentDay => _dayData.Day;

        public static void LoadContent(DayCycleSaveData data)
        {
            _dayData = data;
        }


        public static float FetchDays()
        {
            return _currentDay;
        }

        public static DayCycleSaveData SaveDayCycle()
        {
            return _dayData;
        }
    }
}
