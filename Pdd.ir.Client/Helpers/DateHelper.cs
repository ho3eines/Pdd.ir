using System.Globalization;

namespace Pdd.ir.Client.Helpers;

public static class DateHelper
{
    private static readonly PersianCalendar pc = new();

    // ═══════════════════════ Unix → DateTime ═══════════════════════

    public static DateTime ToMiladi(long unixTimestamp)
        => DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).UtcDateTime;

    public static long ToUnix(DateTime dateTime)
        => new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(dateTime)).ToUnixTimeSeconds();

    // ═══════════════════════ Unix → String ═══════════════════════

    public static string ToShamsi(long unixTimestamp)
    {
        var dt = ToMiladi(unixTimestamp).ToLocalTime();
        return $"{pc.GetYear(dt):0000}/{pc.GetMonth(dt):00}/{pc.GetDayOfMonth(dt):00}";
    }

    public static string ToShamsiFull(long unixTimestamp)
    {
        string[] months = { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند" };
        var dt = ToMiladi(unixTimestamp).ToLocalTime();
        return $"{pc.GetDayOfMonth(dt)} {months[pc.GetMonth(dt) - 1]} {pc.GetYear(dt)}";
    }

    public static string ToShamsiWithTime(long unixTimestamp)
    {
        var dt = ToMiladi(unixTimestamp).ToLocalTime();
        return $"{pc.GetYear(dt):0000}/{pc.GetMonth(dt):00}/{pc.GetDayOfMonth(dt):00} {dt:HH:mm}";
    }

    public static string ToMiladiString(long unixTimestamp)
        => ToMiladi(unixTimestamp).ToLocalTime().ToString("yyyy-MM-dd");

    public static string ToMiladiStringFull(long unixTimestamp)
        => ToMiladi(unixTimestamp).ToLocalTime().ToString("yyyy-MM-dd HH:mm");

    // ═══════════════════════ DateTime → String ═══════════════════════

    public static string ToShamsi(DateTime dateTime)
    {
        var dt = dateTime.Kind == DateTimeKind.Unspecified ? TimeZoneInfo.ConvertTimeToUtc(dateTime) : dateTime;
        var local = dt.ToLocalTime();
        return $"{pc.GetYear(local):0000}/{pc.GetMonth(local):00}/{pc.GetDayOfMonth(local):00}";
    }

    public static string ToShamsiFull(DateTime dateTime)
    {
        string[] months = { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند" };
        var dt = dateTime.Kind == DateTimeKind.Unspecified ? TimeZoneInfo.ConvertTimeToUtc(dateTime) : dateTime;
        var local = dt.ToLocalTime();
        return $"{pc.GetDayOfMonth(local)} {months[pc.GetMonth(local) - 1]} {pc.GetYear(local)}";
    }

    public static string ToMiladiString(DateTime dateTime)
        => dateTime.ToString("yyyy-MM-dd");

    // ═══════════════════════ String → Unix ═══════════════════════

    /// <summary>
    /// تاریخ شمسی رشته‌ای → Unix timestamp
    /// فرمت ورودی: "1402/01/15" یا "1402/01/15 14:30"
    /// </summary>
    public static long FromShamsi(string shamsiDate)
    {
        try
        {
            var parts = shamsiDate.Split(' ');
            var dateParts = parts[0].Split('/');
            int year = int.Parse(dateParts[0]);
            int month = int.Parse(dateParts[1]);
            int day = int.Parse(dateParts[2]);

            int hour = 0, minute = 0;
            if (parts.Length > 1)
            {
                var timeParts = parts[1].Split(':');
                hour = int.Parse(timeParts[0]);
                minute = int.Parse(timeParts[1]);
            }

            var dt = pc.ToDateTime(year, month, day, hour, minute, 0, 0);
            return ToUnix(dt);
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// تاریخ میلادی رشته‌ای → Unix timestamp
    /// فرمت ورودی: "2023-04-05" یا "2023-04-05 14:30"
    /// </summary>
    public static long FromMiladi(string miladiDate)
    {
        try
        {
            if (DateTime.TryParse(miladiDate, out var dt))
                return ToUnix(dt);
            return 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// رشته شمسی → رشته میلادی
    /// </summary>
    public static string ShamsiToMiladi(string shamsiDate)
    {
        var unix = FromShamsi(shamsiDate);
        return unix > 0 ? ToMiladiString(unix) : "";
    }

    /// <summary>
    /// رشته میلادی → رشته شمسی
    /// </summary>
    public static string MiladiToShamsi(string miladiDate)
    {
        var unix = FromMiladi(miladiDate);
        return unix > 0 ? ToShamsi(unix) : "";
    }

    // ═══════════════════════ String → String ═══════════════════════

    /// <summary>
    /// رشته شمسی → رشته شمسی کامل (با نام ماه)
    /// مثال: "1402/01/15" → "15 فروردین 1402"
    /// </summary>
    public static string ShamsiToShamsiFull(string shamsiDate)
    {
        string[] months = { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند" };
        var parts = shamsiDate.Split('/');
        if (parts.Length >= 3)
        {
            int day = int.Parse(parts[2]);
            int month = int.Parse(parts[1]);
            int year = int.Parse(parts[0]);
            return $"{day} {months[month - 1]} {year}";
        }
        return shamsiDate;
    }

    // ═══════════════════════ Utility ═══════════════════════

    public static long Now() => ToUnix(DateTime.Now);

    public static int CurrentShamsiYear() => pc.GetYear(DateTime.Now);

    public static int CurrentShamsiMonth() => pc.GetMonth(DateTime.Now);

    public static int CurrentShamsiDay() => pc.GetDayOfMonth(DateTime.Now);
}
