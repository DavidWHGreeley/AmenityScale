using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;
using Geotool_Objects;
using System.Data.SqlTypes;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Security.Cryptography;
using Geo.Geometries;
using MultiPolygon = Geotool_Objects.MultiPolygon;
using Geo.Measure;
using System.Globalization;
using SharpKml.Dom.GX;
using System.Security.Claims;



////    CHULT! The next level adventure in Fanshawe College's Plant Study! 
///     Vers 1.0     Clay           2023-12-20      Initial Incomplete Just Sketching The idea is we model the whole plant data set.
///                                                 Code word is from fictional jungle setting where all manner of plants live..and pyramids.
///                                                 We require a New U/I.
///          1.1     Clay           2023-12-22      Modelled the initial versions of Conifer and Deciduous. Took a stab at a biome.
///          1.2     Clay           2023-12-26      Refactor day to seek speed and size of file improvements. This means lowering Poly count.
///          2.0     ShakeSpeare!   2024-12-30      10 Days Late but back to Add a custom Calender Class! Integrate an overlay perhaps highlight current garbage pickup zone.

namespace GeoTools_Objects
{
    public class HarptoDay
        {

            public String _YearName;
            public HarptosMonth _Month;
            public DayType _DayType;
            public SelunePhase _Phase;
            public int _DaleReckoningYear;
            public int _DayNumber;
            public List<String> Notes;
            public HarptoDay(HarptosMonth month, int dayNumber, int daleReckoningYear, DayType daytype, String yearName, SelunePhase phase)
            {
                _YearName = yearName;
                _Month = month;
                _DayType = daytype;
                _DaleReckoningYear = daleReckoningYear;
                _DayNumber = dayNumber;
                _Phase = phase;
                Notes = new List<String>();
            }


        public override string ToString()
        {
            String Message = "";
            foreach (String Note in Notes)
            {
                Message += (Note + " ");
            }

            return $"{_DayNumber} {_Month} , {_DaleReckoningYear} DR , Day type: {_DayType}, Notes: {Message} Selune Phase: {_Phase}";
        }
    }

    public enum HarptosMonth
    {
        Hammer = 1,
        Alturiak = 2,
        Ches = 3,
        Tarsakh = 4,
        Mirtul = 5,
        Kythorn = 6,
        Flamerule = 7,
        Eleasis = 8,
        Eleint = 9,
        Marpenoth = 10,
        Uktar = 11,
        Nightal = 12
    }

    public enum SelunePhase
    {
        FullMoon = 1,
        WaxingGibbous = 2,
        FirstQuarter = 3,
        WaxingCrescent = 4,
        NewMoon = 5,
        WaningCrescent = 6,
        LastQuarter = 7,
        WaningGibbous = 8,
       
    }

    public enum DayType
    {
        Regular,
        Midwinter,
        Greengrass,
        Midsummer,
        Shieldmeet,
        Harvestide,
        FeastOfTheMoon
    }

    /// <summary>
    /// It acts as a live calender a running of City Events waste management. 
    /// Taxes collection dates. I am going to use something called a Julian Day Number.
    /// Now things are going to get a bit weird. But this is totally all true.
    /// There are 3 Cycles we need to know about...
    /// 
    /// 1. The Indiction....https://en.wikipedia.org/wiki/Indiction
    ///    Yeah. A 15 year Tax Cycle for Ancient Rome...
    /// 2. The Solar Cycle.....https://en.wikipedia.org/wiki/Solar_cycle_(calendar)
    ///    This cycle is 28 years long! It where the day of the week slips against the number of days in February.
    /// 3. The Lunar Cycle....https://en.wikipedia.org/wiki/Metonic_cycle
    ///    This is a roughly 19 year cycle where the the lunar phase realigns with the date of the calender. The reccurance
    ///    varies because MOONS DON'T CARE ABOUT DAYS. lulz. WHY?! would I show you this? Because JDN as an ABSTRACTION can
    ///    solve many date problems. Astronomers, GIS, history people use JDN as an abstraction.
    ///    
    ///  The Julian perod is a period of 7980 years where all three cycles realign again in phase.
    ///  https://en.wikipedia.org/wiki/Julian_day
    ///  Year 1 of this system is 4713 BC (−4712) ...Chill this abstraction will make things easier.
    ///  So. 4713 BC Noon (Greenwich) 12:00 hrs Monday January 1, 4713 is Day 0. Jan 1, 2000 was day 2,451,545
    ///  I just want to keep track of the day. I don't want any leap days. Holidays, edge cases. This is the universal
    ///  useful calender. Days since some arbitrary time in the past....That lines up nicely for math of
    ///  garbage day vs recyling day vs yard brush day vs compost kitchen scrap day..... 
    /// 
    /// </summary>
    public class CalenderOfHarptos
    {
        private String[] RollOfYears;
        public Dictionary<double, HarptoDay> WheelOfTime { get; private set; }

        private double _JDN;

        /// <summary>
        /// Ripped from https://stackoverflow.com/questions/5248827/convert-datetime-to-julian-date-in-c-sharp-tooadate-safe
        /// </summary>
        /// <param name="Date"></param>
        /// <returns></returns>
        private static double ConvertToJulian(DateTime Date)
        {
            int Month = Date.Month;
            int Day = Date.Day;
            int Year = Date.Year;

            if (Month < 3)
            {
                Month = Month + 12;
                Year = Year - 1;
            }
            double JulianDay = (double)((Day + (153 * Month - 457) / 5 + 365 * Year + (Year / 4) - (Year / 100) + (Year / 400) + 1721119));
            double test = (float)((Date.Minute * 60) + (Date.Second) + (Date.Hour * 3600))/ 86400.00;
            double TimePortion = test-0.5;
            return JulianDay+ TimePortion;
        }

        private SelunePhase CalculatePhase(double jdn)
        {
            //A Cycle is 1461 days long. 
            //Selune Orbits every 30 days 10 hrs and 30 minutes or....30.4375 days
            //So lets mod this to figure where we are in the cycle.
            //The x is the modifier to align the two cycles.
            //    FullMoon = 1,
            //WaxingGibbous = 2,
            //FirstQuarter = 3,
            //WaxingCrescent = 4,
            //NewMoon = 5,
            //WaningCrescent = 6,
            //LastQuarter = 7,
            //WaningGibbous = 8,

            switch (Math.Floor((jdn - 16.9375) % 30.4375))
            {
                case 0:
                    return SelunePhase.FullMoon;
                    break;
                case 1: case 2: case 3:case 4: 
                    return SelunePhase.WaxingGibbous;
                    break;
                 case 5: case 6: case 7: case 8: 
                    return SelunePhase.FirstQuarter;
                    break;
                 case 9:case 10: case 11: case 12:case 13:
                    return SelunePhase.WaxingCrescent;
                    break;
                  case 14: case 15:case 16:case 17:
                    return SelunePhase.NewMoon;
                    break;
                  case 18: case 19: case 20:case 21: case 22:
                    return SelunePhase.WaningCrescent;
                    break;
                   case 23:case 24:case 25:case 26:
                    return SelunePhase.LastQuarter;
                    break;
                   case 27:case 28: case 29: case 30:
                    return SelunePhase.WaningGibbous;

            }
            return SelunePhase.NewMoon;


            //return (SelunePhase)cycle;
        }

        private String CalculateYearName(int year)
        {
            if (year >= -700)
            {
                return RollOfYears[year + 700];
            }
            else
                return "";
          
        }

        public CalenderOfHarptos()
        {
          
            _JDN = ConvertToJulian( DateTime.Now.ToUniversalTime());
              
            BuildRoll();
            BuildCalender();
        }


        public String GetHarptosDay(DateTime d)
        {
            double v = ConvertToJulian(d);

            foreach (var day in WheelOfTime.OrderBy(day => day.Key))
            {
               if(day.Key >= v)
                {
                    return day.Value.ToString();
                }
            }
            return "";
        }
     
        /// <summary>
        /// 1. Shieldmeet occured on year 0 DR. There is a year 0 in this messed up calender. Every four years before and after.
        ///    RollOfYears[700] has that year. There are 2300 years in the roll. 
        /// 2. According to corporate 1492 DR is 2024 AD So I am going to make this moment where we have an alignment.
        ///    Jan 1 2024 12:00 am =  2460310.5000000 JDN = Hammer 1 1492 12:00 am 
        ///    Now I just need to solve the start date alignment
        /// 3. Every year has 365 days except for Shieldmeet years (Leap Year) That means a 4 year cycle of
        ///    4 * 365 + 1 =  1461 Days. Year 0 was a Shieldmeet Year (There IS NO YEAR 0 in our calender!). 
        ///    That means year -700 was a Shieldmeet Year...So was 1492.. 
        ///    (1492/4)*1461 = 544,953 days between Hammer 1 0 DR to Hammer 1 1492 DR
        ///    (700/4)*1461 = 255,675 days between Hammer 1 -700 DR to Hammer 1 0 DR        ///    
        ///    2,460,310.5 - (255,675+544,953) = 1,660,047.5 JDN Remaining.        ///    
        ///     1,136 More cycles back gets us to 
        ///    (1,136)*1461 = 1,659,696 days between Hammer 1 -5244 DR to Hammer 1 -700 DR        ///    
        ///     Hammer 1 -5244 is Day 351.5 JDN
        ///     0 JDN =  Nightal 16 Noon 12:00 hrs -5245 DR = 4713 BC Noon (Greenwich) 12:00 hrs Monday January 1 
        /// </summary>
        private void BuildCalender()
        {
            WheelOfTime = new Dictionary<double, HarptoDay>();

            //How many spins of the 1461 day wheel shall we take?
            int Cycles = (int)Math.Floor(_JDN / 1461);
            //JDN starts at 0 so do we.
            int YearCounter = -5244;
            int MonthCounter = 1;
            int DayCounter = 14;
            float JDN = -0.5f;
            for (int i = 0; i <= Cycles; i++)
            {
//111111111111111111111111111111111111//////////////////////////////////////////////////////////////////////////////////////

                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, 0, YearCounter, DayType.Midwinter, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                JDN++;
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, 0, YearCounter, DayType.Greengrass, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                JDN++;
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, 0, YearCounter, DayType.Midsummer, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                JDN++;
                WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, 0, YearCounter, DayType.Shieldmeet, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                JDN++;
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, 0, YearCounter, DayType.Harvestide, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                JDN++;
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, 0, YearCounter, DayType.FeastOfTheMoon, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                JDN++;
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
//22222222222222222222222222222222//////////////////////////////////////////////////////////////////////////////////////
                DayCounter = 1;
                MonthCounter = 1;
                YearCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, 0, YearCounter, DayType.Midwinter, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                JDN++;
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, 0, YearCounter, DayType.Greengrass, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                JDN++;
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, 0, YearCounter, DayType.Midsummer, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                JDN++;
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, 0, YearCounter, DayType.Harvestide, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                JDN++;
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, 0, YearCounter, DayType.FeastOfTheMoon, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                JDN++;
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
//3333333333333333333333333333333//////////////////////////////////////////////////////////////////////////////////////
                DayCounter = 1;
                MonthCounter = 1;
                YearCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, 0, YearCounter, DayType.Midwinter, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                JDN++;
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, 0, YearCounter, DayType.Greengrass, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                JDN++;
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, 0, YearCounter, DayType.Midsummer, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                JDN++;
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, 0, YearCounter, DayType.Harvestide, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                JDN++;
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, 0, YearCounter, DayType.FeastOfTheMoon, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                JDN++;
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
//444444444444444444444444444444444444444//////////////////////////////////////////////////////////////////////////////////////
                DayCounter = 1;
                MonthCounter = 1;
                YearCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, 0, YearCounter, DayType.Midwinter, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                JDN++;
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, 0, YearCounter, DayType.Greengrass, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                JDN++;
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, 0, YearCounter, DayType.Midsummer, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                JDN++;
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, 0, YearCounter, DayType.Harvestide, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                JDN++;
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, 0, YearCounter, DayType.FeastOfTheMoon, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                JDN++;
                DayCounter = 1;
                MonthCounter++;
                while (DayCounter <= 30)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }
                DayCounter = 1;
                MonthCounter= 1;
                YearCounter++;
                while (DayCounter <= 16)
                {
                    WheelOfTime.Add(JDN, new HarptoDay((HarptosMonth)MonthCounter, DayCounter, YearCounter, DayType.Regular, CalculateYearName(YearCounter), CalculatePhase(JDN)));
                    DayCounter++;
                    JDN++;
                }

            }

            foreach (var item in WheelOfTime)
            {
                if (item.Value._DaleReckoningYear >= -700)
                {
                    if (item.Value._Month == HarptosMonth.Hammer)
                    {



                    }
                    else if (item.Value._Month == HarptosMonth.Alturiak)
                    {



                    }
                    else if (item.Value._Month == HarptosMonth.Ches)
                    { 
                        if (item.Value._DayNumber == 2)
                        {
                            item.Value.Notes.Add("High Tide Festival (Sea Elves)");
                        }
                        if (item.Value._DayNumber == 19)
                        {
                            item.Value.Notes.Add("Spring Equinox");
                        }
                        if (item.Value._DayNumber == 20)
                        {
                            item.Value.Notes.Add("Festival of the Founding (Menzoberranzan)");
                        }
                        if (item.Value._DayNumber == 30)
                        {
                            item.Value.Notes.Add("Fleetswake (Waterdeep)");
                        }

                    }
                    else if(item.Value._Month == HarptosMonth.Tarsakh)
                    {
                        if (item.Value._DayNumber == 4)
                        {
                            item.Value.Notes.Add("The Arming (The Vast)");
                        }
                    }
                    else if (item.Value._Month == HarptosMonth.Mirtul)
                    {
                        if (item.Value._DayNumber == 6)
                        {
                            item.Value.Notes.Add("The Plowing (The Vast)");
                        }
                        if (item.Value._DayNumber == 19)
                        {
                            item.Value.Notes.Add("Festival of Handras(Suzail)");
                        }
                        if (item.Value._DayNumber == 26)
                        {
                            item.Value.Notes.Add("Open Feast (Cormyr)");
                        }
                    }
                    else if (item.Value._Month == HarptosMonth.Kythorn)
                    {
                        if (item.Value._DayNumber == 1)
                        {
                            item.Value.Notes.Add("Trolltide (Waterdeep)");
                        }
                        if (item.Value._DayNumber == 8)
                        {
                            item.Value.Notes.Add("Festival of Remembering (Dwarven)");
                        }
                        if (item.Value._DayNumber == 14)
                        {
                            item.Value.Notes.Add("Hornmoot (The Vast)");
                        }
                        if (item.Value._DayNumber == 20)
                        {
                            item.Value.Notes.Add("Summer Solstice");
                        }
                    }
                    else if (item.Value._Month == HarptosMonth.Flamerule)
                    {
                        if (item.Value._DayNumber == 7)
                        {
                            item.Value.Notes.Add("Lliira's Night (Waterdeep)");
                        }
                        if (item.Value._DayNumber == 14)
                        {
                            item.Value.Notes.Add("Lesser Hornmoot (The Vast)");
                        }
                        
                    }
                    else if (item.Value._Month == HarptosMonth.Eleasis)
                    {
                        if (item.Value._DayNumber == 1)
                        {
                            item.Value.Notes.Add("Ahghairon's Day (Waterdeep)");
                        }
                        if (item.Value._DayNumber == 9)
                        {
                            item.Value.Notes.Add("Festival of Blue Fire (Ormpetarr)");
                        }
                        if (item.Value._DayNumber == 14)
                        {
                            item.Value.Notes.Add("Lesser Hornmoot (The Vast)");
                        }

                    }
                    else if (item.Value._Month == HarptosMonth.Eleint)
                    {
                        if (item.Value._DayNumber == 14)
                        {
                            item.Value.Notes.Add("Final Hornmoot (The Vast)");
                        }
                        if (item.Value._DayNumber == 21)
                        {
                            item.Value.Notes.Add("Autumn Equinox");
                        }

                    }
                    else if (item.Value._Month == HarptosMonth.Marpenoth)
                    {
                        if (item.Value._DayNumber == 15)
                        {
                            item.Value.Notes.Add("Auril's Blesstide (Waterdeep)");
                        }
                        

                    }
                    else if (item.Value._Month == HarptosMonth.Uktar)
                    {
                       


                    }
                    else if (item.Value._Month == HarptosMonth.Nightal)
                    {
                        if (item.Value._DayNumber == 20)
                        {
                            item.Value.Notes.Add("Winter Solstice");
                        }


                    }
                }
            }

        }

