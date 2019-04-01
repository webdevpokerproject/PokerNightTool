using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        Database db = Database.OpenConnectionString(connectionString,provider);
        string QR_GetLogin = "SELECT Gebruikersnaam, Wachtwoord FROM Beheerder WHERE Gebruikersnaam = @0 AND Wachtwoord = @1";
        dynamic Output = db.QuerySingle(QR_GetLogin, naam, wachtwoord);
        if (Output != null)
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
    public static IEnumerable<dynamic> FichesLive(string ReferencieCodeEvent)
    {
        Database db = Database.OpenConnectionString(connectionString, provider);
        string QR_GetFiches = "SELECT Fiche.Kleur, Fiche.Waarde FROM (Fiche INNER JOIN EventFiches ON EventFiches.FicheId = Fiche.FicheId) WHERE EventFiches.ReferencieCode = @0 ORDER BY Fiche.Waarde ASC";
        var result = db.Query(QR_GetFiches, ReferencieCodeEvent);
        return result;
    }

    /// <summary>
    /// Haalt spelers op van het toebehorende event
    /// </summary>
    /// <param name="ReferencieCodeEvent"> ReferencieCode van het event </param>
    /// <returns> De spelers worden opgehaald </returns>
    public static IEnumerable<dynamic> SpelersLive(string ReferencieCodeEvent, int TafelNummer)
    {
        Database db = Database.OpenConnectionString(connectionString, provider);
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
        return SpelersLive(ReferencieCodeEvent, 0).ElementAt(0).TafelNummer;
    }

    /// <summary>
    /// Maakt van de JSON string een DataTable
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static DataTable ConvertJSON(string json, string var)
    {
        DataSet dataSet = JsonConvert.DeserializeObject<DataSet>(json);
        DataTable dataTable = new DataTable();
        if (dataSet != null)
            dataTable = dataSet.Tables[var];
        return dataTable;
    }

    /// <summary>
    /// Voert de query uit op de db om spelerID op te halen op basis van voor en achternaam
    /// </summary>
    /// <param name="voornaam"></param>
    /// <param name="achternaam"></param>
    /// <returns> SpelerID as int </returns>
    public static int GetSpelerID(string voornaam, string achternaam)
    {
        Database db = Database.OpenConnectionString(connectionString, provider);
        string QR_GetID = "SELECT SpelerId FROM Speler WHERE Voornaam = @0 AND Achternaam = @1";
        var result = db.QuerySingle(QR_GetID, voornaam, achternaam);
        return result[0];
    }

    /// <summary>
    /// Haalt de SpelerIDs uit de json string 
    /// </summary>
    /// <param name="json"></param>
    /// <returns>List met spelerIDs </returns>
    public static List<int> GetSpelerIDList(string json)
    {
        List<int> SpelerIDs = new List<int>();
        DataTable dataTable = ConvertJSON(json, "Spelers");
        Database db = Database.OpenConnectionString(connectionString, provider);
        string QR_CheckDB = "SELECT SpelerId, Voornaam, Achternaam FROM Speler WHERE Voornaam = @0 AND Achternaam = @1";
        string QR_VoegSpelerToe = "INSERT INTO Speler(Voornaam, Achternaam) VALUES(@0, @1)";
        
        foreach (DataRow row in dataTable.Rows)
        {
            string voornaam = Convert.ToString(row["Voornaam"]);
            string achternaam = Convert.ToString(row["Achternaam"]); 
            var result = db.QuerySingle(QR_CheckDB, voornaam, achternaam);

            if (result == null)
            { 
                db.Execute(QR_VoegSpelerToe, voornaam, achternaam);
                SpelerIDs.Add(GetSpelerID(voornaam, achternaam));
            }
            else {
                var r = result[0];
                SpelerIDs.Add(r);

            }
            
        }
        return SpelerIDs;
    }

    /// <summary>
    /// WIP
    /// Haalt de FicheIDs uit de json string 
    /// </summary>
    /// <param name="json"></param>
    /// <returns> List met FicheIds</returns>
    public static List<int> GetFicheIDList(string json)
    {
        List<int> FicheIDs = new List<int>();
        DataTable dataTable = ConvertJSON(json, "Fiche");
        Database db = Database.OpenConnectionString(connectionString, provider);

        string QR_GetFicheID = "SELECT FicheId FROM Fiche WHERE Waarde = @0 AND Kleur = @1";

        foreach (DataRow row in dataTable.Rows)
        {
            var kleur = row["Kleur"];
            int waarde = Convert.ToInt32(row["Waarde"]);
            var result = db.QuerySingle(QR_GetFicheID, waarde, kleur) ;
            FicheIDs.Add(result[0]);
        }
        return FicheIDs;
    }

    /// <summary>
    /// Haalt de BlindsData op en stopt het in een array
    /// Array krijgt de lengte van de hoeveelheid Rows in de dataTable
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static string[,] GetBlinds(string json)
    {
        DataTable dataTable = ConvertJSON(json, "Blinds");
        Database db = Database.OpenConnectionString(connectionString, provider);
        int numberOfRows = dataTable.Rows.Count; 
        string[,] arrayOfBlindTable = new string[numberOfRows, 5];

            for (int i = 0; i < numberOfRows; i++)
            {
                arrayOfBlindTable[i, 0] = dataTable.Rows[i]["Ronde"].ToString();
                arrayOfBlindTable[i, 1] = dataTable.Rows[i]["Pauze"].ToString();
                arrayOfBlindTable[i, 2] = dataTable.Rows[i]["BigBlind"].ToString();
                arrayOfBlindTable[i, 3] = dataTable.Rows[i]["SmallBlind"].ToString();
                arrayOfBlindTable[i, 4] = dataTable.Rows[i]["Duratie"].ToString();
            }

        return arrayOfBlindTable; 
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

    /// <summary>
    /// Haalt spelernamen op vanuit de JSON en haalt preview op 
    /// </summary>
    /// <param name="SpelerIDs"></param>
    /// <returns>Lijst met namen van de spelers die eerder zijn ingevoerd</returns>
    public static List<string> SpelersPreview(List<int> SpelerIDs)
    {
        Database db = Database.OpenConnectionString(connectionString, provider);
        string QR_GetSpelerNamen = "SELECT Voornaam, Achternaam FROM Speler WHERE SpelerId = @0";
        List<string> SpelerLijst = new List<string>();

        foreach (int n in SpelerIDs)
        {
            var result = db.QuerySingle(QR_GetSpelerNamen, n);
            string speler = result[0] + " " + result[1];
            SpelerLijst.Add(speler);
        }
        return SpelerLijst;
    }
    /// <summary>
    /// Krijg de naam en waarde van alle fiches
    /// </summary>
    /// <param name="FicheIDs"></param>
    /// <returns>lijst met tuples waar naam en waarde van de fiches in zit</returns>
    public static List<Tuple<string, int>> FichesPreview(List<int> FicheIDs)
    {
        Database db = Database.OpenConnectionString(connectionString, provider);
        string QR_GetFiches = "SELECT Kleur, Waarde FROM Fiche WHERE FicheId = @0";
        var FicheLijst = new List<Tuple<string, int>>();

        foreach(int n in FicheIDs)
        {
            var result = db.QuerySingle(QR_GetFiches, n);
            FicheLijst.Add(Tuple.Create(result[0].ToString(), (int)result[1]));
        }
        return FicheLijst;
    }

    /// <summary>
    /// Methode om een lijst met int's te schudden
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    private static List<int> ShuffleIntList(List<int> list)
    {
        var l = list;
        var rnd = new Random();
        for (var i = l.Count; i > 1; i--)
        {
            var pos = rnd.Next(i);
            var x = l[i - 1];
            l[i - 1] = l[pos];
            l[pos] = x;
        }

        return list;
    }

    /// <summary>
    /// Genereerd een tafelindeling
    /// </summary>
    /// <param name="SpelerIds"></param>
    /// <param name="maxAanTafel"></param>
    /// <returns></returns>
    public static Dictionary<int, int> TafelNummers(List<int> SpelerIds, int maxAanTafel)
    {
        /// Uiteindelijke lijst met speler verbonden aan tafel
        Dictionary<int, int> result = new Dictionary<int, int>();
        /// berekening voor tafels 
        float aantalTafel = (float)SpelerIds.Count / (float)maxAanTafel;
        int aantalTafels = (int)Math.Ceiling(aantalTafel);

        for (int x = 1; x< aantalTafels; x++)
        {
            for(int y = 0; y<maxAanTafel; y++)
            {
                foreach(var s in ShuffleIntList(SpelerIds))
                {
                    result.Add(s, x);
                    SpelerIds.Remove(s);
                }
            }
        }

        return result; 

    }

    /// <summary>
    /// WIP
    /// </summary>
    /// <param name="Spelers"></param>
    /// <param name="Fiches"></param>
    /// <returns></returns>
    public static string EventAanmaken(List<int>Spelers,List<int> Fiches,string[,] Blinds,string refcode)
    {   
        Database db = Database.OpenConnectionString(connectionString, provider);
        /////Maak referencieCode en zet hem in de Event tabel
        //string refcode = MaakReferencieCode();
        //string QR_InsertRefcode = "INSERT INTO Event(ReferencieCode) VALUES(@0)";
        //db.Execute(QR_InsertRefcode, refcode); 

        /// Voeg de spelers toe op basis van de IDLijst
        string QR_SpelersInvoeren = "INSERT INTO SpelerEvent(SpelerId,ReferencieCode) VALUES(@0,@1)";
        foreach (int n in Spelers)
        {
            db.Execute(QR_SpelersInvoeren, n, refcode); 
        }
        
        /// Voeg de fiches toe op basis van de IDLijst
        string QR_FichesInvoeren = "INSERT INTO EventFiches(FicheId,ReferencieCode) VALUES(@0,@1)";
        foreach(int i in Fiches)
        {
            db.Execute(QR_FichesInvoeren, i, refcode); 
        }

        /// Hier moet het blinds-schema ingevoerd worden. 
        string QR_BlindsInvoeren = "INSERT INTO Blinds(Ronde, Pauze, SmallBlind, BigBlind, Duratie) VALUES(@0,@1,@2,@3,@4)";
        for (int i = 0; i < Blinds.GetLength(1); i++)
        {
            string ronde = Blinds[i, 0];
            string pauze = Blinds[i, 0];
            string BigBlind = Blinds[i, 0];
            string SmallBlind = Blinds[i, 0];
            string Duratie = Blinds[i, 0];
            db.Execute(QR_BlindsInvoeren, ronde, pauze, BigBlind, SmallBlind, Duratie); 
        }
        
        /// Return de RefCode zodat de admin deze kan gebruiken.
        return refcode; 

    }

}