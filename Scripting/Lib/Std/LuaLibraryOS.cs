#region -- copyright --
//
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
//
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Neo.IronLua;

namespace BhModule.Community.Pathing.Scripting.Lib.Std {
    public class LuaLibraryOS {

		// https://github.com/neolithos/neolua/blob/master/NeoLua/LuaLibraries.cs#L1551-L1789

		private static readonly DateTime _unixStartTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);

		public static LuaResult clock() => new LuaResult(Process.GetCurrentProcess().TotalProcessorTime.TotalSeconds);

		/// <summary>Converts a number representing the date and time back to some higher-level representation.</summary>
		/// <param name="format">Format string. Same format as the C <see href="http://www.cplusplus.com/reference/ctime/strftime/">strftime()</see> function.</param>
		/// <param name="time">Numeric date-time. It defaults to the current date and time.</param>
		/// <returns>Formatted date string, or table of time information.</returns>
		/// <remarks>by PapyRef</remarks>
		public static object date(string format, object time) {
			// Unix timestamp is seconds past epoch. Epoch date for time_t is 00:00:00 UTC, January 1, 1970.
			DateTime dt;

			var toUtc = format != null && format.Length > 0 && format[0] == '!';

			if (time == null)
				dt = toUtc ? DateTime.UtcNow : DateTime.Now;
			else if (time is DateTime dt2) {
				dt = dt2;
				switch (dt.Kind) {
					case DateTimeKind.Utc:
						if (!toUtc)
							dt = dt.ToLocalTime();
						break;
					default:
						if (toUtc)
							dt = dt.ToUniversalTime();
						break;
				}
			} else {
				dt = _unixStartTime.AddSeconds((long)Lua.RtConvertValue(time, typeof(long)));
                if (toUtc)
					dt = dt.ToUniversalTime();
			}

			// Date and time expressed as coordinated universal time (UTC).
			if (toUtc)
				format = format.Substring(1);

			if (String.Compare(format, "*t", false) == 0) {
				var lt = new LuaTable {
					["year"] = dt.Year,
					["month"] = dt.Month,
					["day"] = dt.Day,
					["hour"] = dt.Hour,
					["min"] = dt.Minute,
					["sec"] = dt.Second,
					["wday"] = (int)dt.DayOfWeek,
					["yday"] = dt.DayOfYear,
					["isdst"] = (dt.Kind == DateTimeKind.Local ? true : false)
				};
				return lt;
			} else
				return ToStrFTime(dt, format);
		} // func date

		/// <summary>Calculate the current date and time, coded as a number. That number is the number of seconds since 
		/// Epoch date, that is 00:00:00 UTC, January 1, 1970. When called with a table, it returns the number representing 
		/// the date and time described by the table.</summary>
		/// <param name="table">Table representing the date and time</param>
		/// <returns>The time in system seconds. </returns>
		/// <remarks>by PapyRef</remarks>
		public static LuaResult time(LuaTable table) {
			TimeSpan ts;

			if (table == null) {
				// Returns the current time when called without arguments
				ts = DateTime.Now.Subtract(_unixStartTime);
			} else {
				try {
					ts = datetime(table).Subtract(_unixStartTime);
				} catch (Exception e) {
					return new LuaResult(null, e.Message);
				}
			}

			return new LuaResult(Convert.ToInt64(ts.TotalSeconds));
		} // func time

		/// <summary>Converts a time to a .net DateTime</summary>
		/// <param name="time"></param>
		/// <returns></returns>
		public static DateTime datetime(object time) {
			switch (time) {
				case LuaTable table:
					return new DateTime(
						table.ContainsKey("year") ? (int)table["year"] < 1970 ? 1970 : (int)table["year"] : 1970,
						table.ContainsKey("month") ? (int)table["month"] : 1,
						table.ContainsKey("day") ? (int)table["day"] : 1,
						table.ContainsKey("hour") ? (int)table["hour"] : 0,
						table.ContainsKey("min") ? (int)table["min"] : 0,
						table.ContainsKey("sec") ? (int)table["sec"] : 0,
						table.ContainsKey("isdst") ? (table.ContainsKey("isdst") == true) ? DateTimeKind.Local : DateTimeKind.Utc : DateTimeKind.Local
					);
				case int i32:
					return _unixStartTime.AddSeconds(i32);
				case long i64:
					return _unixStartTime.AddSeconds(i64);
				case double d:
					return _unixStartTime.AddSeconds(d);
				default:
					throw new ArgumentException();
			}
		} // func datetime

		/// <summary>Calculate the number of seconds between time t1 to time t2.</summary>
		/// <param name="t2">Higher bound of the time interval whose length is calculated.</param>
		/// <param name="t1">Lower bound of the time interval whose length is calculated. If this describes a time point later than end, the result is negative.</param>
		/// <returns>The number of seconds from time t1 to time t2. In other words, the result is t2 - t1.</returns>
		/// <remarks>by PapyRef</remarks>
		public static long difftime(object t2, object t1) {
			var time2 = Convert.ToInt64(t2 is LuaTable ? time((LuaTable)t2)[0] : t2);
			var time1 = Convert.ToInt64(t1 is LuaTable ? time((LuaTable)t1)[0] : t1);

			return time2 - time1;
		} // func difftime

