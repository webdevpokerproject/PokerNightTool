using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Timers;
using System.Web;
using WebMatrix.Data;

/// <summary>
/// Binnen deze class alle functionaliteit wegschrijven zodat de uiteindelijke pages overzichtelijk blijven.
/// Gelieve alle methodes voorzien van XML-comments en eventueel aanvullen in de methode als deze complex is. 
/// 
/// Om een methode aan te roepen (binnen of buiten deze class) werkt als volgt: 
/// ** Functie.AangemaakteMethode(* eventuele parameters*) ** 
/// 
/// Aangemaakte methodes dienen met een Hoofdletter te beginnen en in CamelCase stijl gemaakt worden.
/// Parameters dienen een logische naam te hebben. Zie Login methode. 
/// Voorbeeld: 
/// ** AangemaakteMethode() ** 
/// 
/// Methodes die enkel queries uitvoeren dienen met QR_ te beginnen.
/// Voorbeeld: 
/// ** QR_HaalUniekeCodeOp() ** 
/// 
/// </summary>

public class Functie
{
    /// <summary>
    /// Deze 2 strings vertegenwoordigen de connectie string en database provider. 
    /// Deze hiermee kan de database connectie op onderstaande manier worden aangeroepen:
    /// ** Database db = Database.OpenConnectionString(Functions.connectionString, Functions.provider); ** 
    /// </summary>
    public const string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\data.mdf;Integrated Security=True";
    public const string provider = "System.Data.SqlClient";

    /// <summary>
    /// Zorgt voor inlogfunctionaliteit voor de beheerder. 
    /// </summary>
    /// <param name="naam"></param>
    /// <param name="wachtwoord"></param>
    /// <returns> true als output niet null is (en dus de record bestaat), false als dit niet het geval is </returns>
    public static bool Login(string naam, string wachtwoord)
    {
        Database db = Database.OpenConnectionString(Functie.connectionString, Functie.provider);
        string QR_GetLogin = "SELECT Gebruikersnaam, Wachtwoord FROM Beheerder WHERE Gebruikersnaam = @0 AND Wachtwoord = @1";
        dynamic Output = db.QuerySingle(QR_GetLogin, naam, wachtwoord); 
        if(Output != null)
        {
            return true;
        }
        return false; 
    }
    
    /// <summary>
    /// Haalt fiches op op basis van de eventnaam
    /// </summary>
    /// <param name="ReferencieCodeEvent"> ReferencieCode van het event</param>
    /// <returns> De kleur en waarde van de gebruikte fiches, worden op pagina omgezet tot <img> </returns>
    public static IEnumerable<dynamic> Fiches(string ReferencieCodeEvent)
    {
        Database db = Database.OpenConnectionString(Functie.connectionString, Functie.provider);
        string QR_GetFiches = "SELECT Fiche.Kleur, Fiche.Waarde FROM (Fiche INNER JOIN EventFiches ON EventFiches.FicheId = Fiche.FicheId) WHERE EventFiches.ReferencieCode = @0 ORDER BY Fiche.Waarde ASC";
        var result = db.Query(QR_GetFiches, ReferencieCodeEvent);
        return result;
    }

    /// <summary>
    /// Haalt spelers op van het toebehorende event
    /// </summary>
    /// <param name="ReferencieCodeEvent"> ReferencieCode van het event </param>
    /// <returns> De spelers worden opgehaald </returns>
    public static IEnumerable<dynamic> Spelers(string ReferencieCodeEvent, int TafelNummer)
    {
        Database db = Database.OpenConnectionString(Functie.connectionString, Functie.provider);
        string QR_GetSpelers;

        if (TafelNummer == 0)
        {
            QR_GetSpelers = "SELECT Speler.Voornaam, Speler.Achternaam, SpelerEvent.TafelNummer FROM (Speler INNER JOIN SpelerEvent ON Speler.SpelerId = SpelerEvent.SpelerId) WHERE SpelerEvent.ReferencieCode = @0 ORDER BY TafelNummer DESC";
            var result = db.Query(QR_GetSpelers, ReferencieCodeEvent);
            return result;
        }

        QR_GetSpelers = "SELECT Speler.Voornaam, Speler.Achternaam, SpelerEvent.TafelNummer FROM (Speler INNER JOIN SpelerEvent ON Speler.SpelerId = SpelerEvent.SpelerId) WHERE SpelerEvent.ReferencieCode = @0 AND TafelNummer = @1 ORDER BY TafelNummer DESC";
        var result2 = db.Query(QR_GetSpelers, ReferencieCodeEvent, TafelNummer);
        return result2;
    }



    /// <summary>
    /// Kijkt hoeveel tafels er zijn.
    /// </summary>
    /// <param name="ReferencieCodeEvent"></param>
    /// <returns></returns>
    public static int HoeveelheidTafels(string ReferencieCodeEvent)
    {
        return Spelers(ReferencieCodeEvent, 0).ElementAt(0).TafelNummer;
    }

    /// <summary>
    /// Maakt een unieke code aan voor het event.
    /// Lengte varieert per code. 
    /// Geld als Forgein Key in de Event tabel. 
    /// </summary>
    /// <param name="size"></param>
    /// <returns>Een unieke code</return
    public static string MaakReferencieCode()
    {
        Random rnd = new Random();
        int size = rnd.Next(3, 11);
        var chars = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890";
        var data = new Byte[size];
        using (var crypto = new RNGCryptoServiceProvider())
        {
            crypto.GetBytes(data);
        }
        var result = new StringBuilder(size);
        foreach (var b in data)
        {
            result.Append(chars[b % (chars.Length)]);
        }
        return result.ToString();
    }
}