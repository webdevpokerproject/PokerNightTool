using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Web;
using WebMatrix.Data;

/// <summary>
/// Summary description for Timer
/// </summary>
public class Timer
{
    public const string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\data.mdf;Integrated Security=True";
    public const string provider = "System.Data.SqlClient";
    private static System.Timers.Timer aTimer;

    public static int GetBlindTime(int ronde)
    {
        Database db = Database.OpenConnectionString(connectionString, provider);
        string QR_GetTime = "SELECT Duratie FROM Blinds WHERE Ronde = @0";
        var result = db.QuerySingle(QR_GetTime, ronde); 
        return result[0] * 60000; 
    }

    public static int ronde = 1; 

    private static void SetTimer()
    {
        // Create a timer with a two second interval.
        aTimer = new System.Timers.Timer(GetBlindTime(ronde));
        // Hook up the Elapsed event for the timer. 
        aTimer.Elapsed += OnTimedEvent;
        aTimer.Enabled = true;
    }

    private static void OnTimedEvent(Object source, ElapsedEventArgs e)
    {
        ronde++;
        SetTimer(); 
    }
}