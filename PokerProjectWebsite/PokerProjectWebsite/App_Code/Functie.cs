using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
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
            db.Close();
            return true;
        }
        db.Close();
        return false;
    }


    /// <summary>
    /// Zorgt voor het valideren van de refcode voor de preset van een event. 
    /// </summary>
    /// <param name="refcode"></param>
    /// <returns> true als output niet null is (en dus de record bestaat), false als dit niet het geval is </returns>
    public static bool CheckRefcode(string refcode)
    {
        Database db = Database.OpenConnectionString(Functie.connectionString, Functie.provider);
        string QR_GetRefcode = "SELECT ReferencieCode FROM Event WHERE ReferencieCode = @0;";
        dynamic Output = db.QuerySingle(QR_GetRefcode, refcode);
        if (Output != null)
        {
            db.Close();
            return true;
        }
        db.Close();
        return false;
    }

    /// <summary>
    /// Zorgt voor het valideren van de refcode voor de preset van een event. 
    /// </summary>
    /// <param name="voornaam"></param>
    /// <param name="achternaam"></param>
    /// <param name="refcode"></param>
    /// <returns> true als output niet null is (en dus de record bestaat), false als dit niet het geval is </returns>
    public static void LiveEventAddSpeler(string voornaam, string achternaam, string tafelnummer, string refcode)
    {
        Database db = Database.OpenConnectionString(Functie.connectionString, Functie.provider);
        string QR_GetSpelersNaam = "SELECT SpelerId,Voornaam, Achternaam FROM Speler WHERE Voornaam = @0 AND Achternaam = @1;";
        dynamic Output = db.QuerySingle(QR_GetSpelersNaam, voornaam, achternaam);
        int SpelerID;
        if (Output == null)
        {
            string QR_VoegSpelerToe = "INSERT INTO Speler(Voornaam, Achternaam) VALUES(@0, @1)";
            db.Execute(QR_VoegSpelerToe, voornaam, achternaam);
            SpelerID = GetSpelerID(voornaam, achternaam);
        }
        else
        {
            SpelerID = Output[0];
        }

        string QR_InsertEvent = "INSERT INTO SpelerEvent (SpelerId, ReferencieCode, TafelNummer) VALUES (@0, @1, @2)";
        db.Execute(QR_InsertEvent, SpelerID, refcode, tafelnummer);
        db.Close();
    }

    /// <summary>
    /// Zorgt ervoor dat de geselecteerde speler uit het liveEvent wordt verwijderd 
    /// </summary>
    public static void LiveEventDeleteSpeler(string voornaam, string achternaam)
    {
        Database db = Database.OpenConnectionString(Functie.connectionString, Functie.provider);
        string QR_DeleteSpelersNaam = "DELETE FROM SpelerEvent WHERE SpelerId = @0";
        db.Execute(QR_DeleteSpelersNaam, GetSpelerID(voornaam, achternaam));
        db.Close();
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
        db.Close();
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
        db.Close();
        return result2;
    }

    /// <summary>
    /// Kijkt hoeveel tafels er zijn.
    /// </summary>
    /// <param name="ReferencieCodeEvent"></param>
    /// <returns></returns>
    public static int HoeveelheidTafels(string refcode)
    {
        return SpelersLive(refcode, 0).ElementAt(0).TafelNummer;
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
        db.Close();
        return result[0];
    }

    /// <summary>
    /// Haalt de spelernamen op op basis van een lijst aan IDs
    /// </summary>
    /// <param name="SpelerIDs"></param>
    /// <returns></returns>
    public static Dictionary<string, int> GetSpelerNaam(Dictionary<int,int> SpelerIDs)
    {
        Database db = Database.OpenConnectionString(connectionString, provider);
        string QR_GetID = "SELECT Voornaam, Achternaam FROM Speler WHERE SpelerId =@0";

        Dictionary<string, int> results = new Dictionary<string, int>();

        foreach(var key in SpelerIDs)
        {
            var query = db.QuerySingle(QR_GetID, key.Key);
            string spelernaam = (string)query[0] + " " + (string)query[1];
            results.Add(spelernaam, key.Value);
        }
        db.Close();
        return results;
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
        db.Close();
        return SpelerIDs;
    }

    /// <summary>>
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
        db.Close();
        return FicheIDs;
    }

    /// <summary>
    /// Haalt de BlindsData op en stopt het in een array
    /// Array krijgt de lengte van de hoeveelheid Rows in de dataTable
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static string[,] GetBlinds(string json,string presetnaam)
    {
        Database db = Database.OpenConnectionString(connectionString, provider);
        if (presetnaam == "")
        {
            DataTable dataTable = ConvertJSON(json, "Blinds");
            int numberOfRows = dataTable.Rows.Count;
            string[,] arrayOfBlindTable = new string[numberOfRows, 5];

            for (int i = 0; i < numberOfRows; i++)
            {
                arrayOfBlindTable[i, 0] = dataTable.Rows[i]["Ronde"].ToString();
                arrayOfBlindTable[i, 1] = dataTable.Rows[i]["Pauze"].ToString();
                arrayOfBlindTable[i, 2] = dataTable.Rows[i]["SmallBlind"].ToString();
                arrayOfBlindTable[i, 3] = dataTable.Rows[i]["BigBlind"].ToString();
                arrayOfBlindTable[i, 4] = dataTable.Rows[i]["Duratie"].ToString();
            }
            db.Close();
            return arrayOfBlindTable;
        }
        else
        {
            string QR_GetPreset = "SELECT * FROM Blinds WHERE Presetnaam = @0";
            string QR_GetNumRows = "SELECT COUNT(Ronde) FROM Blinds WHERE Presetnaam = @0";
            dynamic result = db.Query(QR_GetPreset, presetnaam);
            var numberOfRows = db.QuerySingle(QR_GetNumRows,presetnaam);
            string[,] arrayOfBlindTable = new string[numberOfRows[0],5];
            int i = 0; 
            foreach(var row in result)
            {
                arrayOfBlindTable[i, 0] = Convert.ToString(row[1]); 
                arrayOfBlindTable[i, 1] = Convert.ToString(row[2]); 
                arrayOfBlindTable[i, 2] = Convert.ToString(row[3]); 
                arrayOfBlindTable[i, 3] = Convert.ToString(row[4]);
                arrayOfBlindTable[i, 4] = Convert.ToString(row[5]);
                i++;
            }
            return arrayOfBlindTable; 
        }

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
    /// Haalt alle aangemaakte refcodes op en zet de in een lijst voor een dropdown
    /// </summary>
    /// <returns></returns>
    public static List<string> GetAangemaakteRefcodes()
    {
        Database db = Database.OpenConnectionString(connectionString, provider);
        string QR_GetRefcodes = "Select ReferencieCode FROM Event";
        dynamic queryresult = db.Query(QR_GetRefcodes);
        List<string> result = new List<string>();
        foreach(var row in queryresult)
        {
            result.Add(row[0]);
        }
        db.Close();
        return result; 

    }

    /// <summary>
    /// Haalt alle aangemaakte Presetnamen van de blinds op en zet ze in een lijst voor een dorpdown
    /// </summary>
    /// <returns></returns>
    public static List<string> GetAangemaakteBlindPresets()
    {
        Database db = Database.OpenConnectionString(connectionString, provider);
        string QR_GetRefcodes = "Select Presetnaam FROM Blinds GROUP BY Presetnaam ";
        dynamic queryresult = db.Query(QR_GetRefcodes);
        List<string> result = new List<string>();

        foreach(var row in queryresult)
        {
            result.Add(row[0]);
        }
        db.Close();
        return result;

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
        db.Close();
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
        for (var i = l.Count; i > 1; i--) {
            var pos = rnd.Next(i);
            var x = l[i - 1];
            l[i - 1] = l[pos];
            l[pos] = x;
        }

        return list;
    }

    /// <summary>
    /// Verdeeld spelers in evenredige lijsten 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="chunkSize"></param>
    /// <returns>lijst met verdeelde lijsten</returns>
    public static List<List<T>> ChunkEvenly<T>(List<T> source, int chunkSize)
    {
        var l = new List<List<T>>();

        for (int i = 0; i < chunkSize; i++)
        {
            var c = new List<T>();
            l.Add(c);
        }

        var j = 0;
        foreach (var speler in source)
        {
            l[j].Add(speler);
            if (j < (chunkSize - 1)) j++;
            else j = 0;
        }

        return l;
    }

    /// <summary>
    /// Gets number of players at one table for next method
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    private static int GetMaxAanTafel(string json)
    {
        DataTable dataTable = ConvertJSON(json, "Instellingen");
        return Convert.ToInt32(dataTable.Rows[0][1]); 
    }

    /// <summary>
    /// Genereerd een tafelindeling
    /// </summary>
    /// <param name="SpelerIds"></param>
    /// <param name="maxAanTafel"></param>
    /// <returns></returns>
    public static Dictionary<int, int> SpelersMetTafelNummer(string json)
    {
        float maxAanTafel = GetMaxAanTafel(json);
        List<int> SpelerIds = GetSpelerIDList(json);
        // Uiteindelijke lijst met speler verbonden aan tafel
        Dictionary<int, int> result = new Dictionary<int, int>();
        // berekening voor tafels
        
        float aantalTafel = SpelerIds.Count / maxAanTafel;
        int aantalTafels = (int)Math.Ceiling(aantalTafel);

        var dividedLists = ChunkEvenly(ShuffleIntList(SpelerIds), aantalTafels);

        for (var i = 0; i < dividedLists.Count(); i++)
        {
            foreach (var speler in dividedLists[i])
            {
                result[speler] = i + 1;
            }
        }
        return result;
    }

    /// <summary>
    /// Haalt het grootste tafelnummer op om de preview te maken 
    /// </summary>
    /// <param name="spelers"></param>
    /// <returns></returns>
    public static int GetTableNumberPreview(Dictionary<int,int> spelers)
    {
        int result = 0;
        List<int> tafelnummers = new List<int>(); 
        foreach(KeyValuePair<int,int> pair in spelers)
        {
            tafelnummers.Add(pair.Value);
        }

        if(tafelnummers.Count == 0)
        {
            result = 0;
        }
        else
        {
            result = tafelnummers.Max();
        }
        return result; 
    }
    /// <summary>
    /// Voegd een tafelnummer toe aan spelerlijst
    /// </summary>
    /// <param name="Spelers"></param>
    /// <param name="Fiches"></param>
    /// <returns></returns>
    public static void EventAanmaken(Dictionary<int,int> Spelers,List<int> Fiches, string[,] Blinds,string refcode)
    {
        Database db = Database.OpenConnectionString(connectionString, provider);
        /// Event aanmaken
        string QR_EventAanmaken = "INSERT INTO Event(ReferencieCode,EventLoopt) VALUES (@0,@1)";
        db.Execute(QR_EventAanmaken, refcode, false);

        /// Voeg de spelers toe op basis van de IDLijst
        string QR_SpelersInvoeren = "INSERT INTO SpelerEvent(SpelerId,TafelNummer,ReferencieCode) VALUES(@0,@1,@2)";
        foreach (KeyValuePair<int,int> pair in Spelers)
        {

            db.Execute(QR_SpelersInvoeren,pair.Key,pair.Value,refcode); 
        }
        
        /// Voeg de fiches toe op basis van de IDLijst
        string QR_FichesInvoeren = "INSERT INTO EventFiches(FicheId,ReferencieCode) VALUES(@0,@1)";
        foreach(int i in Fiches)
        {
            db.Execute(QR_FichesInvoeren, i, refcode); 
        }

        /// Hier moet het blinds-schema ingevoerd worden. 
        string QR_BlindsInvoeren = "INSERT INTO Blinds(Ronde, Pauze, SmallBlind, BigBlind, Duratie, ReferencieCode) VALUES(@0,@1,@2,@3,@4,@5)";
        for (int i = 0; i < Blinds.GetLength(0); i++)
        {
            int ronde = Convert.ToInt32(Blinds[i, 0]);
            string pauze = Convert.ToString(Blinds[i, 1]);
            int BigBlind = Convert.ToInt32(Blinds[i, 2]);
            int SmallBlind = Convert.ToInt32(Blinds[i, 3]);
            int Duratie = Convert.ToInt32(Blinds[i, 4]);
            db.Execute(QR_BlindsInvoeren, ronde, pauze, BigBlind, SmallBlind, Duratie ,refcode); 
        }
        db.Close();
    }

    /// <summary>
    /// Maakt tijden aan wanneer er op de start knop gedrukt word 
    /// </summary>
    /// <param name="refcode"></param>
    public static void BeginBlindTimer(string refcode)
    {
        Database db = Database.OpenConnectionString(connectionString, provider);

        var startTime = DateTime.Now;

        string QR_GetDuratie = "SELECT Duratie, Begintijd FROM Blinds WHERE ReferencieCode = @0 ";
        string QR_InputTime = "UPDATE Blinds SET Begintijd = @0, Eindtijd = @1 WHERE Ronde = @2 AND ReferencieCode = @3";
        string QR_UpdateEvent = "UPDATE Event SET EventLoopt = @0 WHERE ReferencieCode = @1";
        int ronde = 1;
        dynamic Duraties = db.Query(QR_GetDuratie, refcode);
        db.Execute(QR_UpdateEvent, true, refcode);
        foreach (var row in Duraties)
        {
            DateTime eindTijd = startTime.AddMinutes(row[0]);
            db.Execute(QR_InputTime, startTime, eindTijd, ronde, refcode);
            startTime = eindTijd;
            ronde++;
        }
    }

    /// <summary>
    /// Verwijderd het hele event
    /// </summary>
    /// <param name="refcode"></param>
    public static void DeleteEvent(string refcode)
    {
        Database db = Database.OpenConnectionString(connectionString, provider);

        string QR_DeleteBlinds = "DELETE FROM Blinds WHERE ReferencieCode = @0";
        string QR_DeleteSpeler = "DELETE FROM SpelerEvent WHERE ReferencieCode = @0";
        string QR_DeleteFiches = "DELETE FROM EventFiches WHERE ReferencieCode = @0";
        string QR_DeleteEvent  = "DELETE FROM Event WHERE ReferencieCode = @0";
        db.Execute(QR_DeleteBlinds, refcode);
        db.Execute(QR_DeleteSpeler, refcode);
        db.Execute(QR_DeleteFiches, refcode);
        db.Execute(QR_DeleteEvent, refcode);
    }

    /// <summary>
    /// Kijkt of de blind timer gebruikt word of niet
    /// </summary>
    /// <param name="refcode"></param>
    /// <returns></returns>
    public static bool EventStatus(string refcode)
    {
        Database db = Database.OpenConnectionString(connectionString, provider);
        string QR_GetStatus = "SELECT EventLoopt FROM Event WHERE ReferencieCode = @0";
        var result = db.QuerySingle(QR_GetStatus, refcode);
        db.Close();
        if(result != null)
        {
            return result[0]; 
        }
        return false;
        
    }

    /// <summary>
    /// Maakt een nieuw schema in de db met een presetnaam voor later gebruik
    /// </summary>
    /// <param name="blindschema"></param>
    /// <param name="presetnaam"></param>
    public static void PresetBlindsMaken(string[,] blindschema, string presetnaam)
    {
        Database db = Database.OpenConnectionString(connectionString, provider);
        string QR_MaakPreset = "INSERT INTO Blinds(Ronde,Pauze,SmallBlind,BigBlind,Duratie,Presetnaam) VALUES(@0,@1,@2,@3,@4,@5)";
        for (int i = 0; i < blindschema.GetLength(0); i++)
        {
            int ronde = Convert.ToInt32(blindschema[i, 0]);
            int pauze = Convert.ToInt32(blindschema[i, 1]);
            int SmallBlind  = Convert.ToInt32(blindschema[i, 2]);
            int BigBlind = Convert.ToInt32(blindschema[i, 3]);
            int Duratie = Convert.ToInt32(blindschema[i, 4]);
            db.Execute(QR_MaakPreset, ronde, pauze, SmallBlind, BigBlind, Duratie, presetnaam);
        }
        db.Close();
    }

    /// <summary>
    ///  Haalt de blinddata op voor de timer op createEvent 
    /// </summary>
    /// <param name="refcode"></param>
    /// <returns></returns>
    public static List<double> GetBlindData(string refcode)
    {
        DateTime nu =  DateTime.Now;

        Database db = Database.OpenConnectionString(connectionString, provider);
        string QR_Getblinddata = "SELECT * FROM Blinds WHERE (BeginTijd <= @0 AND EindTijd > @0) AND ReferencieCode = @1";
        List<double> result = new List<double>();
        var queryresult = db.Query(QR_Getblinddata, nu, refcode);

        var rondeplaceholder = 0;

        foreach (var row in queryresult)
        {
           rondeplaceholder = row.ronde + 1;
           result.Add((double)row.Ronde);
           result.Add((double)row.SmallBlind);
           result.Add((double)row.BigBlind);
           long t = row.eindtijd.Ticks;
           DateTime dtEnd = new DateTime(t);
           TimeSpan tmElapsed = dtEnd - nu;
           result.Add(tmElapsed.TotalMilliseconds * 10000);
           result.Add((double)row.Pauze);
        }

        string QR_Getnextrow = "SELECT * FROM Blinds WHERE Ronde = @0 AND ReferencieCode = @1";
        var querynextrow = db.Query(QR_Getnextrow,rondeplaceholder,refcode);

        foreach(var row in querynextrow)
        {
            result.Add((double)row.Ronde);
            result.Add((double)row.SmallBlind);
            result.Add((double)row.BigBlind);
            result.Add((double)row.Pauze);
            result.Add((double)row.Duratie);
        }
        
        if(result.Count() == 0)
        {
            result.Add(2000);
            result.Add(0);
            result.Add(0);
            result.Add(0);
            result.Add(0);
            result.Add(0);
            result.Add(0);
            result.Add(0);
            result.Add(0);
            result.Add(0);
        }
        db.Close();
        return result; 

    }
    
}