using System;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.UI;

namespace Socrata2SqlMigrationTool
{
    public partial class Socrata2Sql : Page
    {
        //  Conection string a la base de datos sql
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Socrata2SqlConnectionString"].ConnectionString;
        // variable del url del que se halara la data
        string url;
        // nombre de la base de datos que se creara o usara en sql
        string databaseName = "SocrataFiles";
        //string que retiene el row sobre el que se está trabajando
        private string rowstring;
        //hala el contenido del archivo y lo coloca en textfromfile
        WebClient wc = new WebClient();

        protected void Page_Load(object sender, EventArgs e)
        {
            //crea el query para crear la base de datos la primera vez
            //string dbquery = GetDbCreationQuery(databaseName);
            //cada vez que inicie la pagina creara o verificara si la base de datos existe
            //ExecuteQuery(dbquery);

        }

        //metodo que repsonde al presionar el boton luego de entrar el url
        protected void GetUrl_Click(object sender, EventArgs e)
        {
            ButtonInsert.Visible = true;
            ButtonGetUrl.Visible = false;
            ButtonCancel.Visible = true;
            //hala el url del textbox
            url = TBurlSocrata.Text;
            //llama el metodo getuniqueidentifier del cual hala el uid del url 
            //coloca ese nombre en el texto del label got uid           
            string tablename = GetUniqueIdentifier(url);
            LabelGotUid.Text = tablename;
            //crea un string que contiene el formato del archivo
            string format = GetFormat(url);
            //coloca el string en el label
            LabelGotFormat.Text = format;

            
            wc.Headers.Add(HttpRequestHeader.AcceptCharset, "UTF-8");
            var textFromFile = wc.DownloadString(url);
            textFromFile = Regex.Replace(textFromFile, @"\r\n?|\n","");
            //cuenta cuantos rows hay usando el corchete de cierre
            int numberofrows = CountStringOnString(textFromFile, "}");
            //cuenta cuantos records tiene el row mas grande...
            // se debe modificar para anadir rows. Considerar subir la data transacional y hacer pivots
            int numberofrecords = FillRowString(textFromFile,numberofrows);
            //tira el contenido del textfromfile a la pagina       

            LabelGotNumRows.Text = numberofrows.ToString();
            LabelGotNumRecords.Text = numberofrecords.ToString();

            string tableQuery = CreateTableQuery(rowstring, numberofrecords, tablename);
            ExecuteQuery(tableQuery);
            //string tableQuery = GenerateColumnList(rowstring, numberofrecords);
            LabelContent.Text = "Table "+tablename+" was succesfully added to database"+ databaseName;

            
        }
        string CreateRowstring(string textFromFile)
        {
            textFromFile = GetRemainingString(textFromFile, "{");
            return GetRow(textFromFile);
        }
        string GetDataForTable(string textFromFile, int numberofrows, string tablename)
        {
            int rowpointer = 0;
            string query = "";
            while (rowpointer < numberofrows)
            {
                query += "INSERT INTO [" + tablename + "] " ;
                string columname = " (";
                string data = " VALUES (";
                string currentrow = GetRow(textFromFile);
                int numberofrecords = CountStringOnString(currentrow, ",");
                int recordpointer = 0;
                string record;
                string currentrecord;
                string currentcolumn;
                while (recordpointer < numberofrecords)
                {
                    currentrecord = GetRecord(currentrow);
                    currentcolumn = GetColumnName(currentrecord).Trim();
                    columname += "["+ currentcolumn + "], ";
                    record = GetDatafromRecord(currentrecord);
                    
                    if (IsDate(currentcolumn))
                    { record = "\'" + record.Replace('T', ' ').Trim() + "\'"; }
                    data += record + ", ";
                    currentrow = GetRemainingString(currentrow, ",");
                    recordpointer++;
                }
                currentrecord = GetRecord(currentrow);
                currentcolumn = GetColumnName(currentrecord).Trim();
                columname += "[" +currentcolumn + "]) ";
                record = GetDatafromRecord(currentrecord);
                if (IsDate(currentcolumn))
                { record = "\'"  +record.Replace('T', ' ').Trim() + "\'"; }
                data += record + "); ";
                //incorporar codigo que verifique presencia de columnas y 
                //anada columnas que faltan con alter table
                query += columname + data;
                textFromFile = GetRemainingString(textFromFile, "}");
                rowpointer++;
            }
            ExecuteQuery(query);
            return "Data has been inserted succefully " ;
        }
        string GetDatafromRecord(string record)
        {
            record = GetRemainingString(record, ":");
            return record.Replace("\"","");
        }
        private int FillRowString(string textFromFile, int numberofrows)
        {
            rowstring = CreateRowstring(textFromFile);
            //cuenta cuantos campos hay en el primer row
            int numberofrecords = CountStringOnString(rowstring, ",");

            int rowpointer = 0;
            while (rowpointer< numberofrows)
            {
                string newrow = CreateRowstring(textFromFile);
                int newNumberOfRecords = CountStringOnString(newrow, ",");
                if (newNumberOfRecords > numberofrecords)
                {
                  rowstring = newrow;
                  numberofrecords = CountStringOnString(rowstring, ",");
                }
                rowpointer++;
            }
            return numberofrecords;
        }
        //metodo que hala el unique identifier del url
        private string GetUniqueIdentifier(string url)
        {
            //detecta la localizacion del index de la instancia de la palabra reosurce
            int filenameIndex = url.IndexOf("resource/");
            //pica el string y solo hala el pedazo que corresponde al UID
            if (url != "")
            {
                string filename = url.Substring(filenameIndex + 9);
                //identifica la localizacion del punto
                int formatIndex = filename.IndexOf(".");
                //pica el string para dar solamente el unique identifier
                filename = filename.Substring(0, formatIndex);
                //devuelve el filename
                return filename;
            }
            else
            {
                Response.Redirect(Request.RawUrl);
                return "";
            }
            
        }