        #region ToStrFTime

        private delegate string DateTimeDelegate(DateTime dateTime);

        private static readonly Dictionary<string, DateTimeDelegate> Formats = new Dictionary<string, DateTimeDelegate> {
            // abbreviated weekday name (e.g., Wed)
            {
                "a", (dateTime) => dateTime.ToString("ddd", CultureInfo.CurrentCulture)
            },

            // full weekday name (e.g., Wednesday)
            {
                "A", (dateTime) => dateTime.ToString("dddd", CultureInfo.CurrentCulture)
            },

            // abbreviated month name (e.g., Sep)
            {
                "b", (dateTime) => dateTime.ToString("MMM", CultureInfo.CurrentCulture)
            },

            // full month name (e.g., September)
            {
                "B", (dateTime) => dateTime.ToString("MMMM", CultureInfo.CurrentCulture)
            },

            // date and time (e.g., 09/16/98 23:48:10)
            {
                "c", (dateTime) => dateTime.ToString("ddd MMM dd HH:mm:ss yyyy", CultureInfo.CurrentCulture)
            },

            // day of the month (16) (01-31)
            {
                "d", (dateTime) => dateTime.ToString("dd", CultureInfo.CurrentCulture)
            },

            // day of the month, space-padded ( 1-31)
            {
                "e", (dateTime) => dateTime.ToString("%d", CultureInfo.CurrentCulture).PadLeft(2, ' ')
            },

            // hour, using a 24-hour clock (00-23)
            {
                "H", (dateTime) => dateTime.ToString("HH", CultureInfo.CurrentCulture)
            },

            // hour, using a 12-hour clock (01-12)
            {
                "I", (dateTime) => dateTime.ToString("hh", CultureInfo.CurrentCulture)
            },

            // day of the year (001-366)
            {
                "j", (dateTime) => dateTime.DayOfYear.ToString().PadLeft(3, '0')
            },

            //month (01-12)
            {
                "m", (dateTime) => dateTime.ToString("MM", CultureInfo.CurrentCulture)
            },

            // minute (00-59)
            {
                "M", (dateTime) => dateTime.Minute.ToString().PadLeft(2, '0')
            },

            // either "AM" or "PM"
            {
                "p", (dateTime) => dateTime.ToString("tt", new CultureInfo("en-US"))
            },

            // second (00-59)
            {
                "S", (dateTime) => dateTime.ToString("ss", CultureInfo.CurrentCulture)
            },

            // week number with the first Sunday as the first day of week one (00-53)   
            {
                "U", (dateTime) => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule, DayOfWeek.Sunday).ToString().PadLeft(2, '0')
            },

            // week number with the first Monday as the first day of week one (00-53)
            {
                "W", (dateTime) => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule, DayOfWeek.Monday).ToString().PadLeft(2, '0')
            },

            // weekday as a decimal number with Sunday as 0 (0-6)
            {
                "w", (dateTime) => ((int)dateTime.DayOfWeek).ToString()
            },

            // date (e.g., 09/16/98)
            {
                "x", (dateTime) => dateTime.ToString("d", CultureInfo.CurrentCulture)
            },

            // time (e.g., 23:48:10)
            {
                "X", (dateTime) => dateTime.ToString("T", CultureInfo.CurrentCulture)
            },

            // two-digit year [00-99]
            {
                "y", (dateTime) => dateTime.ToString("yy", CultureInfo.CurrentCulture)
            },

            // full year (e.g., 2014)
            {
                "Y", (dateTime) => dateTime.ToString("yyyy", CultureInfo.CurrentCulture)
            },

            // Timezone name or abbreviation, If timezone cannot be termined, no characters
            {
                "Z", (dateTime) => dateTime.ToString("zzz", CultureInfo.CurrentCulture)
            },

            // the character `%´
            {
                "%", (dateTime) => "%"
            }
        };

		/// <summary>
		/// Format time as string
		/// </summary>
		/// <param name="dateTime">Instant in time, typically expressed as a date and time of day.</param>
		/// <param name="pattern">String containing any combination of regular characters and special format specifiers.They all begin with a percentage (%).</param>
		/// <returns>String with expanding its format specifiers into the corresponding values that represent the time described in dateTime</returns>
		private static string ToStrFTime(DateTime dateTime, string pattern) {
            string output = "";
            int    n      = 0;

            if (string.IsNullOrEmpty(pattern)) { return dateTime.ToString(); }

            while (n < pattern.Length) {
                string s = pattern.Substring(n, 1);

                if (n + 1 >= pattern.Length)
                    output += s;
                else
                    output += s == "%"
                                  ? Formats.ContainsKey(pattern.Substring(++n, 1)) ? Formats[pattern.Substring(n, 1)].Invoke(dateTime) : "%" + pattern.Substring(n, 1)
                                  : s;
                n++;
            }

            return output;
        }

        #endregion

    }
}