        /// <summary>
        /// This function populates an array of strings. It is hard coded. Each year has a name. Blame City Hall.
        /// </summary>
        private void BuildRoll()
        {
            RollOfYears = new String[] {"Year of the Twelve Gods","Year of the Moon Blades Clashing","Year of the Plentiful Herds","Year of the Furious Giants","Year of the Great Rains",
        "Year of the Kings Clashing","Year of the Embattled Ground","Year of the Keening Whales","Year of the Shadowed Traveler","Year of the Stolen Fire",
        "Year of the Fragile Beginnings","Year of the Final Fates","Year of the Fettered Talons","Year of the Eternal Summer","Year of the Fireshadows",
        "Year of the Ill-timed Truth","Year of the Exacted Oaths","Year of the Fleeting Courage","Year of the Humble Heroes","Year of the Nightmares",
        "Year of the Creeping Thieves","Year of the Ebon Scrimshaw","Year of the Hot Springs","Year of the Fervent Glances","Year of the Frenzied Tempests",
        "Year of the Cresting Waves","Year of the Dwarves Besieged","Year of the Hidden Wisdom","Year of the Giant Shoulders","Year of the Crawling Carrion",
        "Year of the Unfurled Sails","Year of the Summer Frosts","Year of the Eagles Striking","Year of the Harrowing Legends","Year of the Hordling Armies",
        "Year of the Crumbling Ruins","Year of the Turning Tides","Year of the Encroaching Weeds","Year of the Ravens","Year of the Heavy Fogs",
        "Year of the Guarding Icon","Year of the Slaying Arrows","Year of the Friends Rejoicing","Year of the Dancing Drakes","Year of the Cool Breezes",
        "Year of the Troubled Nights","Year of the Drifting Sands","Year of the Reverent Threnody","Year of the Joyful Colors","Year of the Stirred Hearts",
        "Year of the Flames Rising","Year of the Falling Copper","Year of the Tribes Reunited","Year of the Craven Hungers","Year of the False Hopes",
        "Year of the Eternal Amber","Year of the Vacant Wharves","Year of the Melodious Minstrels","Year of the Requiem","Year of the Mountain Snows",
        "Year of the Flashing Daggers","Year of the Cruel Rocks","Year of the Nimble Fingers","Year of the Velvet Tongues","Year of the Unending Sorrow",
        "Year of the Surrounding Darkness","Year of the Dwindling Darkness","Year of the Eleven Lords","Year of the Brazen Hetaera","Year of the Mirthful Gnomes",
        "Year of the Orphaned Princes","Year of the Dreaded Comings","Year of the Empty Crowns","Year of the Blinding Lightning","Year of the Oaths Forsaken",
        "Year of the Molten Fury","Year of the Bountiful Harvests","Year of the Clipped Wings","Year of the Drowning Darkness","Year of the Blessed Rulers",
        "Year of the Noble Souls","Year of the Bright Water","Year of the Mired Wagons","Year of the Black Teardrops","Year of the Newfound Spells",
        "Year of the Billowing Dust","Year of the Traveling Musicians","Year of the Stampeding Hordes","Year of the Sudden Mourning","Year of the Power Usurped",
        "Year of the Reluctent Passion","Year of the Six Cats","Year of the Turned Knives","Year of the No Wars","Year of the Valor",
        "Year of the Lingering Nightfall","Year of the Jade","Year of the Held Breaths","Year of the Joys Ephemeral","Year of the Humble Beginnings",
        "Year of the Moonlit Unicorns","Year of the Hasty Messengers","Year of the Quiet Thunder","Year of the Frequent Betrayals","Year of the Dancing Ladies",
        "Year of the Clever Ettin","Year of the Valiant Blades","Year of the Summoned Heroes","Year of the Deep Questions","Year of the Echoing Prayers",
        "Year of the Masking Shadows","Year of the Neverending Storms","Year of the Smiling Infants","Year of the Homecoming","Year of the Grand Offerings",
        "Year of the Rumbling Earth","Year of the Pyramids","Year of the Lingering Mists","Year of the Glittering Coins","Year of the Toil Unending",
        "Year of the Melodies","Year of the Jewels Glittering","Year of the Haste","Year of the Neverending Carnage","Year of the Licentious Behavior",
        "Year of the Spectacular Art","Year of the Hanged Martyrs","Year of the Full Depths","Year of the Fateful Handshakes","Year of theMeager Portions",
        "Year of the Raised Cups","Year of the Silken Sabres","Year of the Iron Golems","Year of the High Harvest","Year of the Greed Regretted",
        "Year of the Dweomers","Year of the Fearful Looks","Year of the Earnest Gallantry","Year of the Dust","Year of the Unknown Fates",
        "Year of the Dreams Attained","Year of the Hidden Passions","Year of the Fading Tides","Year of the Life's Bounty","Year of the Small Gifts",
        "Year of the Furtive Magics","Year of the Dwindling Light","Year of the Plentiful Wine","Year of the Voices Resounding","Year of the Kindred Spirits",
        "Year of the Glistening Dust","Year of the Clammy Tentacles","Year of the Venemous Scribes","Year of the Toppled Trees","Year of the Erstwile Travelers",
        "Year of the Deep Waters","Year of the Leaping Fish","Year of the Merry Jesters","Year of the Peaceful Dawns","Year of the Benevolent Lords",
        "Year of the Stallions","Year of the Harkened Mounts","Year of the Twilight Celebrations","Year of the Lights Lurking","Year of the Mirrored Spirits",
        "Year of the Glistening Dew","Year of the Long Strides","Year of the Runes Abundant","Year of the Flowing Gold","Year of the Deeds Undreamed",
        "Year of the Meager Means","Year of the Imps Amuck","Year of the Wailing","Year of the Hideous Troops","Year of the Returning Wonders",
        "Year of the Golden Chains","Year of the Felled Timber","Year of the Calling Wonders","Year of the Vengeful Scions","Year of the Untethered Yokes",
        "Year of the Vast Diversions","Year of the Lasting Hate","Year of the Defenses Forsaken","Year of the Foiled Pursuits","Year of the Winds Westward",
        "Year of the Masks Divine","Year of the Remembered Friendships","Year of the Great Misunderstandings","Year of the Freedom Granted","Year of the Dangerous Icicles",
        "Year of the Stirring Beauty","Year of the Harrowing Confrontations","Year of the Rattling Chains","Year of the Frozen Breaths","Year of the Deafening Thunder",
        "Year of the Burnings","Year of the Whispering Winds","Year of the Taut Cloths","Year of the Nothingness","Year of the Light Laughter",
        "Year of the Running Unicorns","Year of the Misplaced Trust","Year of the Bloodied Tears","Year of the Shining Beacons","Year of the Hidden Ways",
        "Year of the Rising Fears","Year of the Glowing Raindrops","Year of the Deadly Cost","Year of the Broken Spirit","Year of the Flowering Mint",
        "Year of the Three Griffons","Year of the Endless Song","Year of the Impossible Tasks","Year of the Free Will","Year of the Ringing Music",
        "Year of the Masks","Year of the Driving Rain","Year of the Closed Gates","Year of the Warring Clans","Year of the Wilting Vines",
        "Year of the Tested Faiths","Year of the Enchanted Voyages","Year of the Lost Children","Year of the Legacy","Year of the Tirades",
        "Year of the Glowing Embers","Year of the Everlasting Slime","Year of the Renewed Faith","Year of the Wizards' Woe","Year of the Scorned Prophets",
        "Year of the Perdition's Flame","Year of the Imparted Riches","Year of the Grace","Year of the Emerald Groves","Year of the Clashing Swords",
        "Year of the Lasting Myth","Year of the Merry Wives","Year of the Parting Waters","Year of the Arcane Prisms","Year of the Bold Pioneers",
        "Year of the Silver Warhammers","Year of the Sparkling Spires","Year of the Sea Monsters","Year of the Hostile Badgers","Year of the Rough Hands",
        "Year of the Rebellious Streets","Year of the Pounding Drums","Year of the Fiends Rising","Year of the Gathered Shields","Year of the Graceful Surrender",
        "Year of the Dwarves' Descent","Year of the Dawning Magic","Year of the Lost Souls","Year of the Queries","Year of the Treasured Moments",
        "Year of the Heroic Vindication","Year of the Favors Withheld","Year of the Clamoring","Year of the Kings Arising","Year of the Paths Unwalked",
        "Year of the Scarecrows","Year of the Chilling Laughter","Year of the Dripping Daggers","Year of the Forgotten Passages","Year of the Joyous Tears",
        "Year of the Passion's Fire","Year of the Snowy Beards","Year of the Words Regretted","Year of the Dryads Calling","Year of the Crowded Walks",
        "Year of the Choking Cords","Year of the Fluttering Wings","Year of the Broken Vows","Year of the Breaking Storms","Year of the Drying Wells",
        "Year of the Ancestral Voices","Year of the Bold Faces","Year of the Forsaken Love","Year of the Four Shadows","Year of the Mortal Consequences",
        "Year of the Manacles","Year of the Yellow Musk","Year of the Cowardly Choices","Year of the Harbor's Lights","Year of the Howling Gibbets",
        "Year of the Lost Way","Year of the Saint's Perdition","Year of the Seasons Unsettled","Year of the Weary Kings","Year of the Crimson Marpenoth",
        "Year of the Silver Cestus","Year of the Sailing Winds","Year of the Sleeping Dragons","Year of the Captive Pride","Year of the Insurrection",
        "Year of the Lost Chances","Year of the Lashing Whips","Year of the Lich's Tears","Year of the Zardazil","Year of the Kyrie Arcanaon",
        "Year of the Gilded Sky","Year of the Risen Myths","Year of the Phantoms","Year of the Falling Stones","Year of the Twilit Leaves",
        "Year of the Ashen Faces","Year of the Clouded Vision","Year of the Burning Winds","Year of the Lanterns","Year of the Squalid Scarecrows",
        "Year of the Enchanters","Year of the Steelsong","Year of the Glad Roses","Year of the Shattered Walls","Year of the Smoking Snow",
        "Year of the Medyoxes","Year of the True Gages","Year of the Riven Tankards","Year of the Angered Ankhegs","Year of the Fallen Goats",
        "Year of the Broken Gates","Year of the Dancing Idols","Year of the Ebon Hawks","Year of the Storm Currents","Year of the Sanctuary",
        "Year of the Clutching Dusk","Year of the Shattering","Year of the Whispering Stones","Year of the Owls' Watching","Year of the Bruins",
        "Year of the Rent Armor","Year of the Falling Gold","Year of the Simulacra","Year of the Flaming Manes","Year of the Arumae",
        "Year of the Seven Tines","Year of the Creaking Corbels","Year of the Paladins' Puissance","Year of the Defilers","Year of the Mageserpents",
        "Year of the Autumnfire","Year of the Boiling Moats","Year of the Shuddering Mythals","Year of the Sycophants","Year of the Stale Ale",
        "Year of the Lost Regalia","Year of the Many Maws","Year of the Enchanted Parchment","Year of the Delights Unending","Year of the Dark Roads",
        "Year of the Craven Words","Year of the Bold Poachers","Year of the Moon Madness","Year of the Shivered Staves","Year of the Rhymes",
        "Year of the Good Courage","Year of the Rolling Crowns","Year of the Chilled Marrow","Year of the Sovereign Truth","Year of the Weeping Minstrels",
        "Year of the Desireand Despair","Year of the Sundered Webs","Year of the Guttering Torches","Year of the Broken Charges","Year of the Dark Sunset",
        "Year of the Seven Spirits","Year of the Three Seas' Rage","Year of the Humbling Havens","Year of the Burnished Books","Year of the Shadows Fleeting",
        "Year of the Empty Quests","Year of the Icefire","Year of the Drowned Hopes","Year of the Opened Graves","Year of the Scorched Skulls",
        "Year of the Crown Hatred","Year of the Windriver's Loss","Year of the Unseen Doom","Year of the Plunging Hooves","Year of the Hollow Hills",
        "Year of the Netted Dreams","Year of the Rock Roses","Year of the Crying Wolves","Year of the Vampiric Glee","Year of the Drifting Death",
        "Year of the Vengeance","Year of the Four Princes","Year of the Humility","Year of the Jade Roses","Year of the Patient Traps",
        "Year of the Glassharks","Year of the Many Anchors","Year of the Proud Vixens","Year of the Illuminated Vellum","Year of the Wan Shades",
        "Year of the Bold Bladesmen","Year of the Erupting Crypts","Year of the Dusky Dwarves","Year of the Heirs Adrift","Year of the Wafting Sorrows",
        "Year of the Cold Anger","Year of the Heartsblood","Year of the Nine Watchers","Year of the Scrying Orbs","Year of the Factols",
        "Year of the Harsh Words","Year of the Sundry Violence","Year of the Tyrant Hawks","Year of the Envenomed Nectar","Year of the Setting Suns",
        "Year of the Fiery Slumber","Year of the Spiteful Stones","Year of the Eight Lightnings","Year of the Hard Currency","Year of the Foul Awakenings",
        "Year of the Early Graves","Year of the Lost Faith","Year of the Shattered Portals","Year of the Vampire's Cloak","Year of the Silvered Thoughts",
        "Year of the Impudent Kin","Year of the Gems Aflame","Year of the Ruling Sceptre","Year of the Winking Eye","Year of the Overflowing Casks",
        "Year of the Grinning Pack","Year of the Purring Pard","Year of the Dancing Faun","Year of the Songstones","Year of the Auburn Beauty",
        "Year of the Winter Wolf","Year of the Mournful Monuments","Year of the Sunbright","Year of the Crumbling Caverns","Year of the Hastening Heralds",
        "Year of the Burgeoning Victory","Year of the Scriveners","Year of the Midwives","Year of the Broken Promise","Year of the Lost Princess",
        "Year of the Hunting Hounds","Year of the Pyre","Year of the Many Moons","Year of the Stricken","Year of the Able Warriors",
        "Year of the Furious Waves","Year of the Quartered Foes","Year of the Whirlwinds","Year of the Vindication","Year of the Black Apples",
        "Year of the Imprisonments","Year of the Silver Wings","Year of the Last Songs","Year of the Striking Lance","Year of the Much Cheer",
        "Year of the Entombed Blades","Year of the Fleeting Fancy","Year of the Rack","Year of the Game","Year of the Sand Shroud",
        "Year of the Enslaved Swords","Year of the Whispered Lore","Year of the Fortunes Fair","Year of the Windragons","Year of the Unfriendly Ports",
        "Year of the Laughing Crystal","Year of the Wizards' Doom","Year of the Destinies Foretold","Year of the Forsaken Goblets","Year of the Ringed Moon",
        "Year of the Loss","Year of the Myrmidon Maid","Year of the Unknown Paraph","Year of the Rangers Lost","Year of the Shallows",
        "Year of the Golden Staff","Year of the Forked Tongues","Year of the Burning Briars","Year of the Desiccated Copse","Year of the Shambling Shadows",
        "Year of the Empty Soul","Year of the Sallow Orb","Year of the Dread","Year of the Sumbril","Year of the Bloodied Orphreys",
        "Year of the Oracle","Year of the Vengeful Mitre","Year of the Great Bell","Year of the High Thrones","Year of the Bound Branches",
        "Year of the Holy Hands","Year of the Valiant Striving","Year of the Shattered Havens","Year of the Tolling","Year of the Elfsorrows",
        "Year of the Good Hunting","Year of the Obsidian Crypt","Year of the Purchased Princess","Year of the Returning Heroes","Year of the Fiendish Gambols",
        "Year of the Stonerising","Year of the Flowing Fire","Year of the Beast","Year of the Talons Twelve","Year of the Gilded Draperies",
        "Year of the Chirugeons","Year of the Torment","Year of the Hawkstaff","Year of the Tourneys","Year of the Dark Villainy",
        "Year of the Three Heirs","Year of the Sunned Serpents","Year of the Wrongful Martyrs","Year of the Gilded Burials","Year of the Quiet Horde",
        "Year of the Unburdening","Year of the Abandoned Heart","Year of the Larks","Year of the Sleeping Giants","Year of the Rivers Rising",
        "Year of the Tragedies","Year of the Huntress","Year of the Cold Quarrel","Year of the Leaping Wolves","Year of the Travel",
        "Year of the Shifting Sands","Year of the Bestiary","Year of the Hale Heroes","Year of the Sickles","Year of the Ogres' Rage",
        "Year of the Many Eyes","Year of the Well Women","Year of the Furrowed Brows","Year of the Sudden Kinship","Year of the Seven Loves Lost",
        "Year of the Abandoned Hope","Year of the Passing Dreams","Year of the Yarting","Year of the Boneblight","Year of the Emerald Mage",
        "Year of the Stone Giant","Year of the Burning River","Year of the Deathdolor","Year of the Many Harvests","Year of the Flickering Sun",
        "Year of the Revealed Chants","Year of the Bloody Hazard","Year of the Starry Shroud","Year of the Wildwine","Year of the Roving Bands",
        "Year of the Recompense","Year of the Adamantite Ore","Year of the Black Marble","Year of the Candlemaker","Year of the Cresting Floods",
        "Year of the Depths Unknown","Year of the Copper Kettle","Year of the Bright Lamplight","Year of the Beauty Drowned","Year of the Ancient Coins",
        "Year of the Blind Justice","Year of the Burnt Ambitions","Year of the Damsels Dancing","Year of the Blooded Sunsets","Year of the Ruling Spectre",
        "Year of the Old Beginnings","Year of the Scorpion","Year of the Silent Screams","Year of the Dragons Diving","Year of the Favor",
        "Year of the Fleeting Pleasures","Year of the Gleaming Shards","Year of the Hallowed Hills","Year of the Shackles","Year of the Rainbows Weeping",
        "Year of the Banished Wisdom","Year of the Lessons Learned","Year of the Rich Rewards","Year of the Unleashed Sorrow","Year of the Ample Rewards",
        "Year of the Confusion","Year of the Close Scrutiny","Year of the Elven Delights","Year of the Endless Bounty","Year of the Lost Messengers",
        "Year of the Strident Bards","Year of the Making Merry","Year of the Ill Tidings","Year of the Tortured Dreams","Year of the Terrible Anger",
        "Year of the Shadowed Glances","Year of the Pixies Playing Foul","Year of the Wands","Year of the Tapestries","Year of the Valorous Kobold",
        "Year of the Bloody Goad","Year of the Star Stallion","Year of the Taint","Year of the Spiked Gauntlet","Year of the Flame Noose",
        "Year of the Black Unicorn","Year of the Silver Sharks","Year of the Hearts Pledged","Year of the Amulets","Year of the Eyes Afire",
        "Year of the Wan Shadows","Year of the Many Bats","Year of the Forgotten Smiles","Year of the Honor Broken","Year of the Old Crowns",
        "Year of the Moonlit Mare","Year of the Battered Blades","Year of the Hostile Hails","Year of the Dashed Dreams","Year of the Goodfields",
        "Year of the Smoke and Lightning","Year of the Sloth","Year of the Bitter Fruit","Year of the Witches","Year of the Untaken Paths",
        "Year of the Tempered Blades","Year of the Scarce Steel","Year of the Gleaming","Year of the Flaming Stones","Year of the Lasting Scars",
        "Year of the Leather Shields","Year of the Splendor","Year of the Swift Courtships","Year of the Enchanted Hearts","Year of the Lurking Shadows",
        "Year of the Burnished Bronze","Year of the No Regrets","Year of the Discordant Destinies","Year of the Festivals","Year of the Hunger",
        "Year of the Monstrous Appetites","Year of the Gleaming Frost","Year of the Quicksilver","Year of the Strong Winds","Year of the Thwarted Gambits",
        "Year of the Vain Questing","Year of the Whims","Year of the Smiling Prophet","Year of the False Sentiments","Year of the Neglected Tasks",
        "Year of the Traitorous Thoughts","Year of the Tomes","Year of the Happy Children","Year of the Choking Spores","Year of the Cluttered Desk",
        "Year of the Phandar","Year of the Irreverent Jest","Year of the Emblazoned Dirk","Year of the Angry Centaur","Year of the Pranks and Mischief",
        "Year of the Raging Brook","Year of the Giants' Rage","Year of the Open Sphere","Year of the Shattered Goblet","Year of the Sphinx's Riddles",
        "Year of the Forge's Eldritch Sparks","Year of the Fraudulent Truth","Year of the Insidious Smile","Year of the Patriots","Year of the Abyssal Choir",
        "Year of the Biting Shards","Year of the Maedar","Year of the Harpist's Delight","Year of the Grinning Skull","Year of the Crimson Embrace",
        "Year of the Dying Fear","Year of the Corpulent Mount","Year of the Laconic Prince","Year of the Masquerade","Year of the Five Mountains",
        "Year of the Lost Librams","Year of the Drenched Robe","Year of the Commander's Tent","Year of the Broached Gates","Year of the Sorrow and Pain",
        "Year of the Peace","Year of the Mazes","Year of the Honor's Price","Year of the Eldath's Hart","Year of the Poisoned Pens",
        "Year of the Embrace","Year of the Cloaker","Year of the Bard's Challenge","Year of the Laughing Lovers","Year of the Frightening Turmoil -11)</option>",
        "Year of the Burning Glades","Year of the Fell Traitors","Year of the Wraths","Year of the Open Eyes","Year of the Scarlet Scourges",
        "Year of the Feuds","Year of the Pacts","Year of the Ruins","Year of the Gruesome Streams","Year of the Shattered Relics",
        "Year of the Rising Flame","Year of the Sunrise","Year of the Smiling Hag","Year of the Faded Flower","Year of the Slaked Blade",
        "Year of the Clutched Emerald","Year of the Firestars","Year of the Vampires' Sun","Year of the Spellspheres","Year of the Falling Wall",
        "Year of the Dreams","Year of the Mellifluent Sphinx","Year of the Wistful Looks","Year of the Sweet Songs","Year of the Unknown Beloved",
        "Year of the Glittering Glory","Year of the Distant Thunder","Year of the Crashing Glee","Year of the Lasting Wonders","Year of the Blessed Beast",
        "Year of the Fallen Fury","Year of the Lamenting","Year of the Empty Fist","Year of the Falling Arrows","Year of the Evasive Hare",
        "Year of the Many Runes","Year of the Opening Doors","Year of the Shadowed Blades","Year of the Barren Fields","Year of the Carved Cliffs",
        "Year of the Crushed Monument","Year of the Garrulous Gargoyle","Year of the Errant Arrows","Year of the Slowing Sands","Year of the Purloined Power",
        "Year of the Scaled Lightning","Year of the Consuming Ice","Year of the Dark Venom","Year of the Verdant Pain","Year of the Proud Flame",
        "Year of the Gaunt Wolf","Year of the Branded Lily","Year of the Charging Cavalry","Year of the Frostbrands","Year of the Vow Manifest",
        "Year of the Torpid Arms","Year of the Hidden Fortress","Year of the Crystal Orb","Year of the Bloodied Pikes","Year of the Deadly Joust",
        "Year of the Barbed Wind","Year of the Red Pearls","Year of the Thundering Horde","Year of the Desert Kingdoms","Year of the Forlorn Prince",
        "Year of the Caustic Blood","Year of the Laughing Nightmare","Year of the Lamplit Nights","Year of the Dazzling Dolphins","Year of the Tangled Threads",
        "Year of the Pirate's Lost Eye","Year of the Branded Mage","Year of the Lonely Daughters","Year of the Winsome Trio","Year of the Boisterous Orc",
        "Year of the Foaming Wave","Year of the Spellbound Heir","Year of the Redolent Innkeeper","Year of the Echoing Chasm","Year of the Lit Pathway",
        "Year of the Mournful Dance","Year of the Entwined Sculpture","Year of the Climber's Rest","Year of the Wise Counsel","Year of the Fearful Mercenary",
        "Year of the Clinging Death","Year of the Windsong","Year of the Quivering Mountains","Year of the Closing Darkness","Year of the Whispering Woods",
        "Year of the Mordant Blight","Year of the Vampiric Torch","Year of the Preordained Youth","Year of the Satyr's Adulation","Year of the Monotonous Speech",
        "Year of the Orb Obsidious","Year of the Eyeless Wraith","Year of the Hoar Frost","Year of the Flood","Year of the Faithful Oracle",
        "Year of the Moor Birds","Year of the Scabbard Peace","Year of the Vanished Tattoo","Year of the Quiet Valley","Year of the Multitudes",
        "Year of the Reluctant Hero","Year of the Mournful Harp","Year of the Flickering Nyths","Year of the Revealed Grimoires","Year of the Dragonstar",
        "Year of the Greybeards","Year of the Smiling Moon","Year of the Ormage","Year of the White Hound","Year of the Preying Griffon",
        "Year of the Tattooed Mistress","Year of the Adamantine Spiral","Year of the Fledglings","Year of the Mortified Monk","Year of the False Ghost",
        "Year of the Quirt","Year of the Fallen Guards","Year of the Tusk","Year of the Belching Boggle","Year of the Jagged Leaves",
        "Year of the Morning Glory","Year of the Mortal Promise","Year of the Swinging Pendulum","Year of the Monkey","Year of the Mountain's Fire",
        "Year of the Remembered Pain","Year of the Embroidered Button","Year of the Mourning Armsmen","Year of the Icy Axe","Year of the Biting Frost",
        "Year of the Ironwood","Year of the Cinammon Haze","Year of the Defiant Stone","Year of the Addled Arcanist","Year of the Mummy's Amulet",
        "Year of the Dwarven Twins","Year of the Cockatrice's Stare","Year of the Thirteen Prides Lost","Year of the Arduous Journey","Year of the Impassable Chasm",
        "Year of the Halfling's Dale","Year of the Stalking Beholders","Year of the King's Destiny","Year of the Sparks Flying","Year of the Resolute Courtesans",
        "Year of the Executioner","Year of the Impenetrable Mystery","Year of the Prowling Naga","Year of the Smiling Princess","Year of the Fear and Flame",
        "Year of the Pirates' Port","Year of the Risen Towers","Year of the Iron Colossus","Year of the Blue Ice","Year of the Dwarf",
        "Year of the Lost Library","Year of the Kraken","Year of the Severed Hand","Year of the Wolfstone","Year of the Jealous Hag",
        "Year of the Mellifluous Heaps","Year of the Imaginary Foe","Year of the Resounding Call","Year of the Imploring Widow","Year of the Lost Profit",
        "Year of the Smirking Knaves","Year of the Hangman's Noose","Year of the Great Dwarven Gate","Year of the Screeching Vole","Year of the Fallen Temple",
        "Year of the Smoking Brazier","Year of the Reremouse","Year of the Deep Wellspring","Year of the Scattered Stars","Year of the Weary Warrior",
        "Year of the Bloodties","Year of the Unkind Weapons","Year of the Great Debate","Year of the Screaming Sharn","Year of the Windswept Plains",
        "Year of the Black Boats","Year of the Wyrmclaws","Year of the Troublesome Vixen","Year of the Jealous Spouse","Year of the Engraved Locket",
        "Year of the Leaning Pillars","Year of the Sinking Islands","Year of the Quenched Stirges","Year of the Murmuring Dead","Year of the Leucrotta",
        "Year of the Golden Elephant","Year of the Vestigial Wings","Year of the Twisted Tree","Year of the Dissolute Drow","Year of the Cowled Defender",
        "Year of the Apparition","Year of the Broken Lands","Year of the Magnificent Equine","Year of the Raised Brow","Year of the Coiling Smoke",
        "Year of the Yeti","Year of the Yearning Elves","Year of the Unseeing Priest","Year of the Almond Eyes","Year of the Cold Enchanter",
        "Year of the Leaping Flames","Year of the Student","Year of the Fanged Gauntlet","Year of the Scattered Sands","Year of the Avarice",
        "Year of the Greengrass","Year of the Regal Doppleganger","Year of the Majestic Mace","Year of the Riven Realms","Year of the Dying Eye",
        "Year of the Steelfall","Year of the Spoiled Splendors","Year of the Awakening Magic","Year of the Waking Wraith","Year of the Lost Voices",
        "Year of the Shadowsnare","Year of the Battle Horns","Year of the Giant Skulls","Year of the Dancing Lights","Year of the Old Danger",
        "Year of the Sword Violets","Year of the Melting Manscorpion","Year of the Ghosthunt","Year of the Dark Dreams","Year of the Flaming Forests",
        "Year of the Shattered Skulls","Year of the Empty Turret","Year of the Raised Banner","Year of the Loremasters","Year of the Black Flame",
        "Year of the Wailing Dryads","Year of the Mist Dragon","Year of the Leaping Centaur","Year of the Much Ale","Year of the Bloodflowers",
        "Year of the Drawn Knives","Year of the Plague Clouds","Year of the Disappearing Dragon","Year of the Many Mushrooms","Year of the Wandering Leucrotta",
        "Year of the Chosen","Year of the Hippogriff's Folly","Year of the Hunting Horn","Year of the Sad Orm","Year of the Elfsands",
        "Year of the Dun Dragon","Year of the Sepulchre","Year of the Moaning Maiden","Year of the Tumbletowns","Year of the Crystal Casket",
        "Year of the Storm Crown","Year of the Strange Seedlings","Year of the Bloody Spider","Year of the Somber Smiles","Year of the Ghost Horse",
        "Year of the Magethunder","Year of the Thousand Snows","Year of the Speaking Mountain","Year of the War Wyvern","Year of the Magedirge",
        "Year of the Sunless Stones","Year of the Soaring Stars","Year of the Pages Perilous","Year of the Worn Pages","Year of the Vanishing Cat",
        "Year of the Masterful Plan","Year of the Unspoken Name","Year of the Bane's Shadow","Year of the Cruel Storms","Year of the Wild Roses",
        "Year of the Unheeded Warning","Year of the Port Stormed","Year of the Weeping Kingdom","Year of the Delighted Dwarves","Year of the Vested Vigil",
        "Year of the Wrath Sword","Year of the Burnished Blade","Year of the Broken Flame","Year of the Fallen Banner","Year of the Smiling Nyth",
        "Year of the Secreted Phylactery","Year of the Weeping Flail","Year of the Watchful Hermit","Year of the Skillful Tailor","Year of the Fallen Flagons",
        "Year of the Wasted Pride","Year of the Jolly Mongrels","Year of the Warped Narthex","Year of the Xorn's Yearning","Year of the Waking Dreams",
        "Year of the Full Cribs","Year of the Vintner's Dagger","Year of the Frostfires","Year of the Hounds","Year of the Yak Men",
        "Year of the Wrathful Revenant","Year of the Two Riders","Year of the Wailing Mothers","Year of the Scarred Wagon","Year of the Vaasan Knot",
        "Year of the Late Sun","Year of the Argent Cape","Year of the Deep Bay","Year of the Crimson Tiara","Year of the Questing Raven",
        "Year of the Barrows","Year of the Fanged Horde","Year of the Sundered Sails","Year of the Promise","Year of the Cascade",
        "Year of the Amber Hulk","Year of the Lupine Embrace","Year of the Aurum Bramble","Year of the Regretful Births","Year of the Stammering Apprentice",
        "Year of the Carnivorose","Year of the Vibrant Land","Year of the Riven Shield","Year of the Hero's Lament","Year of the Unforgotten Fire",
        "Year of the Bright Plumage","Year of the Blessed Sleep","Year of the Seven Scales","Year of the Miscast Shadow","Year of the Freedom's Friends",
        "Year of the Silken Whisper","Year of the Secret Slaughters","Year of the Sullen Grimalkin","Year of the Ermine Cloak","Year of the Closed Scroll",
        "Year of the Roused Giants","Year of the Cold Clashes","Year of the Crashing Steeple","Year of the Drawstring","Year of the Humbled Fiend",
        "Year of the Seven Stones","Year of the Whipped Cur","Year of the Chosen's Blade","Year of the Envenomed Bolt","Year of the Vanished Foe",
        "Year of the Uncrossed Bridge","Year of the Black Wing","Year of the Cantobele Stalking","Year of the Fraying Binds","Year of the Loom",
        "Year of the Flying Daggers","Year of the Blushing Stars","Year of the Sage's Fervor","Year of the Dagger","Year of the Toad",
        "Year of the Hunter","Year of the Dancing Deer","Year of the Dancing Piper","Year of the Gold Band","Year of the Fleeting Pains",
        "Year of the Mourning Horns","Year of the Errant Kings","Year of the Pendulum","Year of the Battle Talons","Year of the Awakened Witch",
        "Year of the Dusty Shelf","Year of the Fearless King","Year of the Greedy Altruist","Year of the Opal Key","Year of the Selfless Knave",
        "Year of the Swift Sword","Year of the Molten Anvils","Year of the Shying Eyes","Year of the Bitter Smile","Year of the Maiden's Fancy",
        "Year of the Sleeping Dangers","Year of the Emerald Eyes","Year of the Elder","Year of the Pacifist","Year of the Thoughtful Man",
        "Year of the Woeful Resurrection","Year of the Leaping Hare","Year of the Ghoul","Year of the Autumn Drums","Year of the Seven Stars",
        "Year of the Guarded Stance","Year of the Broken Chalice","Year of the Steel Roses","Year of the Quelzarn","Year of the Dreaming Dragons",
        "Year of the Lady's Gaze","Year of the Dawn Moons","Year of the River Candles","Year of the Simoom","Year of the Wooded Altar",
        "Year of the Half Moon","Year of the Azure Cockatrice","Year of the Firstborn","Year of the Dying Bard","Year of the Herald's Tale",
        "Year of the Narrow Escape","Year of the Purring Tiger","Year of the Swallowing Mists","Year of the Warning Ghost","Year of the Frayed Rope",
        "Year of the Blue Shield","Year of the Serous Fist","Year of the Banished Bard","Year of the Black Dagger","Year of the Withered Flowers",
        "Year of the Blinding Locusts","Year of the Catoblepas","Year of the Ebony Cudgel","Year of the Goblin King","Year of the High Eyes",
        "Year of the Sun Crystal","Year of the Wavering Shadow","Year of the Burning Blazes","Year of the Beholder's Grin","Year of the Omen Stars",
        "Year of the Fiend's Kiss","Year of the Striped Moon","Year of the Brilliant Plan","Year of the Eagle's Flight","Year of the Hale Blacksmith",
        "Year of the Last Breath","Year of the Peerless Foe","Year of the Murderous Mire","Year of the Velvet Night","Year of the Ambitious Sycophant",
        "Year of the Argent Shafts","Year of the Black Dawn","Year of the Violet Fungi","Year of the One's Tears","Year of the Cat's Eye",
        "Year of the Floating Rock","Year of the Hearth","Year of the Sea Princes","Year of the Rebellious Youth","Year of the Stallion Triumphant",
        "Year of the Willing Sacrifice","Year of the Steelscreaming","Year of the Silver Holly","Year of the Bitter Root","Year of the Child's Tear",
        "Year of the Festering Heart","Year of the Haggling Merchant","Year of the Relic's Vigil","Year of the Thousand Enemies","Year of the Infamous Wizard",
        "Year of the Beardless Dwarf","Year of the Unblinking Eye","Year of the Awakening Treant","Year of the Haughty Friend","Year of the Killing Ice",
        "Year of the Corrie Fist","Year of the Unleashed Fears","Year of the Rolling Heads","Year of the Lady's Palace","Year of the Glorious Windfall",
        "Year of the Dryad's Dowry","Year of the Swift Hart","Year of the Unfurled Flag","Year of the Beast's Redemption","Year of the Blooded Dagger",
        "Year of the Scorching Suns","Year of the Lissome Apprentice","Year of the Empty Helm","Year of the Burning Sands","Year of the True Names",
        "Year of the Dawn Blades","Year of the Burnt Spear","Year of the Four Winds","Year of the Bared Sword","Year of the Dusty Library",
        "Year of the Merciful Shadow","Year of the Sundered Tower","Year of the Full Cellars","Year of the Crowned Knave","Year of the Goblin Battles",
        "Year of the Maiden's Tears","Year of the Raging Hunter","Year of the Owlbear","Year of the Coarse Wool","Year of the Forestsfrost",
        "Year of the Winter Sphinx","Year of the Eversharp Axe","Year of the Blighted Vine","Year of the Soaring Galleon","Year of the Lawless Hunt",
        "Year of the Arcane Image","Year of the Bleeding Altar","Year of the Hidden Relics","Year of the Empty Hall","Year of the Foaming Tankard",
        "Year of the Crone's Counsel","Year of the Faltering Fires","Year of the Azure Darkness","Year of the Ecstatic Priest","Year of the Ghost Ship",
        "Year of the Listening Ear","Year of the Ravaging Dragon","Year of the Spear","Year of the Unstrung Bow","Year of the Wager",
        "Year of the Flame Tongue","Year of the Lost Bird","Year of the Crawling Vine","Year of the Galloping Gorgon","Year of the Eclipsed Heart",
        "Year of the Humble Knight","Year of the Opaque Eye","Year of the Rotting Pox","Year of the Stony Terror","Year of the Thunder Lizard",
        "Year of the Unwavering Glare","Year of the Fortress Scoured","Year of the Wyvernfall","Year of the Blood Price","Year of the Elk",
        "Year of the Frivolous Exchange","Year of the Haunting Hawk","Year of the Oaken Glade","Year of the Pendulous Tongues","Year of the Phoenix",
        "Year of the Quiver","Year of the Sea's Beauty","Year of the Unmasked Traitor","Year of the Trials Arcane","Year of the Arcane Cabal",
        "Year of the Cracked Bell","Year of the Besieged Keep","Year of the Tatters","Year of the Burning Sky","Year of the Evening Tree",
        "Year of the Gluttonous Otyugh","Year of the Lily","Year of the Perceptive Judge","Year of the Shattered Manacles","Year of the Spitting Viper",
        "Year of the Upright Man","Year of the Laughing Lich","Year of the Basilisk","Year of the Amethyst Axe","Year of the Borrowed Crown",
        "Year of the Colorful Costume","Year of the Etched Chevron","Year of the Grasping Claw","Year of the Lyre","Year of the Killing Rose",
        "Year of the Pernicon","Year of the Rusted Sabre","Year of the Simpering Courtier","Year of the Eloene Bride","Year of the Unstoppable Ogre",
        "Year of the Zealous","Year of the Barren Chamber","Year of the Dead","Year of the Gnashing Tooth","Year of the Waving Wheat",
        "Year of the Dances Perilous","Year of the Green Man","Year of the Melding","Year of the Mithral Eagle","Year of the Scarlet Dagger",
        "Year of the Unknown Truth","Year of the Three Setting Suns","Year of the Waking Feyr","Year of the Sable Basilisk","Year of the Brandished Axe",
        "Year of the Encrusted Pendant","Year of the Ghasts","Year of the Martyr","Year of the Pernicious Hauberk","Year of the Tumbled Bones",
        "Year of the Bright Fangs","Year of the Scholar","Year of the Writhing Darkness","Year of the Sable Spider","Year of the Gored Griffon",
        "Year of the Breaching Bulette","Year of the Sunless Passage","Year of the Alabaster Mounds","Year of the Floating Fish","Year of the Cultured Rake",
        "Year of the Loose Coins","Year of the Harried Harpies","Year of the Deep Wound","Year of the Furled Sail","Year of the Juggernaut",
        "Year of the Ogling Beholder","Year of the Night's Dying","Year of the Radiant Rods","Year of the Dragons Dawning","Year of the Splintered Oak",
        "Year of the Turning Leaf","Year of the Silver Streams","Year of the Supreme Duelist","Year of the Yellow Locus","Year of the Couched Spear",
        "Year of the Coven","Year of the Flightless Eagle","Year of the Hungry Anelace","Year of the Pauper","Year of the Scourged Fool",
        "Year of the Fire and Frost","Year of the Desolate Warrior","Year of the Glimmering Sea","Year of the Frigid Ghosts","Year of the Immured Imp",
        "Year of the Many Serpents","Year of the Kindly Lich","Year of the Crystal Vambrace","Year of the Failed Daggers","Year of the Old Bones",
        "Year of the Spellfire","Year of the Normiir","Year of the Jester's Smile","Year of the Glaring Eye","Year of the Shattered Scepter",
        "Year of the Lamia's Kiss","Year of the Ensorceled Kings","Year of the Needless Slaughter","Year of the Siege Tower","Year of the Orcsfall",
        "Year of the Mountain Crypts","Year of the Nineteen Swords","Year of the Soaring Shadows","Year of the Nightsilver","Year of the Journey Home",
        "Year of the Torrents","Year of the Eagle and Falcon","Year of the Bloodcrystals","Year of the Kobold Hordes","Year of the Empty Hearth",
        "Year of the Winking Jester","Year of the Lone Lark","Year of the Burning Skies","Year of the Chasms","Year of the Darkspawn",
        "Year of the Soldier's Forfeit","Year of the Luminous Tabard","Year of the Silver Sun","Year of the Menial Phrases","Year of the Ire's Immolation",
        "Year of the Fanged Beast","Year of the Necropolis","Year of the Sifting Sands","Year of the Nesting Harpy","Year of the Gleaming Gates",
        "Year of the Costly Gift","Year of the Tormented Souls","Year of the Wayward Heart","Year of the Dancing Daggers","Year of the Bloody Crown",
        "Year of the Falling Tower","Year of the Waning Sun","Year of the Viper","Year of the Killing Blow","Year of the Coveted Briars",
        "Year of the Volanth","Year of the Peaceful Seas","Year of the Nine Stars","Year of the Dangerous Game","Year of the Hunting Ghosts",
        "Year of the Morning Horn","Year of the Bloody Tusk","Year of the Peoples' Mourning","Year of the Baleful Song","Year of the Falling Petals",
        "Year of the Ashen Tears","Year of the Stern Judgment","Year of the Austere Ceremonies","Year of the Telling Tome","Year of the Brutal Beast",
        "Year of the Many Floods","Year of the Shrouded Slayer","Year of the Angry Caverns","Year of the Covenant","Year of the Nomad",
        "Year of the Bloodfeud","Year of the Gruesome Grimoires","Year of the Resonant Silence","Year of the Poignant Poniard","Year of the Scarlet Sash",
        "Year of the Long March","Year of the Zombie Lords","Year of the Howling","Year of the Tainted Troll","Year of the Sundered Crypt",
        "Year of the Wraithwinds","Year of the Unshriven","Year of the Wandering Sylph","Year of the Zephyr","Year of the Eager Executioner",
        "Year of the Clashing Blades","Year of the Stricken Sun","Year of the Crawling Crags","Year of the Enigmatic Smile","Year of the Ominous Oracle",
        "Year of the Fanciful Feasts","Year of the Great Escape","Year of the Triton's Horn","Year of the Voracious Vole","Year of the Rampaging Raaserpents",
        "Year of the Slain Raven","Year of the White Jonquil","Year of the Clutching Death","Year of the Shambling Ice","Year of the Emerald Citadel",
        "Year of the Watchful Eyes","Year of the Realmsrage","Year of the Portents Perilous","Year of the Bound Evils","Year of the Earnest Oaths",
        "Year of the Toppled Throne","Year of the Despairing Elves","Year of the Lost Lance","Year of the Firedrake","Year of the Doom",
        "Year of the Hungry Jaws","Year of the Reaching Regret","Year of the Druid's Wrath","Year of the Painful Price","Year of the Lost Lord",
        "Year of the Dawn Rose","Year of the Hungry Pool","Year of the Last Hunt","Year of the Underdark Afire","Year of the Prisoner Unfettered",
        "Year of the Shorn Beard","Year of the Dowager Lady","Year of the Purloined Throne","Year of the Sleeping Princess","Year of the Twisted Horn",
        "Year of the Jovial Mage","Year of the Visions","Year of the Proud Father","Year of the Sad Refrains","Year of the Splendid Stag",
        "Year of the Prophet's Child","Year of the Gleeful Noise","Year of the Purple Wyrm","Year of the Gliding Man","Year of the Staggered Minotaur",
        "Year of the Netherese Lai","Year of the Shandon Eyes","Year of the Wavering Voice","Year of the Snowy Addax","Year of the Jeweled Aerie",
        "Year of the Proud Menhir","Year of the Somber Dancers","Year of the Stagnant Water","Year of the Coin","Year of the Glass Eye",
        "Year of the Dying Dwarf","Year of the Good Tidings","Year of the High Treachery","Year of the Strife","Year of the Midsummer's Dreams",
        "Year of the Enigma","Year of the Leaning Post","Year of the Lost Wayfarers","Year of the Scorched Sea","Year of the Missing Blade",
        "Year of the Drifting Stars","Year of the Laughter","Year of the Snow Sword","Year of the Sharp Edge","Year of the Mistmaidens",
        "Year of the Cowl","Year of the Yearning","Year of the Awakening Wyrm","Year of the Prying Gods","Year of the Torm Cloak",
        "Year of the Diamond Sword","Year of the Stalking Knight","Year of the Giggling Ogre","Year of the Aurumvorax","Year of the Scowling Duchess",
        "Year of the Bloody Stone","Year of the Crystal Ball","Year of the Fortified Mind","Year of the Awaiting Webs","Year of the Crying Sphinx",
        "Year of the Broken Crossbow","Year of the Elven Fortress","Year of the Gentle Hand","Year of the Lizard King","Year of the Shattered Tome",
        "Year of the Manticore Rampant","Year of the Moaning Gorge","Year of the Rotting Orchard","Year of the True Believer","Year of the Flourishing Forests",
        "Year of the Bend Sinister","Year of the Sisters' Battles","Year of the Crimson Thorn","Year of the Furious Horse","Year of the Grimacing Elf",
        "Year of the Firehawk","Year of the Gray Mists","Year of the Hearthstone","Year of the Holy Aspergill","Year of the Laughing Gull",
        "Year of the Black Fist","Year of the Star Rose","Year of the Patchworked Peace","Year of the Reaching Hand","Year of the Spreading Scourge",
        "Year of the Unsung Bard","Year of the Warrior's Rest","Year of the Bearded Maiden","Year of the Crescent Moon","Year of the Boastful Noble",
        "Year of the Dark Mystery","Year of the Many Tears","Year of the Gem Dragons","Year of the Widows","Year of the Harper's Apprentice",
        "Year of the Heavy Heart","Year of the Laughing Swan","Year of the Deadly Torch","Year of the Broken Locks","Year of the Mendacious Page",
        "Year of the Roving Tyrant","Year of the Firewall","Year of the Wizard's Chalice","Year of the Floating Petals","Year of the Copper Coil",
        "Year of the Silver Flagon","Year of the Wolfpacks","Year of the Sacrificed Fortune","Year of the AlarmedMerchants","Year of the Thessalhydra",
        "Year of the Ambitious Proposal","Year of the Deceptive Tongue","Year of the Slow Herald","Year of the Flying Serpent","Year of the Leaping Lion",
        "Year of the Billowed Sail","Year of the Twelve Bells","Year of the Darkened Sundial","Year of the Unfettered Genie","Year of the Ten Atonements",
        "Year of the Fighting Sage","Year of the Hunted Elk","Year of the Maverick","Year of the Amber","Year of the Midnight Sun",
        "Year of the Ruby Pendant","Year of the Steadfast Dwarf","Year of the Unmarked Path","Year of the Vigilant Familiar","Year of the Black Book",
        "Year of the Empty Throne","Year of the Jasmal Blade","Year of the False Smile","Year of the Hungry Box","Year of the Indigo Inferno",
        "Year of the Cornerstones","Year of the Thorns","Year of the Forgotten Fame","Year of the Saffron Orb","Year of the Sea Crossing",
        "Year of the Tired Horsemen","Year of the Exploding Orl","Year of the Snow Rose","Year of the Wondrous Sea","Year of the Broken Branch",
        "Year of the Flamedance","Year of the Blessed Morning","Year of the Cryptic Recipe","Year of the Endless Scroll","Year of the Final Price",
        "Year of the Hooded Tracker","Year of the Marching Golem","Year of the Moonbar Crest","Year of the Opening Flower","Year of the Roiling Cauldron",
        "Year of the Stricken Star","Year of the Toothless Skulls","Year of the Scratching Claw","Year of the Two-edged Axe","Year of the Winter's Warmth",
        "Year of the Unfettered Secrets","Year of the Brazen Vizier","Year of the Curse","Year of the Giant's Oath","Year of the Singing Arrows",
        "Year of the Thistle","Year of the Fell Firebreak","Year of the Fell Pearls","Year of the Twelve Teeth","Year of the Shining Shield",
        "Year of the Burning Tree","Year of the Leaning Keep","Year of the Open Tome","Year of the Raised Sword","Year of the Cold Flame",
        "Year of the Spitting Cat","Year of the Empty Hand","Year of the Calling Shrike","Year of the Common Corpse","Year of the Tolling Bell",
        "Year of the Thirsty Sword","Year of the August Armathor","Year of the Queen's Tears","Year of the Trial","Year of the Rising Maeran",
        "Year of the Rotting Word","Year of the Plough","Year of the Waiting","Year of the Lone Tribe","Year of the Ogre",
        "Year of the Deathblows Denied","Year of the Ruins Reborn","Year of the Sudden Journey","Year of the Watching Raven","Year of the Book",
        "Year of the Bats","Year of the Sinhala","Year of the Winding Road","Year of the Palace","Year of the Chase",
        "Year of the Great Riches","Year of the Falling Maeran","Year of the Spouting Fish","Year of the Bloodied Soldier","Year of the Cracked Turtle",
        "Year of the Enchanted Trail","Year of the Fearless Peasant","Year of the Red Rain","Year of the Hurled Axe","Year of the Flashing Eyes",
        "Year of the Liberty Crest","Year of the Penitent Rogue","Year of the Fireslaughter","Year of the Five Jugs","Year of the Fell Wizardry",
        "Year of the Rearing Lion","Year of the Sky Riders","Year of the Turning Wheel","Year of the Unhanged Man","Year of the Vengeful Halfling",
        "Year of the Cold Claws","Year of the Sudden Sorrows","Year of the Circling Vulture","Year of the Flying Steed","Year of the Animated Armor",
        "Year of the Foolish Bridegroom","Year of the Blazing Call","Year of the Advancing Wind","Year of the Clarion Trumpet","Year of the Forbidden Tome",
        "Year of the Doomguard","Year of the Empty Hourglass","Year of the Rings Royal","Year of the Guiding Crow","Year of the Perilous Halls",
        "Year of the Telltale Candle","Year of the Crooked Finger","Year of the Entombed Poet","Year of the Far-flung Harp","Year of the Haunted Crew",
        "Year of the Mageling","Year of the Pensive Gibberling","Year of the Shandon Veil","Year of the Deadly Duo","Year of the Pickled Privateer",
        "Year of the Runelightning","Year of the Squire","Year of the Tearful Princess","Year of the Wandering Gnome","Year of the Bright Standard",
        "Year of the Child's Trinket","Year of the Children","Year of the Cairngorm Crown","Year of the Emptied Lair","Year of the Haunting Harpy",
        "Year of the Bent Coin","Year of the Slaying Spells","Year of the Eternal Amber","Year of the Hooded Rogue","Year of the Marching Forest",
        "Year of the Orator","Year of the Rebel Uprising","Year of the Scythe","Year of the Submerged Country","Year of the Caravan",
        "Year of the Bright Nights","Year of the Dusken Ride","Year of the Flaming Dwarf","Year of the Meddling Avatar","Year of the Dark Stalking",
        "Year of the Muster","Year of the Breaking Ice","Year of the Watching Helm","Year of the Slain Mountain","Year of the Weary Scribe",
        "Year of the Charging Mare","Year of the Disfiguring Scar","Year of the Fearful Harper","Year of the Much Iron","Year of the Gaping Sky",
        "Year of the Wailing Winds","Year of the Awakening","Year of the Heavenly Rock","Year of the Labyrinth","Year of the Oracle's Carcass",
        "Year of the Pillaged Crypt","Year of the Second Son","Year of the Bold Barbarian","Year of the Treacherous Path","Year of the Broken Spear",
        "Year of the Three Signs","Year of the Defiant Mountain","Year of the Flamboyant Coif","Year of the Hunted Whale","Year of the Grimacing Sage",
        "Year of the Maid Enraged","Year of the Roaring Tempest","Year of the Stone Rose","Year of the Dracorage","Year of the Sure Quarrel",
        "Year of the Smoldering Spells","Year of the Howling Axe","Year of the Wandering Wyvern","Year of the Pirates' Trove","Year of the Lathander's Light",
        "Year of the Screaming Princesses","Year of the Crimson Magics","Year of the Tempest","Year of the Wistful Nymph","Year of the Bold Strides",
        "Year of the Warlords","Year of the Comforting Hand","Year of the Nightmaidens","Year of the Dreamforging","Year of the Bane's Brood",
        "Year of the Falling Stars","Year of the Final Test","Year of the Immortals","Year of the Spreading Spring","Year of the Haunted Haven",
        "Year of the Lion's Heart","Year of the Mistmarch Soldier","Year of the Reaching Beacon","Year of the Dark Rider","Year of the Singing Shards",
        "Year of the Singing Mushrooms","Year of the Twilight Campaign","Year of the Vitriolic Sage","Year of the Chevalier","Year of the Auril's Absence",
        "Year of the Keening Gale","Year of the Dogged Search","Year of the Frozen Kingdoms","Year of the Lashing and Torment","Year of the Tolling Terrors",
        "Year of the Grueling Story","Year of the Laughing Dead","Year of the Azure Frost","Year of the Spider's Daughter","Year of the Broken Pillar",
        "Year of the Fantastic Spectacle","Year of the Pious Dance","Year of the Shattered Lance","Year of the Deluded Tyrant","Year of the Stranger",
        "Year of the Watching Wood","Year of the Lord's Dilemma","Year of the Minotaur Paladin","Year of the Seer Born","Year of the Thunder's Child",
        "Year of the Spawning","Year of the Lions' Roars","Year of the Wandering Elfmaid","Year of the Bottomless Ocean","Year of the Tightening Fist",
        "Year of the Bronze Banner","Year of the Defiant Salute","Year of the Friendly Jackal","Year of the Hierodulic Wolverines","Year of the Lazy Scribe",
        "Year of the Misguided Archer","Year of the Disastrous Bauble","Year of the Prancing Korred","Year of the Sighted Hind","Year of the Tireless Lute",
        "Year of the Vacant Plain","Year of the Seer's Fires","Year of the Forgotten Anger","Year of the Shambles","Year of the Three Faces",
        "Year of the Slaughter","Year of the Watery Graves","Year of the Aimless Mystic","Year of the Bursting Song","Year of the Crested Thrush",
        "Year of the Dawndance","Year of the Diverged Path","Year of the Gleaming Crown","Year of the Rose","Year of the Restless",
        "Year of the Bloodrose","Year of the Maelstrom","Year of the Chaste Maiden","Year of the Consuming Glory","Year of the Dark Dawn",
        "Year of the Guardian","Year of the Solemn Halfling","Year of the Skulk","Year of the Open Chest","Year of the Lover's Eyes",
        "Year of the Bloody Fields","Year of the Old Giant","Year of the Perilous Storm","Year of the Outcast Prophet","Year of the Last Enclave",
        "Year of the Haunted Herald","Year of the Empty Scabbard","Year of the Twelverule","Year of the False Bargain","Year of the Sharn Suitors",
        "Year of the Perplexing Sphinx","Year of the Shameful Plea","Year of the Rose Pearls","Year of the Shattered Chains","Year of the Shared Sorrows",
        "Year of the Lupine Torque","Year of the Azure Blood","Year of the Luminar Procession","Year of the Peryton","Year of the Gilded Cormorant",
        "Year of the Howling Moon","Year of the Seven Kings Horde","Year of the Talking Spiders","Year of the Persuasive Voice","Year of the Sylvan Wards",
        "Year of the Petulant Dragon","Year of the Shadowkin Return","Year of the Falling Menhirs","Year of the Sharpened Teeth","Year of the Shining Waves",
        "Year of the Knight","Year of the Eyes","Year of the Sword's Oath","Year of the Talisman","Year of the Giant's Maul",
        "Year of the Smiling Flame","Year of the Tardy Guests","Year of the Glad Tidings","Year of the Angry Sea","Year of the Persuasive Trees",
        "Year of the Scourge","Year of the Molten Man","Year of the Portentous Waters","Year of the Remembering Stones","Year of the Sun Underground",
        "Year of the Tyrant's Lament","Year of the Winged Gift","Year of the Wizened Mage","Year of the Blood Tusk Charge","Year of the Cloven Stones",
        "Year of the Swimming Lass","Year of the Quiet Earth","Year of the Prancing Centaur","Year of the Shrouded Sky","Year of the Long Shadows",
        "Year of the Obsidian Heart","Year of the Countless Scribes","Year of the Parchment Heretical","Year of the Leering Orc","Year of the Earth Shaking",
        "Year of the Moonlight Tapestry","Year of the Dark Mask","Year of the Hoary Host","Year of the Fledglings","Year of the Agate Hammer",
        "Year of the Storm Skeleton","Year of the Prowling Naga","Year of the Majesty","Year of the Secret Rider","Year of the Stalking Satyr",
        "Year of the Sinking Sails","Year of the Shieldtree","Year of the Tomb","Year of the Grisly Ghosts","Year of the Howling Hourglass",
        "Year of the Immoral Imp","Year of the Mesmer Pool","Year of the Arcane Guise","Year of the Soft Fogs","Year of the Lynx",
        "Year of the Poisoned Quill","Year of the Bone Helm","Year of the Guide","Year of the Peltast","Year of the Bloody Wave",
        "Year of the Midday Mists","Year of the Shrike","Year of the Sundered Shields","Year of the Lean Purse","Year of the Baldric",
        "Year of the Buckler","Year of the Embers","Year of the Dragon Altar","Year of the Gold Sash","Year of the Private Tears",
        "Year of the Seven Trinkets","Year of the Sarune","Year of the Bloated Baron","Year of the Gamine","Year of the Blazing Banners",
        "Year of the Armarel","Year of the Crimson Crag","Year of the Ocean's Wrath","Year of the Night's Peace","Year of the Waking Wrath",
        "Year of the Starlight","Year of the Green Wings","Year of the Falling Moon","Year of the Swimming Cats","Year of the Prideful Tales",
        "Year of the Toppled Tree","Year of the Frozen Flower","Year of the Horn","Year of the Trembling Tree","Year of the Swollen Stars",
        "Year of the Winged Worm","Year of the Black Buck","Year of the Wall","Year of the Tattered Banners","Year of the Carrion Crow",
        "Year of the Long Watch","Year of the Bright Star","Year of the Weeping Wives","Year of the Many Monsters","Year of the Full Flagon",
        "Year of the Black Horde","Year of the Struck Gong","Year of the Grotto","Year of the Lone Candle","Year of the Bloodied Sword",
        "Year of the Bright Sun","Year of the Lost Lady","Year of the Yellow Rose","Year of the Blue Dragon","Year of the Defiant Keep",
        "Year of the Pain","Year of the Burning Steel","Year of the Purple Basilisk","Year of the Cockatrice","Year of the Bold Knight",
        "Year of the Riven Skull","Year of the Wandering Winds","Year of the Empty Goblet","Year of the Beckoning Death","Year of the Silent Steel",
        "Year of the Raging Flame","Year of the Dusty Throne","Year of the Killing Wave","Year of the Wilted Flowers","Year of the Vigilant Fist",
        "Year of the Broken Blade","Year of the Bright Dreams","Year of the Black Wind","Year of the Tressym","Year of the Shattered Altar",
        "Year of the Flowers","Year of the Leaping Frog","Year of the Groaning Cart","Year of the Daystars","Year of the Moat",
        "Year of the Tooth","Year of the Shattered Wall","Year of the Shrieker","Year of the Wagon","Year of the Purple Toad",
        "Year of the Blade","Year of the Crumbling Keep","Year of the Beholder","Year of the Many Bones","Year of the Snarling Dragon",
        "Year of the Manticore","Year of the Cold Soul","Year of the Many Mists","Year of the Crawling Clouds","Year of the Dying Stars",
        "Year of the Blacksnake","Year of the Rock","Year of the Smoky Moon","Year of the Roaring Horn","Year of the Sighing Serpent",
        "Year of the Whelm","Year of the Hooded Falcon","Year of the Wandering Waves","Year of the Talking Skull","Year of the Deep Moon",
        "Year of the Ormserpent","Year of the Black Hound","Year of the Singing Skull","Year of the Pointed Bone","Year of the Claw",
        "Year of the Starfall","Year of the Trumpet","Year of the Broken Helm","Year of the Evening Sun","Year of the Stag",
        "Year of the Creeping Fang","Year of the Thunder","Year of the Mace","Year of the Catacombs","Year of the Sunset Winds",
        "Year of the Storms","Year of the Fist","Year of the Griffon","Year of the Shattered Oak","Year of the Shadowtop",
        "Year of the Spilled Blood","Year of the Gulagoar","Year of the Wandering Wyrm","Year of the Tired Treant","Year of the Fallen Throne",
        "Year of the Watching Cold","Year of the Chains","Year of the Lurking Death","Year of the Dreamwebs","Year of the Grimoire",
        "Year of the Great Harvests","Year of the Striking Hawk","Year of the Blue Flame","Year of the Adder","Year of the Lost Helm",
        "Year of the Marching Moon","Year of the Leaping Dolphin","Year of the Sword and Stars","Year of the Striking Falcon","Year of the Blazing Brand",
        "Year of the Snow Winds","Year of the Highmantle","Year of the Wandering Maiden","Year of the Wanderer","Year of the Weeping Moon",
        "Year of the Lion","Year of the Gate","Year of the Behir","Year of the Boot","Year of the Moonfall",
        "Year of the Saddle","Year of the Bloodbird","Year of the Bright Blade","Year of the Spur","Year of the Bridle",
        "Year of the Morningstar","Year of the Crown","Year of the Dragon","Year of the Arch","Year of the Bow",
        "Year of the Harp","Year of the Worm","Year of the Prince","Year of the Shadows","Year of the Serpent",
        "Year of the Turret","Year of the Maidens","Year of the Helm","Year of the Wyvern","Year of the Wave",
        "Year of the Sword","Year of the Staff","Year of the Shield","Year of the Banner","Year of the Gauntlet",
        "Year of the Tankard","Year of the Unstrung Harp","Year of the Wild Magic","Year of the Rogue Dragons","Year of the Lightning Storms",
        "Year of the Risen Elfkin","Year of the Bent Blade","Year of the Haunting","Year of the Cauldron","Year of the Lost Keep",
        "Year of the Blazing Hand","Year of the Starving","Year of the Black Blazon","Year of the Vindicated Warrior","Year of the Three Streams Blooded",
        "Year of the Blue Fire","Year of the Halflings' Lament","Year of the Emerald Ermine","Year of the Tanarukka","Year of the Forgiven Foes",
        "Year of the Walking Man","Year of the Wrathful Eye","Year of the Scroll","Year of the Ring","Year of the Deaths Unmourned",
        "Year of the Silent Death","Year of the Secret","Year of the Quill","Year of the Voyage","Year of the Fallen Friends",
        "Year of the Lost Ships","Year of the Sheltered Viper","Year of the Exorcised Helm","Year of the Hidden Harp","Year of the Sceptered One",
        "Year of the Golden Mask","Year of the Blackened Moon","Year of the Halls Unhaunted","Year of the Solitary Cloister","Year of the True Omens",
        "Year of the Eight-legged Mount","Year of the Wrathful Vizier","Year of the Dauntless Dwarves","Year of the Sunken Vessels","Year of the Sea Lions Roaring",
        "Year of the Staves Arcane","Year of the Enthroned Puppet","Year of the Phaerimm's Vengeance","Year of the Lords' Coronation","Year of the Empty Necropolis",
        "Year of the Dark Goddess","Year of the Walking Trees","Year of the Advancing Shadows","Year of the Thundering Hosts","Year of the Dog-Eared Journal",
        "Year of the Seven Sisters","Year of the Dozen Dwarves","Year of the Shalarins Surfacing","Year of the Elfqueen's Joy","Year of the Ten Terrors",
        "Year of the Stalking Horrors","Year of the Lashing Tail","Year of the Silent Thunder","Year of the Silent Departure","Year of the Silent Crickets",
        "Year of the Silent Bell","Year of the Silent Shadows","Year of the Silent Flute","Year of the Silent Waterfalls","Year of the Silent Tear",
        "Year of the Azuth's Woe","Year of the Resurrections Rampant","Year of the Darkenbeasts Risen","Year of the Silver Bell Tolling","Year of the Seductive Cambion",
        "Year of the Malachite Throne","Year of the Queen's Honor","Year of the Fallen Tower","Year of the Neomen Swords","Year of the Godly Invitation",
        "Year of the Holy Thunder","Year of the Knowledge Unearthed","Year of the Impatient Son","Year of the Strangled Jester","Year of the Emerald Sun",
        "Year of the King's Repentance","Year of the Mithral Hammer","Year of the Lightning Strikes","Year of the Plotting Priests","Year of the Forged Sigil",
        "Year of the Malachite Shadows","Year of the Three Goddesses Blessing","Year of the Elves' Weeping","Year of the Reborn Hero","Year of the Six-Armed Elf",
        "Year of the Elven Swords Returned","Year of the Mages in Amber","Year of the Three Heroes United","Year of the First Circle","Year of the Splendors Burning",
        "Year of the Second Circle","Year of the Plagued Lords","Year of the Third Circle","Year of the Heretic's Rampage","Year of the Fourth Circle",
        "Year of the Final Stand","Year of the Fifth Circle","Year of the Purloined Statue","Year of the Dark Circle","Year of the Ageless One",
        "Year of the Deep Water Drifting","Year of the Grinning Halfling","Year of the Narthex Murders","Year of the Tasked Weasel","Year of the Awakened Sleepers",
        "Year of the Iron Dwarf's Vengeance","Year of the Nether Mountain Scrolls","Year of the Rune Lords Triumphant","Year of the Dwarvenkind Reborn","Year of the Warrior Princess",
        "Year of the Star Walker's Return","Year of the Scarlet Witch","Year of the Three Ships Sailing","Year of the Purple Dragons","Year of the Twelve Warnings",
        "Year of the Tyrant's Pawn","Year of the Duplicitous Courtier","Year of the Palls Purple","Year of the Black Regalia","Year of the Desperate Gambit",
        "Year of the Sea's Secrets Revealed","Year of the Shining Mythal","Year of the Pox Plague","Year of the Haunted Inn","Year of the Conquering Queen",
        "Year of the Ogres Marching","Year of the Discarded Shields","Year of the Glowing Onyx","Year of the Legend Reborn","Year of the Sea Lion",
        "Year of the Treasure Abandoned","Year of the Lion Rampant","Year of the Shattered Mirror","Year of the Tawny Feline","Year of the Lost Wagers",
        "Year of the Howling Ghouls","Year of the Hangman's Joke","Year of the Coward Rewarded","Year of the Adomal Tapestry","Year of the Deceitful Brother",
        "Year of the Arcane","Year of the Moon Harp Restored","Year of the Bored Phylls","Year of the Brownie's Delight","Year of the Captive Harper",
        "Year of the Drawn Line","Year of the Hazy Coast","Year of the Hoard Retaken","Year of the Insufferable Mystic","Year of the Horseman's Triumph",
        "Year of the Long-toothed Tiger","Year of the Oozing Bog","Year of the Locked Crypt","Year of the Mishapen Mage","Year of the Pale Lords",
        "Year of the Laurel Wreath","Year of the Mirrored Face","Year of the Jungle's Vengeance","Year of the Stalking Tiger","Year of the Thoughtless Suitor",
        "Year of the Lifeless Archdruid","Year of the Mirthful House","Year of the Painted Grin","Year of the Sacred Sceptre","Year of the Shadow Fiends",
        "Year of the Undying March","Year of the Winter Rose","Year of the Dungeons Reclaimed","Year of the Handsome Deal","Year of the Meandering Archipelago",
        "Year of the Scarlet Tabard","Year of the Misty Grave","Year of the Overflowing Cup","Year of the Request","Year of the Dark Chosen",
        "Year of the Argent Scarab","Year of the Stone Steps","Year of the Murdered Sage","Year of the Watchful Guardian","Year of the Shepherd's Son",
        "Year of the Trees' Receding","Year of the Wild Hunt","Year of the Pointing Finger","Year of the Starlit Necklace","Year of the Slaughtered Lamb",
        "Year of the Unkindest Cut","Year of the Weasel","Year of the Sacred Lash","Year of the Studious Enchanter","Year of the Turned Page",
        "Year of the Vacant Cairn","Year of the Red Mantle","Year of the Stingray","Year of the Wicked Jailor","Year of the Rebuked Storm",
        "Year of the Twin Pavilions","Year of the Vanishing Throne","Year of the Whispering Hood","Year of the Steadfast Patrol","Year of the Underking",
        "Year of the Widow's Tears","Year of the Rings","Year of the Howling Winds","Year of the Decay","Year of the Skirling Pipes",
        "Year of the Bloodied Manacles","Year of the Pax Draconomica","Year of the Long Silence","Year of the Swarming Ravens","Year of the Watching Ancestors",
        "Year of the Coming Twilight","Year of the Skeletons","Year of the Dying Hate","Year of the Rising Stars","Year of the Fragrant Orchards",
        "Year of the Raging Baatezu","Year of the Heavenly Scriptures","Year of the Stolen Gold","Year of the Doom Cauldron","Year of the Black Pearls" };
        }

    }

    /// <summary>
    /// An enmum is like a category machine. This one makes Seasons.
    /// </summary>
    public enum Season
    {
        Spring, //Rainy
        Summer, //Rainy
        Fall, //Dry
        Winter //Dry
    }
    /// <summary>
    /// This is the base class. Everything that does not move is a structure.
    /// </summary>
    public abstract class Structure
    {
        public Coordinate _Origin { get; set; }
        public Double _Orientation { get; set; }

        public abstract List<SharpKml.Dom.Placemark> ToPlaceMarks();

        protected Structure() { }

    }

    /// <summary>
    /// So to be a little less abstract I decided to build the idea of an abstract tree.
    /// The Lindenmayer system will be the Top High Hanging Fruit for this project.
    /// https://en.wikipedia.org/wiki/L-system
    /// https://en.wikipedia.org/wiki/Glossary_of_plant_morphology#Plant_habit
    /// 
    /// </summary>
    /// 
    public abstract class Plant : Structure
    {

        protected Geotool_Objects.GeoTools OG;
        protected Random _Randotron;
        public List<Geotool_Objects.Polygon> _Foliage { get; set; }
        public List<Geotool_Objects.Polygon> _Branches { get; set; }
        public List<Geotool_Objects.Polygon> MainStem { get; set; }
        public int _Generation { get; set; }

        protected String Species;

        public String GetSpecies()
        {
            return Species;
        }

        protected Plant() { }

        protected List<Geotool_Objects.Polygon> MakeStem(Coordinate Origin, Double Height, Double UV)
        {
            List<Geotool_Objects.Polygon> returnList = new List<Geotool_Objects.Polygon>();
            List<Coordinate> Triangle = OG.MakeGeodesicEquilateralTriangleList(UV, Origin);

            Coordinate Temp = new Coordinate(Origin.Latitude, Origin.Longitude, Origin.Altitude);

            Temp.Altitude = Temp.Altitude + Height;
            //Now I build three polygons
            List<Coordinate> WorkList = new List<Coordinate>();
            //Clean this up in a loop later.
            WorkList.Clear();
            WorkList.Add(Temp);
            WorkList.Add(Triangle[0]);
            WorkList.Add(Triangle[1]);
            returnList.Add(new Geotool_Objects.Polygon(WorkList));
            WorkList.Clear();
            WorkList.Add(Temp);
            WorkList.Add(Triangle[1]);
            WorkList.Add(Triangle[2]);
            returnList.Add(new Geotool_Objects.Polygon(WorkList));
            WorkList.Clear();
            WorkList.Add(Temp);
            WorkList.Add(Triangle[2]);
            WorkList.Add(Triangle[0]);
            returnList.Add(new Geotool_Objects.Polygon(WorkList));


            return returnList;
        }

        protected abstract void MakeBranch(Coordinate Origin, Double Height, Double UV, Double Bearing);

        public abstract List<SharpKml.Dom.Placemark> ToPlaceMarks(bool low, Season S);

    }
    /// <summary>
    /// Spelled correctly and this is a scientifically accurate name. Have fun in the compiler typing it out lol. Since the L system has 
    /// not been implemented.
    /// </summary>
    public class GenericDeciduous : Plant
    {

        public GenericDeciduous(int Seasons, Coordinate Origin, Double Orientation)
        {
            OG = new GeoTools("Chult");
            _Foliage = new List<Geotool_Objects.Polygon>();
            _Branches = new List<Geotool_Objects.Polygon>();
            _Randotron = new Random();
            Species = "Generic Deciduous";






            //We are growing from a seed so we need to add the initial stem(s)

            _Origin = new Coordinate(Origin.Latitude, Origin.Longitude, Origin.Altitude);



            MainStem = MakeStem(_Origin, Seasons, Seasons * 0.00005);

            //Start at the base of the main stem and increment up the stem at 0.5 m increments.
            for (double i = _Origin.Altitude; i <= _Origin.Altitude + Seasons; i += 0.5)
            {
                Coordinate Temp = new Coordinate(_Origin.Latitude, _Origin.Longitude, i);
                //I picked the halway point for this example.
                if (i >= _Origin.Altitude + (Seasons / 2.00) && _Foliage.Count == 0)
                {
                    // _Foliage.Add(new Geotool_Objects.Polygon(OG.GeodesicRotation(OG.MakeGeodesicHexagon(Temp, (0.0005 * (_Origin.Altitude + Seasons - i + 1))), Temp, (_Randotron.Next(0, 90) + Orientation) % 360)));

                }
                if (i >= _Origin.Altitude + (Seasons / 2.00))
                {
                    //     _Foliage.Add(new Geotool_Objects.Polygon(OG.GeodesicRotation(OG.MakeGeodesicHexagon(Temp, (0.0005 * (_Origin.Altitude + Seasons - i + 1))), Temp, (_Randotron.Next(0, 90) + Orientation) % 360)));
                    MakeBranch(Temp, 1, (0.0005 * (_Origin.Altitude + Seasons - i + 1)), -1);
                }
            }

        }

        protected override void MakeBranch(Coordinate Origin, Double Height, Double UV, Double Bearing)
        {
            List<Coordinate> tempBranch = new List<Coordinate>();
            Double D;
            if (Bearing < 0)
            {
                D = _Randotron.Next(0, 360);
            }
            else
            {
                D = ((Bearing - 15) + _Randotron.Next(0, 30)) % 360;
            }


            Coordinate t = OG.DestinationCoordinate(Origin, UV, D);
            Coordinate t2 = OG.DestinationCoordinate(Origin, UV / 2, D);
            t.Altitude = t.Altitude + Height;
            t2.Altitude = t2.Altitude + Height / 2;
            tempBranch.Add(t);
            //Can you understand this super cool formula? (D-90)%360 the left side and (D + 90) % 360 
            tempBranch.Add(OG.DestinationCoordinate(Origin, UV / 20, (D - 90) % 360));
            tempBranch.Add(OG.DestinationCoordinate(Origin, UV / 20, (D + 90) % 360));
            _Branches.Add(new Geotool_Objects.Polygon(tempBranch));
            _Foliage.Add(new Geotool_Objects.Polygon(OG.GeodesicRotation(OG.MakeGeodesicHexagon(t, (0.0009) * Height), t, (_Randotron.Next(0, 90)) % 360)));
            _Foliage.Add(new Geotool_Objects.Polygon(OG.GeodesicRotation(OG.MakeGeodesicHexagon(t2, (0.0009) * Height), t2, (_Randotron.Next(0, 90)) % 360)));

            if (Height / 2 >= 0.5)
            {
                MakeBranch(t, Height / 3, UV / 3, 0);
                MakeBranch(t, Height / 3, UV / 3, 120);
                MakeBranch(t, Height / 3, UV / 3, 240);
                MakeBranch(t2, Height / 3, UV / 3, 0);
                MakeBranch(t2, Height / 3, UV / 3, 120);
                MakeBranch(t2, Height / 3, UV / 3, 240);
            }

        }

        public GenericDeciduous()
        {
            Species = "Generic Deciduous";
        }

        public override List<SharpKml.Dom.Placemark> ToPlaceMarks(bool low, Season S)
        {
            List<Placemark> returnList = new List<Placemark>();

            foreach (Geotool_Objects.Polygon P in MainStem)
            {
                returnList.Add(OG.SampleStyler(P.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species));

            }
            if (!low)
            {
                foreach (Geotool_Objects.Polygon Branch in _Branches)
                {
                    returnList.Add(OG.SampleStyler(Branch.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species));

                }
            }
            Placemark Trunk = OG.CombineIntoMultiGeometry(returnList);
            returnList = new List<Placemark>();

            foreach (Geotool_Objects.Polygon Foliage in _Foliage)
            {
                returnList.Add(OG.SampleStyler(Foliage.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 107, 53), new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 107, 53), Species + " foliage"));

            }
            Placemark Leaves = OG.CombineIntoMultiGeometry(returnList);
            returnList = new List<Placemark>();

            returnList.Add(Trunk.Clone());
            returnList.Add(Leaves.Clone());

            return returnList;
        }

        public override List<Placemark> ToPlaceMarks()
        {
            List<Placemark> returnList = new List<Placemark>();

            foreach (Geotool_Objects.Polygon P in MainStem)
            {
                returnList.Add(OG.SampleStyler(P.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species));

            }

            foreach (Geotool_Objects.Polygon Branch in _Branches)
            {
                returnList.Add(OG.SampleStyler(Branch.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species));

            }
            Placemark Trunk = OG.CombineIntoMultiGeometry(returnList);
            returnList = new List<Placemark>();

            foreach (Geotool_Objects.Polygon Foliage in _Foliage)
            {
                returnList.Add(OG.SampleStyler(Foliage.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 107, 53), new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 107, 53), Species + " foliage"));

            }
            Placemark Leaves = OG.CombineIntoMultiGeometry(returnList);
            returnList = new List<Placemark>();

            returnList.Add(Trunk.Clone());
            returnList.Add(Leaves.Clone());

            return returnList;
        }

    }

    public class GenericDeciduousII : Plant
    {

        public GenericDeciduousII(int Seasons, Coordinate Origin, Double Orientation)
        {
            OG = new GeoTools("Chult");
            _Foliage = new List<Geotool_Objects.Polygon>();
            _Branches = new List<Geotool_Objects.Polygon>();
            _Randotron = new Random();
            Species = "Generic Deciduous II";

            //We are growing from a seed so we need to add the initial stem(s)

            _Origin = new Coordinate(Origin.Latitude, Origin.Longitude, Origin.Altitude);



            MainStem = MakeStem(_Origin, Seasons, Seasons * 0.00005);

            //Start at the base of the main stem and increment up the stem at 0.5 m increments.
            for (double i = _Origin.Altitude; i <= _Origin.Altitude + Seasons; i += 0.5)
            {
                Coordinate Temp = new Coordinate(_Origin.Latitude, _Origin.Longitude, i);
                //I picked the halway point for this example.
                if (i >= _Origin.Altitude + (Seasons / 2.00) && _Foliage.Count == 0)
                {
                    // _Foliage.Add(new Geotool_Objects.Polygon(OG.GeodesicRotation(OG.MakeGeodesicHexagon(Temp, (0.0005 * (_Origin.Altitude + Seasons - i + 1))), Temp, (_Randotron.Next(0, 90) + Orientation) % 360)));

                }
                if (i >= _Origin.Altitude + (Seasons / 2.00))
                {
                    //     _Foliage.Add(new Geotool_Objects.Polygon(OG.GeodesicRotation(OG.MakeGeodesicHexagon(Temp, (0.0005 * (_Origin.Altitude + Seasons - i + 1))), Temp, (_Randotron.Next(0, 90) + Orientation) % 360)));
                    MakeBranch(Temp, 1, (0.0005 * (_Origin.Altitude + Seasons - i + 1)), -1);
                }
            }

        }

        protected override void MakeBranch(Coordinate Origin, Double Height, Double UV, Double Bearing)
        {
            List<Coordinate> tempBranch = new List<Coordinate>();
            Double D;
            if (Bearing < 0)
            {
                D = _Randotron.Next(0, 360);
            }
            else
            {
                D = ((Bearing - 15) + _Randotron.Next(0, 30)) % 360;
            }


            Coordinate t = OG.DestinationCoordinate(Origin, UV, D);
            Coordinate t2 = OG.DestinationCoordinate(Origin, UV / 2, D);
            t.Altitude = t.Altitude + Height;
            t2.Altitude = t2.Altitude + Height / 2;
            tempBranch.Add(t);
            //Can you understand this super cool formula? (D-90)%360 the left side and (D + 90) % 360 
            tempBranch.Add(OG.DestinationCoordinate(Origin, UV / 20, (D - 90) % 360));
            tempBranch.Add(OG.DestinationCoordinate(Origin, UV / 20, (D + 90) % 360));
            _Branches.Add(new Geotool_Objects.Polygon(tempBranch));
            _Foliage.Add(new Geotool_Objects.Polygon(OG.GeodesicRotation(OG.MakeGeodesicStarBurst(t, (0.0009) * Height), t, (_Randotron.Next(0, 90)) % 360)));
            _Foliage.Add(new Geotool_Objects.Polygon(OG.GeodesicRotation(OG.MakeGeodesicStarBurst(t2, (0.0009) * Height), t2, (_Randotron.Next(0, 90)) % 360)));

            if (Height / 2 >= 0.5)
            {
                MakeBranch(t, Height / 3, UV / 2.5, D);
                MakeBranch(t, Height / 3, UV / 2.5, D - 45);
                MakeBranch(t, Height / 3, UV / 2.5, D + 45);
                MakeBranch(t2, Height / 3, UV / 2.75, D);
                MakeBranch(t2, Height / 3, UV / 2.75, D + 45);
                MakeBranch(t2, Height / 3, UV / 2.75, D - 45);
            }

        }

        public GenericDeciduousII()
        {
            Species = "Generic Deciduous II";
        }

        public override List<SharpKml.Dom.Placemark> ToPlaceMarks(bool low, Season S)
        {
            List<Placemark> returnList = new List<Placemark>();

            foreach (Geotool_Objects.Polygon P in MainStem)
            {
                returnList.Add(OG.SampleStyler(P.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species));

            }
            if (!low)
            {

                foreach (Geotool_Objects.Polygon Branch in _Branches)
                {
                    returnList.Add(OG.SampleStyler(Branch.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species));

                }
            }
            Placemark Trunk = OG.CombineIntoMultiGeometry(returnList);
            returnList = new List<Placemark>();

            foreach (Geotool_Objects.Polygon Foliage in _Foliage)
            {
                returnList.Add(OG.SampleStyler(Foliage.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 200, 70), new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 206, 137), Species + " foliage"));

            }
            Placemark Leaves = OG.CombineIntoMultiGeometry(returnList);
            returnList = new List<Placemark>();

            returnList.Add(Trunk.Clone());
            returnList.Add(Leaves.Clone());

            return returnList;
        }

        public override List<Placemark> ToPlaceMarks()
        {
            List<Placemark> returnList = new List<Placemark>();

            foreach (Geotool_Objects.Polygon P in MainStem)
            {
                returnList.Add(OG.SampleStyler(P.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species));

            }

            foreach (Geotool_Objects.Polygon Branch in _Branches)
            {
                returnList.Add(OG.SampleStyler(Branch.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species));

            }
            Placemark Trunk = OG.CombineIntoMultiGeometry(returnList);
            returnList = new List<Placemark>();

            foreach (Geotool_Objects.Polygon Foliage in _Foliage)
            {
                returnList.Add(OG.SampleStyler(Foliage.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 200, 70), new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 206, 137), Species + " foliage"));

            }
            Placemark Leaves = OG.CombineIntoMultiGeometry(returnList);
            returnList = new List<Placemark>();

            returnList.Add(Trunk.Clone());
            returnList.Add(Leaves.Clone());

            return returnList;
        }

    }

    public class WhiteOak : Plant
    {

        public WhiteOak(int Seasons, Coordinate Origin, Double Orientation)
        {
            OG = new GeoTools("Chult");
            _Foliage = new List<Geotool_Objects.Polygon>();
            _Branches = new List<Geotool_Objects.Polygon>();
            _Randotron = new Random();
            Species = "White Oak";


            //We are growing from a seed so we need to add the initial stem(s)

            _Origin = new Coordinate(Origin.Latitude, Origin.Longitude, Origin.Altitude);



            MainStem = MakeStem(_Origin, (Seasons / 4.00), Seasons * 0.00005);

            //Start at the base of the main stem and increment up the stem at 0.5 m increments.
            for (double i = _Origin.Altitude; i <= _Origin.Altitude + (Seasons / 4.00); i += 0.5)
            {
                Coordinate Temp = new Coordinate(_Origin.Latitude, _Origin.Longitude, i);
                //I picked the halway point for this example.
                if (i >= _Origin.Altitude + ((Seasons / 4.00) / 2.00) && _Foliage.Count == 0)
                {
                    //    _Foliage.Add(new Geotool_Objects.Polygon(OG.GeodesicRotation(OG.MakeGeodesicHexagon(Temp, (0.0005 * (_Origin.Altitude + (Seasons / 4.00) - i + 1))), Temp, (_Randotron.Next(0, 90) + Orientation) % 360)));

                }
                if (i >= _Origin.Altitude + ((Seasons / 4.00) / 2.00))
                {
                    //     _Foliage.Add(new Geotool_Objects.Polygon(OG.GeodesicRotation(OG.MakeGeodesicHexagon(Temp, (0.0005 * (_Origin.Altitude + Seasons - i + 1))), Temp, (_Randotron.Next(0, 90) + Orientation) % 360)));
                    MakeBranch(Temp, 2, (0.00025 * (_Origin.Altitude + Seasons - i + 1)), 0);
                    MakeBranch(Temp, 2, (0.00025 * (_Origin.Altitude + Seasons - i + 1)), 120);
                    MakeBranch(Temp, 2, (0.00025 * (_Origin.Altitude + Seasons - i + 1)), 240);
                }
            }


        }

        protected override void MakeBranch(Coordinate Origin, Double Height, Double UV, Double Bearing)
        {
            List<Coordinate> tempBranch = new List<Coordinate>();
            Double D;
            if (Bearing < 0)
            {
                D = _Randotron.Next(0, 360);
            }
            else
            {
                D = ((Bearing - 45) + _Randotron.Next(0, 90)) % 360;
            }


            Coordinate t = OG.DestinationCoordinate(Origin, UV, D);
            Coordinate t2 = OG.DestinationCoordinate(Origin, UV / 2, D);
            t.Altitude = t.Altitude + Height;
            t2.Altitude = t2.Altitude + Height / 2;
            tempBranch.Add(t);
            //Can you understand this super cool formula? (D-90)%360 the left side and (D + 90) % 360 
            tempBranch.Add(OG.DestinationCoordinate(Origin, UV / 20, (D - 90) % 360));
            tempBranch.Add(OG.DestinationCoordinate(Origin, UV / 20, (D + 90) % 360));
            _Branches.Add(new Geotool_Objects.Polygon(tempBranch));
            _Foliage.Add(new Geotool_Objects.Polygon(OG.GeodesicRotation(OG.MakeGeodesicHexagon(t, (0.0009) * Height), t, (_Randotron.Next(0, 90)) % 360)));
            _Foliage.Add(new Geotool_Objects.Polygon(OG.GeodesicRotation(OG.MakeGeodesicHexagon(t2, (0.0009) * Height), t2, (_Randotron.Next(0, 90)) % 360)));

            if (Height / 2 >= 0.5)
            {
                MakeBranch(t, Height / 3, UV / 3, 0);
                MakeBranch(t, Height / 3, UV / 3, 120);
                MakeBranch(t, Height / 3, UV / 3, 240);
                MakeBranch(t2, Height / 3, UV / 3, 0);
                MakeBranch(t2, Height / 3, UV / 3, 120);
                MakeBranch(t2, Height / 3, UV / 3, 240);
            }

        }

        public WhiteOak()
        {
            Species = "White Oak";
        }

        public override List<SharpKml.Dom.Placemark> ToPlaceMarks(bool low, Season S)
        {
            List<Placemark> returnList = new List<Placemark>();

            foreach (Geotool_Objects.Polygon P in MainStem)
            {
                returnList.Add(OG.SampleStyler(P.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species));

            }
            if (!low)
            {
                foreach (Geotool_Objects.Polygon Branch in _Branches)
                {
                    returnList.Add(OG.SampleStyler(Branch.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species));

                }
            }

            Placemark Trunk = OG.CombineIntoMultiGeometry(returnList);
            returnList = new List<Placemark>();

            foreach (Geotool_Objects.Polygon Foliage in _Foliage)
            {
                returnList.Add(OG.SampleStyler(Foliage.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 107, 53), new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 107, 53), Species + " foliage"));

            }
            Placemark Leaves = OG.CombineIntoMultiGeometry(returnList);
            returnList = new List<Placemark>();

            returnList.Add(Trunk.Clone());
            returnList.Add(Leaves.Clone());

            return returnList;
        }

        public override List<Placemark> ToPlaceMarks()
        {
            List<Placemark> returnList = new List<Placemark>();

            foreach (Geotool_Objects.Polygon P in MainStem)
            {
                returnList.Add(OG.SampleStyler(P.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species));

            }

            foreach (Geotool_Objects.Polygon Branch in _Branches)
            {
                returnList.Add(OG.SampleStyler(Branch.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species));

            }
            Placemark Trunk = OG.CombineIntoMultiGeometry(returnList);
            returnList = new List<Placemark>();

            foreach (Geotool_Objects.Polygon Foliage in _Foliage)
            {
                returnList.Add(OG.SampleStyler(Foliage.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 107, 53), new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 107, 53), Species + " foliage"));

            }
            Placemark Leaves = OG.CombineIntoMultiGeometry(returnList);
            returnList = new List<Placemark>();

            returnList.Add(Trunk.Clone());
            returnList.Add(Leaves.Clone());

            return returnList;
        }

    }

    public class Poplar : Plant
    {

        public Poplar(int Seasons, Coordinate Origin, Double Orientation)
        {
            OG = new GeoTools("Chult");
            _Foliage = new List<Geotool_Objects.Polygon>();
            _Branches = new List<Geotool_Objects.Polygon>();
            _Randotron = new Random();
            Species = "Poplar";


            //We are growing from a seed so we need to add the initial stem(s)

            _Origin = new Coordinate(Origin.Latitude, Origin.Longitude, Origin.Altitude);



            MainStem = MakeStem(_Origin, (Seasons / 2.00), Seasons * 0.00005);

            //Start at the base of the main stem and increment up the stem at 0.5 m increments.
            for (double i = _Origin.Altitude; i <= _Origin.Altitude + (Seasons / 2.00); i += 0.5)
            {
                Coordinate Temp = new Coordinate(_Origin.Latitude, _Origin.Longitude, i);
                //I picked the halway point for this example.
                if (i >= _Origin.Altitude + ((Seasons / 4.00) / 2.00) && _Foliage.Count == 0)
                {
                    //    _Foliage.Add(new Geotool_Objects.Polygon(OG.GeodesicRotation(OG.MakeGeodesicHexagon(Temp, (0.0005 * (_Origin.Altitude + (Seasons / 4.00) - i + 1))), Temp, (_Randotron.Next(0, 90) + Orientation) % 360)));

                }
                if (i >= _Origin.Altitude + ((Seasons / 4.00) / 2.00))
                {
                    //     _Foliage.Add(new Geotool_Objects.Polygon(OG.GeodesicRotation(OG.MakeGeodesicHexagon(Temp, (0.0005 * (_Origin.Altitude + Seasons - i + 1))), Temp, (_Randotron.Next(0, 90) + Orientation) % 360)));
                    MakeBranch(Temp, 2, (0.00025 * (_Origin.Altitude + Seasons - i + 1)), -1);
                    MakeBranch(Temp, 2, (0.00025 * (_Origin.Altitude + Seasons - i + 1)), -1);

                }
            }

        }

        protected override void MakeBranch(Coordinate Origin, Double Height, Double UV, Double Bearing)
        {
            List<Coordinate> tempBranch = new List<Coordinate>();
            Double D;
            if (Bearing < 0)
            {
                D = _Randotron.Next(0, 360);
            }
            else
            {
                D = ((Bearing - 22.5) + _Randotron.Next(0, 45)) % 360;
            }


            Coordinate t = OG.DestinationCoordinate(Origin, UV, D);
            Coordinate t2 = OG.DestinationCoordinate(Origin, UV / 2, D);
            t.Altitude = t.Altitude + Height;
            t2.Altitude = t2.Altitude + Height / 2;
            tempBranch.Add(t);
            //Can you understand this super cool formula? (D-90)%360 the left side and (D + 90) % 360 
            tempBranch.Add(OG.DestinationCoordinate(Origin, UV / 20, (D - 90) % 360));
            tempBranch.Add(OG.DestinationCoordinate(Origin, UV / 20, (D + 90) % 360));
            _Branches.Add(new Geotool_Objects.Polygon(tempBranch));
            _Foliage.Add(new Geotool_Objects.Polygon(OG.GeodesicRotation(OG.MakeGeodesicHexagon(t, (0.0009) * Height), t, (_Randotron.Next(0, 90)) % 360)));
            _Foliage.Add(new Geotool_Objects.Polygon(OG.GeodesicRotation(OG.MakeGeodesicHexagon(t2, (0.0009) * Height), t2, (_Randotron.Next(0, 90)) % 360)));

            if (Height / 3 >= 0.5)
            {
                MakeBranch(t, Height / 2, UV / 2, -1);
                //MakeBranch(t, Height / 2, UV / 2, 120);
                //MakeBranch(t, Height / 2, UV / 2, 240);
                MakeBranch(t2, Height / 2.5, UV / 2, -1);
                //MakeBranch(t2, Height / 2.5, UV / 2, 120);
                //MakeBranch(t2, Height / 2.5, UV / 2, 240);
            }

        }

        public Poplar()
        {
            Species = "Poplar";
        }

        public override List<SharpKml.Dom.Placemark> ToPlaceMarks(bool low, Season S)
        {
            List<Placemark> returnList = new List<Placemark>();

            foreach (Geotool_Objects.Polygon P in MainStem)
            {
                returnList.Add(OG.SampleStyler(P.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species));

            }
            if (!low)
            {
                foreach (Geotool_Objects.Polygon Branch in _Branches)
                {
                    returnList.Add(OG.SampleStyler(Branch.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species));

                }
            }
            Placemark Trunk = OG.CombineIntoMultiGeometry(returnList);
            returnList = new List<Placemark>();

            foreach (Geotool_Objects.Polygon Foliage in _Foliage)
            {
                returnList.Add(OG.SampleStyler(Foliage.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 107, 53), new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 107, 53), Species + " foliage"));

            }
            Placemark Leaves = OG.CombineIntoMultiGeometry(returnList);
            returnList = new List<Placemark>();

            returnList.Add(Trunk.Clone());
            returnList.Add(Leaves.Clone());

            return returnList;
        }

        public override List<Placemark> ToPlaceMarks()
        {
            List<Placemark> returnList = new List<Placemark>();

            foreach (Geotool_Objects.Polygon P in MainStem)
            {
                returnList.Add(OG.SampleStyler(P.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species));

            }

            foreach (Geotool_Objects.Polygon Branch in _Branches)
            {
                returnList.Add(OG.SampleStyler(Branch.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species));

            }
            Placemark Trunk = OG.CombineIntoMultiGeometry(returnList);
            returnList = new List<Placemark>();

            foreach (Geotool_Objects.Polygon Foliage in _Foliage)
            {
                returnList.Add(OG.SampleStyler(Foliage.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 107, 53), new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 107, 53), Species + " foliage"));

            }
            Placemark Leaves = OG.CombineIntoMultiGeometry(returnList);
            returnList = new List<Placemark>();

            returnList.Add(Trunk.Clone());
            returnList.Add(Leaves.Clone());

            return returnList;
        }

    }

    public class WhiteSpruce : Plant
    {

        public WhiteSpruce(int Seasons, Coordinate Origin, Double Orientation)
        {
            OG = new GeoTools("Chult");
            _Foliage = new List<Geotool_Objects.Polygon>();
            _Branches = new List<Geotool_Objects.Polygon>();
            _Randotron = new Random();
            Species = "White Spruce";


            //We are growing from a seed so we need to add the initial stem(s)

            _Origin = new Coordinate(Origin.Latitude, Origin.Longitude, Origin.Altitude);



            MainStem = MakeStem(_Origin, (Seasons * 2), Seasons * 0.00005);

            //Start at the base of the main stem and increment up the stem at 0.5 m increments.
            for (double i = _Origin.Altitude; i <= _Origin.Altitude + (Seasons * 2); i += 0.5)
            {
                Coordinate Temp = new Coordinate(_Origin.Latitude, _Origin.Longitude, i);
                //I picked the halway point for this example.

                //This is where you want your recursion to start along a stem. 
                if (i >= _Origin.Altitude + ((Seasons * 2) / 7.00))
                {
                    //     _Foliage.Add(new Geotool_Objects.Polygon(OG.GeodesicRotation(OG.MakeGeodesicHexagon(Temp, (0.0005 * (_Origin.Altitude + Seasons - i + 1))), Temp, (_Randotron.Next(0, 90) + Orientation) % 360)));
                    MakeBranch(Temp, -0.5, (0.00025 * (_Origin.Altitude + (Seasons * 2) - i + 1)), -1);

                }
            }

            //Some how grow recursively.



        }

        protected override void MakeBranch(Coordinate Origin, Double Height, Double UV, Double Bearing)
        {
            List<Coordinate> tempBranch = new List<Coordinate>();
            Double D;
            if (Bearing < 0)
            {
                D = _Randotron.Next(0, 360);
            }
            else
            {
                D = ((Bearing - 45) + _Randotron.Next(0, 90)) % 360;
            }


            Coordinate t = OG.DestinationCoordinate(Origin, UV, D);
            Coordinate t2 = OG.DestinationCoordinate(Origin, UV / 2, D);
            t.Altitude = t.Altitude + Height;
            t2.Altitude = t2.Altitude + Height / 2;
            tempBranch.Add(t);
            //Can you understand this super cool formula? (D-90)%360 the left side and (D + 90) % 360 
            tempBranch.Add(OG.DestinationCoordinate(Origin, UV / 20, (D - 90) % 360));
            tempBranch.Add(OG.DestinationCoordinate(Origin, UV / 20, (D + 90) % 360));
            _Branches.Add(new Geotool_Objects.Polygon(tempBranch));


            //recycling the tempbranch with a code change. :)

            List<Coordinate> t3 = OG.GeodesicRotation(OG.MakeGeodesicEquilateralTriangleList(UV / 5.00, t), t, D % 360);
            t3[1].Altitude = t3[1].Altitude - 2;
            List<Coordinate> t4 = OG.GeodesicRotation(OG.MakeGeodesicEquilateralTriangleList(UV / 5.00, t2), t2, D % 360);
            t4[1].Altitude = t4[1].Altitude - 2;
            List<Coordinate> t5 = OG.GeodesicRotation(OG.MakeGeodesicEquilateralTriangleList(UV / 5.00, t), t, D + 180 % 360);
            t5[1].Altitude = t5[1].Altitude - 2;
            List<Coordinate> t6 = OG.GeodesicRotation(OG.MakeGeodesicEquilateralTriangleList(UV / 5.00, t2), t2, D + 180 % 360);
            t6[1].Altitude = t6[1].Altitude - 2;
            _Foliage.Add(new Geotool_Objects.Polygon(t3));
            _Foliage.Add(new Geotool_Objects.Polygon(t4));
            _Foliage.Add(new Geotool_Objects.Polygon(t5));
            _Foliage.Add(new Geotool_Objects.Polygon(t6));




        }

        public WhiteSpruce()
        {
            Species = "White Spruce";
        }

        public override List<SharpKml.Dom.Placemark> ToPlaceMarks(bool low, Season S)
        {
            List<Placemark> returnList = new List<Placemark>();

            foreach (Geotool_Objects.Polygon P in MainStem)
            {
                returnList.Add(OG.SampleStyler(P.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species));

            }
            if (!low)
            {
                foreach (Geotool_Objects.Polygon Branch in _Branches)
                {
                    returnList.Add(OG.SampleStyler(Branch.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species + " foliage"));

                }
            }
            Placemark Trunk = OG.CombineIntoMultiGeometry(returnList);
            returnList = new List<Placemark>();

            foreach (Geotool_Objects.Polygon Foliage in _Foliage)
            {
                returnList.Add(OG.SampleStyler(Foliage.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 107, 53), new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 107, 53), ""));

            }
            Placemark Leaves = OG.CombineIntoMultiGeometry(returnList);
            returnList = new List<Placemark>();

            returnList.Add(Trunk.Clone());
            returnList.Add(Leaves.Clone());

            return returnList;
        }

        public override List<Placemark> ToPlaceMarks()
        {
            List<Placemark> returnList = new List<Placemark>();

            foreach (Geotool_Objects.Polygon P in MainStem)
            {
                returnList.Add(OG.SampleStyler(P.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species));

            }

            foreach (Geotool_Objects.Polygon Branch in _Branches)
            {
                returnList.Add(OG.SampleStyler(Branch.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species + " foliage"));

            }
            Placemark Trunk = OG.CombineIntoMultiGeometry(returnList);
            returnList = new List<Placemark>();

            foreach (Geotool_Objects.Polygon Foliage in _Foliage)
            {
                returnList.Add(OG.SampleStyler(Foliage.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 107, 53), new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 107, 53), ""));

            }
            Placemark Leaves = OG.CombineIntoMultiGeometry(returnList);
            returnList = new List<Placemark>();

            returnList.Add(Trunk.Clone());
            returnList.Add(Leaves.Clone());

            return returnList;
        }

    }

    public class GenericConifer : Plant
    {

        public GenericConifer(int Seasons, Coordinate Origin, Double Orientation)
        {
            OG = new GeoTools("Chult");
            _Foliage = new List<Geotool_Objects.Polygon>();
            _Branches = new List<Geotool_Objects.Polygon>();
            _Randotron = new Random();
            Species = "Generic Conifer";


            //We are growing from a seed so we need to add the initial stem(s)

            _Origin = new Coordinate(Origin.Latitude, Origin.Longitude, Origin.Altitude);



            MainStem = MakeStem(_Origin, Seasons, Seasons * 0.00005);

            //Start at the base of the main stem and increment up the stem at 0.5 m increments.
            for (double i = _Origin.Altitude; i <= _Origin.Altitude + Seasons; i += 0.5)
            {
                Coordinate Temp = new Coordinate(_Origin.Latitude, _Origin.Longitude, i);
                //I picked the halway point for this example.
                if (i >= _Origin.Altitude + (Seasons / 2.00))
                {
                    _Foliage.Add(new Geotool_Objects.Polygon(OG.GeodesicRotation(OG.MakeGeodesicHexagon(Temp, (0.0005 * (_Origin.Altitude + Seasons - i + 1))), Temp, (_Randotron.Next(0, 90) + Orientation) % 360)));

                }
            }



        }

        protected override void MakeBranch(Coordinate Origin, Double Height, Double UV, Double Bearing)
        {
            List<Coordinate> tempBranch = new List<Coordinate>();
            Double D = _Randotron.Next(0, 360);
            Coordinate t = OG.DestinationCoordinate(Origin, UV, D);
            Coordinate t2 = OG.DestinationCoordinate(Origin, UV / 2, D);
            t.Altitude = t.Altitude + Height;
            t2.Altitude = t2.Altitude + Height / 2;
            tempBranch.Add(t);
            //Can you understand this super cool formula? (D-90)%360 the left side and (D + 90) % 360 
            tempBranch.Add(OG.DestinationCoordinate(Origin, UV / 20, (D - 90) % 360));
            tempBranch.Add(OG.DestinationCoordinate(Origin, UV / 20, (D + 90) % 360));
            _Branches.Add(new Geotool_Objects.Polygon(tempBranch));
            _Foliage.Add(new Geotool_Objects.Polygon(OG.GeodesicRotation(OG.MakeGeodesicHexagon(t, (0.0009) * Height), t, (_Randotron.Next(0, 90)) % 360)));
            _Foliage.Add(new Geotool_Objects.Polygon(OG.GeodesicRotation(OG.MakeGeodesicHexagon(t2, (0.0009) * Height), t2, (_Randotron.Next(0, 90)) % 360)));

            if (Height / 2 >= 0.5)
            {
                MakeBranch(t, Height / 3, UV / 3, 120);
                MakeBranch(t, Height / 3, UV / 3, 240);
                MakeBranch(t, Height / 3, UV / 3, 0);
                MakeBranch(t2, Height / 3, UV / 3, 120);
                MakeBranch(t2, Height / 3, UV / 3, 240);
                MakeBranch(t2, Height / 3, UV / 3, 0);
            }

        }

        private GenericConifer()
        {
            Species = "Generic Conifer";
        }

        public override List<SharpKml.Dom.Placemark> ToPlaceMarks(bool low, Season S)
        {
            List<Placemark> returnList = new List<Placemark>();

            foreach (Geotool_Objects.Polygon P in MainStem)
            {
                returnList.Add(OG.SampleStyler(P.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species));

            }
            Placemark Trunk = OG.CombineIntoMultiGeometry(returnList);
            returnList.Clear();




            foreach (Geotool_Objects.Polygon Foliage in _Foliage)
            {
                returnList.Add(OG.SampleStyler(Foliage.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 107, 53), new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 107, 53), Species));

            }
            Placemark Leaves = OG.CombineIntoMultiGeometry(returnList);
            returnList.Clear();

            if (!low)
            {
                foreach (Geotool_Objects.Polygon Branch in _Branches)
                {
                    returnList.Add(OG.SampleStyler(Branch.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species + " foliage"));

                }
            }



            returnList.Add(Trunk);
            returnList.Add(Leaves);
            return returnList;
        }


        public override List<Placemark> ToPlaceMarks()
        {
            List<Placemark> returnList = new List<Placemark>();

            foreach (Geotool_Objects.Polygon P in MainStem)
            {
                returnList.Add(OG.SampleStyler(P.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species));

            }
            Placemark Trunk = OG.CombineIntoMultiGeometry(returnList);
            returnList.Clear();




            foreach (Geotool_Objects.Polygon Foliage in _Foliage)
            {
                returnList.Add(OG.SampleStyler(Foliage.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 107, 53), new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 107, 53), Species));

            }
            Placemark Leaves = OG.CombineIntoMultiGeometry(returnList);
            returnList.Clear();


            foreach (Geotool_Objects.Polygon Branch in _Branches)
            {
                returnList.Add(OG.SampleStyler(Branch.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species + " foliage"));

            }



            returnList.Add(Trunk);
            returnList.Add(Leaves);
            return returnList;
        }


    }

    public class Apple : Plant
    {
        public Apple(int Seasons, Coordinate Origin, Double Orientation)
        {
            OG = new GeoTools("Chult");
            _Foliage = new List<Geotool_Objects.Polygon>();
            _Branches = new List<Geotool_Objects.Polygon>();
            _Randotron = new Random();
            Species = "Apple";


            //We are growing from a seed so we need to add the initial stem(s)

            _Origin = new Coordinate(Origin.Latitude, Origin.Longitude, Origin.Altitude);



            MainStem = MakeStem(_Origin, (Seasons / 2.00), Seasons * 0.00005);

            //Start at the base of the main stem and increment up the stem at 0.5 m increments.
            for (double i = _Origin.Altitude; i <= _Origin.Altitude + (Seasons / 2.00); i += 0.5)
            {
                Coordinate Temp = new Coordinate(_Origin.Latitude, _Origin.Longitude, i);
                //I picked the halway point for this example.
                if (i >= _Origin.Altitude + ((Seasons / 3.00) / 2.00) && _Foliage.Count == 0)
                {
                    //    _Foliage.Add(new Geotool_Objects.Polygon(OG.GeodesicRotation(OG.MakeGeodesicHexagon(Temp, (0.0005 * (_Origin.Altitude + (Seasons / 4.00) - i + 1))), Temp, (_Randotron.Next(0, 90) + Orientation) % 360)));

                }
                if (i >= _Origin.Altitude + ((Seasons / 3.00) / 2.00))
                {
                    //      _Foliage.Add(new Geotool_Objects.Polygon(OG.GeodesicRotation(OG.MakeGeodesicHexagon(Temp, (0.0005 * (_Origin.Altitude + Seasons - i + 1))), Temp, (_Randotron.Next(0, 90) + Orientation) % 360)));
                    MakeBranch(Temp, 1, (0.00025 * (_Origin.Altitude + Seasons - i + 2)), -1);
                    MakeBranch(Temp, 1.75, (0.00025 * (_Origin.Altitude + Seasons - i + 1)), -1);

                }
            }



        }

        protected override void MakeBranch(Coordinate Origin, Double Height, Double UV, Double Bearing)
        {
            List<Coordinate> tempBranch = new List<Coordinate>();
            Double D;
            if (Bearing < 0)
            {
                D = _Randotron.Next(0, 360);
            }
            else
            {
                D = ((Bearing - 22.5) + _Randotron.Next(0, 45)) % 360;
            }


            Coordinate t = OG.DestinationCoordinate(Origin, UV, D);
            Coordinate t2 = OG.DestinationCoordinate(Origin, UV / 2, D);
            t.Altitude = t.Altitude + Height;
            t2.Altitude = t2.Altitude + Height / 2.1;
            tempBranch.Add(t);
            //Can you understand this super cool formula? (D-90)%360 the left side and (D + 90) % 360 
            tempBranch.Add(OG.DestinationCoordinate(Origin, UV / 20, (D - 90) % 360));
            tempBranch.Add(OG.DestinationCoordinate(Origin, UV / 20, (D + 90) % 360));
            _Branches.Add(new Geotool_Objects.Polygon(tempBranch));
            _Foliage.Add(new Geotool_Objects.Polygon(OG.GeodesicRotation(OG.MakeGeodesicHexagon(t, UV), t, (_Randotron.Next(0, 90)) % 360)));
            _Foliage.Add(new Geotool_Objects.Polygon(OG.GeodesicRotation(OG.MakeGeodesicHexagon(t2, UV), t2, (_Randotron.Next(0, 90)) % 360)));

            if (Height / 4 >= 0.5)
            {
                MakeBranch(t, Height / 2, UV / 2, -1);
                //MakeBranch(t, Height / 2, UV / 2, 120);
                //MakeBranch(t, Height / 2, UV / 2, 240);
                // MakeBranch(t2, Height / 2, UV / 2, -1);
                //MakeBranch(t2, Height / 2.5, UV / 2, 120);
                //MakeBranch(t2, Height / 2.5, UV / 2, 240);
            }

        }

        public Apple()
        {
            Species = "Apple";
        }

        public override List<SharpKml.Dom.Placemark> ToPlaceMarks(bool low, Season S)
        {
            List<Placemark> returnList = new List<Placemark>();

            foreach (Geotool_Objects.Polygon P in MainStem)
            {
                returnList.Add(OG.SampleStyler(P.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species));

            }
            //If we are low we skip rendering any branches. What if we are low and fall???

            if (!low)
            {
                foreach (Geotool_Objects.Polygon Branch in _Branches)
                {
                    returnList.Add(OG.SampleStyler(Branch.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species));

                }

            }
            Placemark Trunk = OG.CombineIntoMultiGeometry(returnList);
            returnList = new List<Placemark>();




            foreach (Geotool_Objects.Polygon Foliage in _Foliage)
            {
                returnList.Add(OG.SampleStyler(Foliage.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 107, 53), new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 107, 53), Species + " foliage"));

            }
            Placemark Leaves = OG.CombineIntoMultiGeometry(returnList);
            returnList = new List<Placemark>();

            returnList.Add(Trunk.Clone());
            returnList.Add(Leaves.Clone());

            return returnList;
        }

        public override List<Placemark> ToPlaceMarks()
        {
            List<Placemark> returnList = new List<Placemark>();

            foreach (Geotool_Objects.Polygon P in MainStem)
            {
                returnList.Add(OG.SampleStyler(P.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species));

            }

            foreach (Geotool_Objects.Polygon Branch in _Branches)
            {
                returnList.Add(OG.SampleStyler(Branch.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species));

            }
            Placemark Trunk = OG.CombineIntoMultiGeometry(returnList);
            returnList = new List<Placemark>();

            foreach (Geotool_Objects.Polygon Foliage in _Foliage)
            {
                returnList.Add(OG.SampleStyler(Foliage.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 107, 53), new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 107, 53), Species + " foliage"));

            }
            Placemark Leaves = OG.CombineIntoMultiGeometry(returnList);
            returnList = new List<Placemark>();

            returnList.Add(Trunk.Clone());
            returnList.Add(Leaves.Clone());

            return returnList;
        }
    }

    public class Biome : Plant
    {
        private Geotool_Objects.Polygon _Polygon;
        public List<List<Placemark>> PlantFoliage; //We have different colours of leaf
        public List<List<Placemark>> PlantBranches; //We will have different colours of wood.

        private List<Placemark> returnList;

        public Biome(List<Coordinate> coordinates, List<Plant> AllowedSpecies, int MinSeason, int MaxSeason, bool low, Season S)
        {
            OG = new GeoTools("Chult");
            _Randotron = new Random();
            PlantFoliage = new List<List<Placemark>>(); //We have different colours of leaf
            PlantBranches = new List<List<Placemark>>(); //We will have different colours of wood.
            //Some of the metrics I need to make a biome with the right "feel". Botanists in the audience?
            Double Area = OG.GetArea(coordinates) / 100;
            Coordinate avg = OG.GetAverage(coordinates);
            Double Distance = OG.GetMaxDistance(avg, coordinates);
            Double MinDistance = OG.GetMinDistance(avg, coordinates);

            //Build a few more parts I need to build the biome.
            Double AreaCounter = Area;
            _Polygon = new Geotool_Objects.Polygon(coordinates);
            //Coordinate Origin = OG.GetAverage(coordinates);
            returnList = new List<Placemark>();

            //Imagine a circle with a center mark. Into this we are going to build the following parts.
            //1. ONE polygon with a green background and a splotch cut out
            //2. ONE multpoly of a lighter tansparent colour 1 meter above ground in splotch patterns
            //3. TWO Mulltipolys for The Trees By Species. (foliage and Branch)

            //The number of each object and the obect scale has a lot to do wth the size of the polygon of the biome.
            double bearing = 0;
            Coordinate Potential;
            bool IsInside = true;
            List<Coordinate> PotentialSplotch;
            while (AreaCounter > 0)
            {
                IsInside = true;
                bearing = _Randotron.NextDouble() * 360;
                Potential = OG.DestinationCoordinate(avg, _Randotron.NextDouble() * Distance, bearing);

                PotentialSplotch = OG.MakeGeodesicRandomSplotch(Potential, _Randotron.NextDouble() * 0.0020);
                foreach (Coordinate c in PotentialSplotch)
                {
                    if (!OG.IsPointInPolygon4(coordinates, c))
                    {
                        IsInside = false;
                        break;
                    }
                }
                if (IsInside)
                {
                    _Polygon.InnerBoundaryList.Add(PotentialSplotch);
                    AreaCounter -= 1;
                }
            }

            //Reset Area Counter
            AreaCounter = Area;
            while (AreaCounter > 0)
            {
                IsInside = true;
                bearing = _Randotron.NextDouble() * 360;
                Potential = OG.DestinationCoordinate(avg, _Randotron.NextDouble() * Distance, bearing);
                Potential.Altitude = Potential.Altitude + 1;
                PotentialSplotch = OG.MakeGeodesicRandomSplotch(Potential, _Randotron.NextDouble() * 0.0020);
                foreach (Coordinate c in PotentialSplotch)
                {
                    if (!OG.IsPointInPolygon4(coordinates, c))
                    {
                        IsInside = false;
                        break;
                    }
                }
                if (IsInside)
                {
                    returnList.Add(OG.SampleStyler(new Geotool_Objects.Polygon(PotentialSplotch).ToPlaceMark(),
                                                      false,
                                                      SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor,
                                                      new Color32(127, 0, 200, 70), new Color32(127, 0, 206, 137),
                                                     "Biome Foliage"));
                    AreaCounter -= 1;
                }
            }

            Placemark Foliage = OG.CombineIntoMultiGeometry(returnList);
            Foliage.Name = "Ground Cover";
            returnList.Clear();

            //This little nonesense acts like a sorting hat. I make the number of slots for the number of species.
            for (int i = 0; i < AllowedSpecies.Count; ++i)
            {
                PlantFoliage.Add(new List<Placemark>());
                PlantBranches.Add(new List<Placemark>());
            }
            //Then I top off the last bit with the ground cover that is 1 meter above ground.
            PlantFoliage.Add(new List<Placemark>());
            PlantFoliage.Last().Add(Foliage);


            //Reset our variables for the third round.
            AreaCounter = Area;

            List<Placemark> temp = new List<Placemark>();
            while (AreaCounter > 0)
            {
                IsInside = true;
                bearing = _Randotron.NextDouble() * 360;
                Potential = OG.DestinationCoordinate(avg, _Randotron.NextDouble() * Distance, bearing);
                Potential.Altitude = Potential.Altitude - 0.1;

                if (!OG.IsPointInPolygon4(coordinates, Potential))
                {
                    IsInside = false;

                }

                if (IsInside)
                {

                    int SpeciesSlot = _Randotron.Next(0, AllowedSpecies.Count - 1);

                    //
                    switch (AllowedSpecies[SpeciesSlot].GetSpecies())
                    {
                        case "Generic Deciduous":

                            temp = new GenericDeciduous(_Randotron.Next(MinSeason, MaxSeason), Potential, 0).ToPlaceMarks(low, S);
                            PlantFoliage[SpeciesSlot].Add(temp.First());
                            PlantBranches[SpeciesSlot].Add(temp.Last());
                            break;
                        case "Generic Deciduous II":
                            temp = new GenericDeciduousII(_Randotron.Next(MinSeason, MaxSeason), Potential, 0).ToPlaceMarks(low, S);
                            PlantFoliage[SpeciesSlot].Add(temp.First());
                            PlantBranches[SpeciesSlot].Add(temp.Last());
                            break;
                        case "White Oak":
                            temp = new WhiteOak(_Randotron.Next(MinSeason, MaxSeason), Potential, 0).ToPlaceMarks(low, S);
                            PlantFoliage[SpeciesSlot].Add(temp.First());
                            PlantBranches[SpeciesSlot].Add(temp.Last());
                            break;
                        case "Poplar":
                            temp = new Poplar(_Randotron.Next(MinSeason, MaxSeason), Potential, 0).ToPlaceMarks(low, S);
                            PlantFoliage[SpeciesSlot].Add(temp.First());
                            PlantBranches[SpeciesSlot].Add(temp.Last());
                            break;
                        case "White Spruce":
                            temp = new WhiteSpruce(_Randotron.Next(MinSeason, MaxSeason), Potential, 0).ToPlaceMarks(low, S);
                            PlantFoliage[SpeciesSlot].Add(temp.First());
                            PlantBranches[SpeciesSlot].Add(temp.Last());
                            break;
                        case "Generic Conifer":
                            temp = new GenericConifer(_Randotron.Next(MinSeason, MaxSeason), Potential, 0).ToPlaceMarks(low, S);
                            PlantFoliage[SpeciesSlot].Add(temp.First());
                            PlantBranches[SpeciesSlot].Add(temp.Last());
                            break;
                        case "Apple":
                            temp = new Apple(_Randotron.Next(MinSeason, MaxSeason), Potential, 0).ToPlaceMarks(low, S);
                            PlantFoliage[SpeciesSlot].Add(temp.First());
                            PlantBranches[SpeciesSlot].Add(temp.Last());
                            break;
                        default:
                            break;

                    }

                    AreaCounter -= 1;
                }
            }



        }

        public override List<SharpKml.Dom.Placemark> ToPlaceMarks(bool low, Season S)
        {
            return ToPlaceMarks();
        }

        public override List<Placemark> ToPlaceMarks()
        {

            Placemark q = OG.SampleStyler(_Polygon.ToPlaceMark(),
                                             false,
                                             SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor,
                                             new Color32(127, 0, 107, 53),
                                             new Color32(127, 0, 107, 53),
                                             "Biome Forest");
            returnList.Clear();
            returnList.Add(q);
            try
            {
                foreach (List<Placemark> list in PlantFoliage)

                {
                    if (list.Count > 0)
                    {
                        returnList.Add(OG.CombineIntoMultiGeometry(list));
                    }
                }
                foreach (List<Placemark> list in PlantBranches)
                {
                    if (list.Count > 0)
                    {
                        returnList.Add(OG.CombineIntoMultiGeometry(list));
                    }

                }
            }
            catch (Exception ex)
            {
                int x = 0;
            }


            return returnList;
        }

        protected override void MakeBranch(Coordinate Origin, double Height, double UV, double Bearing)
        {
            throw new NotImplementedException();
        }


    }

    public class PlantLine : Plant
    {
        List<Placemark> PlantFoliage;
        List<Placemark> PlantBranches;

        /// <summary>
        /// Woot Sprint 3! Lets build some biomes and tree lines and collections of life.
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="Species"></param>
        /// <param name="MinSeason"></param>
        /// <param name="MaxSeason"></param>
        public PlantLine(List<Coordinate> coords, Plant Species, int MinSeason, int MaxSeason, bool low, Season S)
        {
            OG = new GeoTools("Chult");
            _Randotron = new Random();
            _Foliage = new List<Geotool_Objects.Polygon>();
            _Branches = new List<Geotool_Objects.Polygon>();
            MainStem = new List<Geotool_Objects.Polygon>();

            PlantFoliage = new List<Placemark>();
            PlantBranches = new List<Placemark>();
            List<Placemark> temp = new List<Placemark>();


            foreach (Coordinate C in coords)
            {
                switch (Species.GetSpecies())
                {
                    case "Generic Deciduous":

                        temp = new GenericDeciduous(_Randotron.Next(MinSeason, MaxSeason), C, 0).ToPlaceMarks(low, S);
                        PlantFoliage.Add(temp[0].Clone());
                        PlantBranches.Add(temp[1].Clone());
                        break;
                    case "Generic Deciduous II":
                        temp = new GenericDeciduousII(_Randotron.Next(MinSeason, MaxSeason), C, 0).ToPlaceMarks(low, S);
                        PlantFoliage.Add(temp[0].Clone());
                        PlantBranches.Add(temp[1].Clone());
                        break;
                    case "White Oak":
                        temp = new WhiteOak(_Randotron.Next(MinSeason, MaxSeason), C, 0).ToPlaceMarks(low, S);
                        PlantFoliage.Add(temp[0].Clone());
                        PlantBranches.Add(temp[1].Clone());
                        break;
                    case "Poplar":
                        temp = new Poplar(_Randotron.Next(MinSeason, MaxSeason), C, 0).ToPlaceMarks(low, S);
                        PlantFoliage.Add(temp[0].Clone());
                        PlantBranches.Add(temp[1].Clone());
                        break;
                    case "White Spruce":
                        temp = new WhiteSpruce(_Randotron.Next(MinSeason, MaxSeason), C, 0).ToPlaceMarks(low, S);
                        PlantFoliage.Add(temp[0].Clone());
                        PlantBranches.Add(temp[1].Clone());
                        break;
                    case "Generic Conifer":
                        temp = new GenericConifer(_Randotron.Next(MinSeason, MaxSeason), C, 0).ToPlaceMarks(low, S);
                        PlantFoliage.Add(temp[0].Clone());
                        PlantBranches.Add(temp[1].Clone());
                        break;
                    case "Apple":
                        temp = new Apple(_Randotron.Next(MinSeason, MaxSeason), C, 0).ToPlaceMarks(low, S);
                        PlantFoliage.Add(temp[0].Clone());
                        PlantBranches.Add(temp[1].Clone());
                        break;
                    default:
                        break;

                }
            }


        }

        private PlantLine()
        { }

        public override List<Placemark> ToPlaceMarks(bool low, Season S)
        {
            return ToPlaceMarks();
        }
        public override List<Placemark> ToPlaceMarks()
        {
            List<Placemark> returnList = new List<Placemark>();
            List<Placemark> tempLeafList = new List<Placemark>();
            List<Placemark> tempBranchList = new List<Placemark>();

            foreach (Geotool_Objects.Polygon P in MainStem)
            {
                tempBranchList.Add(OG.SampleStyler(P.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species));

            }

            foreach (Geotool_Objects.Polygon Branch in _Branches)
            {
                tempBranchList.Add(OG.SampleStyler(Branch.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(255, 83, 103, 134), new Color32(255, 83, 103, 134), Species));

            }

            foreach (Geotool_Objects.Polygon Foliage in _Foliage)
            {
                tempLeafList.Add(OG.SampleStyler(Foliage.ToPlaceMark(), false, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 107, 53), new Color32(Convert.ToByte(_Randotron.Next(127, 255)), 0, 107, 53), Species + " foliage"));

            }

            foreach (Placemark PlantPart in PlantFoliage)
            {
                tempLeafList.Add(PlantPart.Clone());
            }

            foreach (Placemark PlantPart in PlantBranches)
            {
                tempBranchList.Add(PlantPart.Clone());
            }

            returnList.Add(OG.CombineIntoMultiGeometry(tempLeafList));
            returnList.Add(OG.CombineIntoMultiGeometry(tempBranchList));


            return returnList;
        }

        protected override void MakeBranch(Coordinate Origin, double Height, double UV, double Bearing)
        {
            throw new NotImplementedException();
        }
    }

    //This bad boy will change colour of polygon based on matrix of elevation, latitude, modifier 

    public class SeasonalContourMachine
    {
        public Season _Season;
        public SharpKml.Dom.Placemark temp;

        public SeasonalContourMachine() { }

        public List<Placemark> MakeSeasonalEquitorialContours(Season season, List<Placemark> contourCollection, Color32 EnviromentalOffset)
        {
            List<Placemark> returnList = new List<Placemark>();
            GeoTools Thingamabob = new GeoTools("SeasonalStyler");

            //Switch on Seasons
            switch (season)
            {
                case Season.Spring:
                    break;
                case Season.Summer:
                    break;
                case Season.Fall:
                    break;
                case Season.Winter:

                    foreach (Placemark p in contourCollection)
                    {
                        throw new NotImplementedException();


                        //     returnList.Add(Thingamabob.SampleStyler(p, true, SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor, Color32 Fill, Color32 LineColor, p.Name)) ;
                    }
                    break;
            }


            return returnList;
        }

    }

    public class BlockOfBuilding : Structure
    {



        public BlockOfBuilding(Coordinate Origin, double Orientation, double Scale, double LocalGrade)
        {

        }

        public override List<SharpKml.Dom.Placemark> ToPlaceMarks()
        {
            List<SharpKml.Dom.Placemark> returnList = new List<SharpKml.Dom.Placemark>();





            return returnList;
        }
    }

    public class RoundTower : Structure
    {
        public Geotool_Objects.MultiPolygon _BuildingShape;
        private Geotool_Objects.GeoTools OG;

        private RoundTower() { }

        public RoundTower(Coordinate Origin, double Orientation, double Scale)
        {
            OG = new GeoTools("Clay");
            _BuildingShape = new MultiPolygon();
            Origin.Altitude = Origin.Altitude + (20 * Scale);

            Coordinate West = OG.DestinationCoordinate(Origin, (0.010 * Scale), 270);
            Coordinate East = OG.DestinationCoordinate(Origin, (0.010 * Scale), 90);
            Coordinate South = OG.DestinationCoordinate(Origin, (0.010 * Scale), 180);
            Coordinate LowerOrigin = new Coordinate(Origin.Latitude, Origin.Longitude, Origin.Altitude - 12);
            Coordinate North = OG.DestinationCoordinate(LowerOrigin, (0.0173 * Scale), 0);
            Coordinate NW = OG.DestinationCoordinate(North, (0.005 * Scale), 270);
            NW.Altitude = NW.Altitude - (2 * Scale);
            Coordinate NE = OG.DestinationCoordinate(North, (0.005 * Scale), 90);
            NE.Altitude = NE.Altitude - (2 * Scale);
            Coordinate SW = OG.DestinationCoordinate(NW, (0.0173 * Scale), 180);
            Coordinate SE = OG.DestinationCoordinate(SW, (0.010 * Scale), 90);

            Geotool_Objects.Polygon LeftRoof = new Geotool_Objects.Polygon();
            Geotool_Objects.Polygon RightRoof = new Geotool_Objects.Polygon();
            LeftRoof.LinearList.Add(LowerOrigin);
            LeftRoof.LinearList.Add(North);
            LeftRoof.LinearList.Add(NW);
            LeftRoof.LinearList.Add(SW);


            RightRoof.LinearList.Add(SE);

            RightRoof.LinearList.Add(NE);

            RightRoof.LinearList.Add(North);

            RightRoof.LinearList.Add(LowerOrigin);


            // List<Coordinate> WestTower  = 
            Geotool_Objects.Polygon WestTower = new Geotool_Objects.Polygon();
            WestTower.LinearList = OG.GeodesicRotation(OG.MakeGeodesicCircleList(0.0045 * Scale, West), Origin, Orientation);
            Geotool_Objects.Polygon EastTower = new Geotool_Objects.Polygon();
            EastTower.LinearList = OG.GeodesicRotation(OG.MakeGeodesicCircleList(0.0045 * Scale, East), Origin, Orientation); ;
            Geotool_Objects.Polygon SouthTower = new Geotool_Objects.Polygon();
            SouthTower.LinearList = OG.GeodesicRotation(OG.MakeGeodesicCircleList(0.0045 * Scale, South), Origin, Orientation);



            //List<Coordinate> EastTower = OG.MakeGeodesicCircleList(0.0045*Scale, East);
            //List<Coordinate> SouthTower = OG.MakeGeodesicCircleList(0.0045*Scale, South);
            //Just an update to the dataset with this shape placed using this function.
            //I build the building itself
            Geotool_Objects.Polygon BuildingProper = new Geotool_Objects.Polygon();
            //Can you decipher this nested call? THink about the pattern of things inside of things.
            //See how because I use the distance from the West and South the scaling will be correct?
            Origin.Altitude = Origin.Altitude - (20 * Scale) + (10 * Scale);
            BuildingProper.LinearList = OG.GeodesicRotation(OG.MakeGeodesicSquareList(OG.Distance(West, South), Origin), Origin, (Orientation + 45) % 360);



            _BuildingShape.PolyGonList.Add(WestTower);
            _BuildingShape.PolyGonList.Add(EastTower);
            _BuildingShape.PolyGonList.Add(SouthTower);
            _BuildingShape.PolyGonList.Add(BuildingProper);
            _BuildingShape.PolyGonList.Add(LeftRoof);
            _BuildingShape.PolyGonList.Add(RightRoof);

        }

        public override List<SharpKml.Dom.Placemark> ToPlaceMarks()
        {
            List<SharpKml.Dom.Placemark> returnList = new List<SharpKml.Dom.Placemark>();

            foreach (Geotool_Objects.Polygon MP in _BuildingShape.PolyGonList)
            {
                Placemark Temp = new Placemark();
                Temp = OG.SampleStyler(MP.ToPlaceMark(),
                                                        true,
                                                        SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor,
                                                        new Color32(255, 93, 209, 235),
                                                        new Color32(255, 0, 0, 0),
                                                        "Hospice of St. Laupseen");
                returnList.Add(Temp.Clone());
            }




            return returnList;
        }
    }

    public class TeotihuacanPyramid : Structure
    {
        public List<MultiPolygon> Pyramid { get; set; }
        private TeotihuacanPyramid() { }



        public TeotihuacanPyramid(Double Scale, Double Orientation, int Platforms, Coordinate Origin)
        {
            //Lets wrap Orientation in a Modulo
            _Orientation = Orientation % 360;

            _Origin = Origin;
            GeoTools T = new GeoTools("Clay");

            //I assume the pyramid get larger based on unit scale. 
            //Use a loop to create the layers.

            Pyramid = new List<MultiPolygon>();

            MultiPolygon Base = new MultiPolygon();
            MultiPolygon Walkways = new MultiPolygon();
            MultiPolygon Stairs = new MultiPolygon();
            Geotool_Objects.Polygon LevelTheLevel = new Geotool_Objects.Polygon();
            List<Coordinate> Stair = new List<Coordinate>();
            Coordinate A;
            Coordinate B;
            Coordinate C;
            Coordinate D;
            List<Coordinate> Platform;
            List<Coordinate> PlatformTop;

            for (int i = Platforms, q = 1; i > 0; --i, ++q)
            {
                if (Platforms == 1 || i == Platforms || i == Platforms - 1)
                {
                    Platform = T.MakeGeodesicSquareList(Scale * q, Origin);
                    PlatformTop = T.MakeGeodesicSquareList((Scale * q) - 0.001, Origin);
                }
                else
                {
                    Platform = T.MakeGeodesicSquareList((Scale * 2) + (Scale * (q - 2) / 2), Origin);
                    PlatformTop = T.MakeGeodesicSquareList((Scale * 2) + (Scale * (q - 2) / 2) - 0.001, Origin);
                }


                if (Platforms == 1 && i == 1)
                {
                    Stair.Add(T.DestinationCoordinate(Platform.Last(), 0.004, 90));
                    Stair.Add(T.DestinationCoordinate(Platform.Last(), 0.004, 270));
                    Stair.Add(T.DestinationCoordinate(Stair[1], Scale * (Platforms) / 4, 0));
                    Stair.Add(T.DestinationCoordinate(Stair[0], Scale * (Platforms) / 4, 0));
                }
                if (Platforms != 1 && i == Platforms - 1)
                {
                    Stair.Add(T.DestinationCoordinate(Platform.Last(), 0.004, 90));
                    Stair.Add(T.DestinationCoordinate(Platform.Last(), 0.004, 270));

                }
                if (Platforms != 1 && i == 1)
                {
                    Stair.Add(T.DestinationCoordinate(Stair[1], Scale * (Platforms) / 4, 0));
                    Stair.Add(T.DestinationCoordinate(Stair[0], Scale * (Platforms) / 4, 0));
                }


                Platform = T.GeodesicRotation(Platform, Origin, _Orientation);
                PlatformTop = T.GeodesicRotation(PlatformTop, Origin, _Orientation);


                for (int j = 0; j < Platform.Count; ++j)
                {
                    Platform[j].Altitude = ((i) * 2.5) + Origin.Altitude;
                    PlatformTop[j].Altitude = ((i) * 2.5) + Origin.Altitude + 0.1;
                }


                LevelTheLevel = new Geotool_Objects.Polygon();
                LevelTheLevel.LinearList.AddRange(Platform);
                Base.PolyGonList.Add(LevelTheLevel);
                LevelTheLevel = new Geotool_Objects.Polygon();
                LevelTheLevel.LinearList.AddRange(PlatformTop);
                Walkways.PolyGonList.Add(LevelTheLevel);
            }
            Stair = T.GeodesicRotation(Stair, Origin, _Orientation);
            if (Platforms > 1)
            {
                Stair[0].Altitude = Base.PolyGonList[1].LinearList[0].Altitude;
                Stair[1].Altitude = Base.PolyGonList[1].LinearList[0].Altitude;
            }
            else
            {
                Stair[0].Altitude = Base.PolyGonList[0].LinearList[0].Altitude;
                Stair[1].Altitude = Base.PolyGonList[0].LinearList[0].Altitude;
            }

            Stair[2].Altitude = Origin.Altitude;
            Stair[3].Altitude = Origin.Altitude;
            LevelTheLevel = new Geotool_Objects.Polygon();
            LevelTheLevel.LinearList.AddRange(Stair);
            Stairs.PolyGonList.Add(LevelTheLevel);


            Pyramid.Add(Base);
            Pyramid.Add(Walkways);
            Pyramid.Add(Stairs);






        }




        public override List<SharpKml.Dom.Placemark> ToPlaceMarks()
        {
            GeoTools T = new GeoTools("Clay");
            List<SharpKml.Dom.Placemark> r = new List<Placemark>();
            r.Add(T.SampleStyler(Pyramid[0].ToPlacemark(),
                                        true,
                                         SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor,
                                        new Color32(255, 0, 0, 170),
                                        new Color32(255, 0, 0, 170),
                                        "Base"));
            r.Add(T.SampleStyler(Pyramid[1].ToPlacemark(),
                             true,
                              SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor,
                             new Color32(255, 188, 250, 255),
                             new Color32(255, 188, 250, 255),
                             "Walkway"));
            r.Add(T.SampleStyler(Pyramid[2].ToPlacemark(),
                             true,
                              SharpKml.Dom.GX.AltitudeMode.RelativeToSeafloor,
                             new Color32(255, 188, 250, 255),
                             new Color32(255, 188, 250, 255),
                             "Stairs"));

            return r;
        }

    }





}