        //hala solo el formato del string
        private string GetFormat(string url)
        {
            //busca la primera instancia de la palabra resource
            int filenameIndex = url.IndexOf("resource/");
            string filename = url.Substring(filenameIndex + 9);
            //pica el filename
            int formatIndex = filename.IndexOf(".");
            //pica el formato
            string format = filename.Substring(formatIndex + 1);
            //devuelve el formato
            return format;
        }

        //metodo que crea el query para crear bases de datos
        static string GetDbCreationQuery(string dbName)
        {
            // db creation query
            string query = "IF  NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'"+dbName+"') BEGIN";
            query += " CREATE DATABASE "+ dbName +" END; USING " + dbName;
            return query;
        }

        //metodo que verifica si la base de datos existe
        public static bool CheckDatabaseExists(string connectionString, string databaseName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand(string.Format(
                       "SELECT db_id('{0}')", databaseName), connection))
                {
                    connection.Open();
                    return (command.ExecuteScalar() != DBNull.Value);
                }
            }
        }

        //funcion que cuenta cantidad de instancias de un string en otro
        private int CountStringOnString(string haystack, string needle)
        {
            int count = 0, n = 0;

            if (needle != "")
            {
                while ((n = haystack.IndexOf(needle, n)) != -1)
                {
                    n += needle.Length;
                    ++count;
                }
            }
            return count;
        }
        //funcion que obtiene el row
        string GetRow(string fulltext)
        {
            if (fulltext.IndexOf("{") >= 0)
            { fulltext = fulltext.Substring(fulltext.IndexOf("{") + 1); }
            if (fulltext.IndexOf("}") >= 0)
            { fulltext = fulltext.Remove(fulltext.IndexOf("}")); }
            return fulltext;
        }

        //funcion que reduce el string eliminando el row analizado
        string GetRemainingString(string fulltext, string cutstring)
        {
            return fulltext.Substring(fulltext.IndexOf(cutstring) + 1);
        }
        string GetRecord(string fulltext)
        {
            if (fulltext.IndexOf(",") > 0)
            { return fulltext.Substring(1, fulltext.IndexOf(",")-1); }
            else
            { return fulltext; }

        }
        string GetColumnName(string record)
        {
            record = record.Trim();
            if (record.IndexOf(":") > 0)
            { record = record.Substring(1, record.IndexOf(":")-3); }
            //record = Regex.Replace(record, "[^A-Za-z0-9 _]", "");
            record = record.Replace(" ", "_");
            //limite maximo del nombre de las columnas
            if (record.Length > 127)
            { record = record.Substring(0, 127); }
            return record;
        }

        string GenerateColumnList(string rowstring, int numberofrecords)
        {
            int rowPointer = 0;
            string rowlist = "";
            string currentrecord; string currentdatatype; string currentcolumn;

            while (rowPointer < numberofrecords)
            {
                currentrecord = GetRecord(rowstring);
                currentcolumn = GetColumnName(currentrecord);
                currentdatatype = GetDataType(GetDatafromRecord(currentrecord).Trim(), currentcolumn);
                rowstring = GetRemainingString(rowstring, ",");
                rowlist = rowlist + " [" + currentcolumn + "] " + currentdatatype + ",";
                rowPointer++;
            }
            currentrecord = GetRecord(rowstring);
            currentcolumn = GetColumnName(currentrecord);
            currentdatatype = GetDataType(GetDatafromRecord(currentrecord).Trim(), currentcolumn);
            return rowlist + " [" + currentcolumn + "] " + currentdatatype;
        }

        string GetDataType(string data, string columname)
        {
            if (IsDate(columname))
            {
                return "datetime";
            }
            else if (IsOnlyNumbers(data))
            {
                return "int";
            }
            //los data type ints y decimal se tienen que evaluar con
            //herramientas del SQL usando un cast() por que hay que mirar todos
            //los records para luego decidr el tipo de dato que corresponde a todos
            // los datos de dicha columna (pueden ser todos enteros excepto uno ya es decimal)
            else
            {
                return "VARCHAR(MAX)";
            }
        }

        public static bool IsOnlyNumbers(string data)
        {
            Regex regex = new Regex("^[0-9]+$");

            if (regex.IsMatch(data))
            {
                return true;
            }
            else { return false; }
            //return strValidateString.All(char.IsDigit);
        }

        public static bool Isdecimal(string str)
        {
            int first = str.IndexOf(".");
            string noperiod = str.Replace(".","");
            if (((first >= 0) && (first - str.LastIndexOf(".")) == 0) &&
                noperiod.All(char.IsDigit))
            {
                return true;
            }
            else { return false; }
        }
        public static bool IsDate(string columname)
        {
            //establecer criterios para idetificar columnas basados en el nombre de la columna
            if ((columname.ToLower() == "fecha")||
                (columname.ToLower() == "date") ||
                (columname.ToLower() == "mes")||
                (columname.ToLower() == "month"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string CreateTableQuery(string rowstring, int numberofrecords, string tablename)
        {
            string tableQuery = "IF(EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES " +
                                 " WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = '" + tablename + "')) BEGIN " + 
                                 " DROP TABLE [" + tablename + "] END;";
            tableQuery += " CREATE TABLE [" + tablename + "](";
            tableQuery += GenerateColumnList(rowstring, numberofrecords);
            return tableQuery + ");";
        }

        //metodo que ejecuta un query en forma de string
        void ExecuteQuery(string query)
        {
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            SqlCommand command = new SqlCommand(query, con);
            command.ExecuteNonQuery();

        }

        protected void InsertData_Click(object sender, EventArgs e)
        {
            url = TBurlSocrata.Text;
            var textFromFile = wc.DownloadString(url);
            textFromFile = Regex.Replace(textFromFile, @"\r\n?|\n", "");
            //cuenta cuantos rows hay usando el corchete de cierre
            int numberofrows = CountStringOnString(textFromFile, "}");
            LabelData.Text = GetDataForTable(textFromFile, numberofrows, LabelGotUid.Text);
            GVDataTypes.Visible = false;
            GVTableUploaded.Visible = true;
            ButtonInsert.Visible = false;
            ButtonGetUrl.Visible = false;
            ButtonCancel.Visible = true;
            ButtonCancel.Text = "Back";
            SqlTableUploaded.SelectCommand = "Select * from [" + LabelGotUid.Text +"]";
            Page.DataBind();
        }
    }
}