using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Westwind.Utilities.Logging;

namespace Westwind.Utilities.Logging
{

    public class XmlLogAdapter : XmlLogAdapter<LogEntry>
    {
    }



    public class XmlLogAdapter<T> : ILogAdapter<T> where T : LogEntry, new()
    {

        public XmlLogAdapter()
        {
            ConnectionString = LogManagerConfiguration.Current.LogFilename;
            if (string.IsNullOrEmpty(ConnectionString))
                ConnectionString = LogManagerConfiguration.Current.ConnectionString;
        }

        /// <summary>
        /// The Xml Connection string is the filename
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                _connectionString = value;
            }
        }
        private string _connectionString;

        /// <summary>
        /// The name of the file where we're logging to
        /// </summary>
        public string Filename
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

#if NET10_0_OR_GREATER
        private static Lock _writeLock = new();
#else
        private static object _writeLock = new object();
#endif


        /// <summary>
        /// Writes an entry to the log
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public bool WriteEntry(T entry)
        {
            lock (_writeLock)
            {             
                string logFilename = ConnectionString;
                bool writeEndDoc = true;
                FileStream fileStream = null;
                try
                {
                    fileStream = new FileStream(logFilename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
                    fileStream.Seek(0, SeekOrigin.End);

                    // *** If the file's not empty start writing over the end doc tag
                    // *** We'll rewrite it at the end
                    if (fileStream.Position > 15)
                    {
                        try
                        {
                            fileStream.Seek(-1 * "</ApplicationLog>\r\n".Length, SeekOrigin.End);
                        }
                        catch { }
                    }
                }
                catch
                {
                    return false;
                }

                XmlTextWriter writer = new XmlTextWriter((Stream)fileStream, Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                writer.IndentChar = ' ';
                writer.Indentation = 4;

                // *** If the file is empty write the root element
                if (fileStream.Position == 0)
                {
                    writer.WriteStartElement("ApplicationLog");
                    writeEndDoc = false; // it'll automatically unwind the StartElement
                }

                writer.WriteStartElement("LogEntry");
                writer.WriteElementString("Id", entry.Id.ToString());

                writer.WriteStartElement("Entered");
                writer.WriteValue(entry.Entered);
                writer.WriteEndElement();

                writer.WriteElementString("Message", entry.Message);
                writer.WriteElementString("ErrorLevel", entry.ErrorLevel.ToString());
                writer.WriteElementString("Details", entry.Details);


                if (entry.Web != null)
                {
                    writer.WriteStartElement("Web");

                    writer.WriteElementString("Url", entry.Web.Url);
                    writer.WriteElementString("QueryString", entry.Web.QueryString);
                    writer.WriteElementString("UserAgent", entry.Web.UserAgent);
                    writer.WriteElementString("Referrer", entry.Web.Referrer);
                    writer.WriteElementString("PostData", entry.Web.PostData);
                    writer.WriteElementString("IpAddress", entry.Web.IpAddress);
                    writer.WriteElementString("RequestDuration", entry.Web.RequestDuration.ToString());

                    writer.WriteEndElement(); // Web
                }

                writer.WriteEndElement(); // error


                if (writeEndDoc)
                    writer.WriteRaw("\r\n</ApplicationLog>\r\n");
                else
                {
                    writer.WriteEndElement();
                    writer.WriteRaw("\r\n");
                }

                writer.Close();
                fileStream.Close();

                return true;
            }
        }


        /// <summary>
        /// This method is provided for async logging, but it's not 
        /// truly async adn simply wraps in a `Task.Run()`. It works
        /// to offload but isn't truly IO async due to the file locking
        /// requirements.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public Task<bool> WriteEntryAsync(T entry)
        {
            // offload from main processing pipeline
            return Task.Run(() => WriteEntry(entry));
        }

        /// <summary>
        /// Returns an individual entry entity
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetEntry(string id)
        {
            XElement doc = XElement.Load(Filename);
            XElement match = doc
                .Descendants("LogEntry")
                .FirstOrDefault(el => (string)el.Element("Id") == id);

            var entry = new T();
            return UpdateEntryFromElement<T>(match, entry);            
        }

    

        /// <summary>
        /// Returns a filtered list of XML entries sorted in descending order.
        /// </summary>
        /// <param name="errorLevel">The specific error level to return</param>
        /// <param name="count">Max number of items to return</param>
        /// <param name="dateFrom">From Date</param>
        /// <param name="dateTo">To Date</param>
        /// <param name="fieldList">"*" or any of the fields you want returned - currently not supported</param>
        /// <returns></returns>
        public IEnumerable<T> GetEntries(ErrorLevels errorLevel, int count, DateTime? dateFrom, DateTime? dateTo, string fieldList) 
        {
            if (dateFrom == null)
                dateFrom = DateTime.Now.Date.AddDays(-2);
            if (dateTo == null)
                dateTo = DateTime.Now.Date.AddDays(1);

            XElement doc = XElement.Load(Filename);
            var res = doc.Descendants("LogEntry");

            string elevel = errorLevel.ToString();

            if (errorLevel != ErrorLevels.All)
                res = res.Where(el => (string)el.Element("ErrorLevel") == elevel);

            res = res.Take(count)
                     .OrderByDescending(el => (DateTime)el.Element("Entered"));

            var logEntry = new T();
            var list = new List<T>();

            foreach (XElement node in res)
            {

                UpdateEntryFromElement(node, logEntry);

                list.Add(logEntry);
            }

            return list;
        }



        /// <summary>
        /// Does nothing for the XmlLogAdapter - log is created with first new entry instead
        /// </summary>
        /// <param name="logType"></param>
        /// <returns></returns>
        public bool CreateLog()
        {
            return true;
        }

        /// <summary>
        /// Deletes the XML log file
        /// </summary>
        /// <returns></returns>
        public bool DeleteLog()
        {
            try
            {
                File.Delete(Filename);
                return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Clears out all items from the XML log - in effect deletes the log file.
        /// </summary>
        /// <returns></returns>
        public bool Clear()
        {
            return DeleteLog();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="countToLeave"></param>
        /// <returns></returns>
        public bool Clear(int countToLeave)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="daysToDelete"></param>
        /// <returns></returns>
        public bool Clear(decimal daysToDelete)
        {
            throw new NotImplementedException();
        }

        public int GetEntryCount(ErrorLevels errorLevel = ErrorLevels.All)
        {
            throw new NotFiniteNumberException();
        }


        /// <summary>
        /// Updates the DataRow with data from node passed in
        /// </summary>
        /// <param name="node">Xml root node for LogEntry</param>        
        /// <param name="entry">Log entry object to assign to</param>
        private T UpdateEntryFromElement<T>(XElement node, T entry) where T : LogEntry, new()
        {
            entry.Id = node.Element("Id")?.ToString();
            entry.Entered = (DateTime)node.Element("Entered");
            entry.Message = (string)node.Element("Message");
            entry.ErrorLevel = (ErrorLevels)Enum.Parse(typeof(ErrorLevels), (string)node.Element("ErrorLevel"));

            entry.Details = (string)node.Element("Details");


            var url = (string)node.Element("Url");
            if (string.IsNullOrEmpty(url))
                return entry;
            
            entry.Web.Url = url;
            entry.Web.QueryString = node.Element("QueryString")?.Value;
            entry.Web.UserAgent = node.Element("UserAgent")?.Value;
            entry.Web.Referrer = node.Element("Referrer")?.Value;
            entry.Web.PostData = node.Element("PostData")?.Value;
            entry.Web.IpAddress =node.Element("IpAddress")?.Value;

            return entry;
        }
    }
}
