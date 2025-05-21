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
        private static float _dayCount {  get; set; }
        public static void LoadContent(float day)
        {
            _dayCount = day;
        }


        public static float FetchDays()
        {
            return _dayCount;
        }

        public static DayCycleSaveData SaveDayCycle()
        {
            return new DayCycleSaveData()
            {
                Day = _dayCount
            };
        }
    }
}
