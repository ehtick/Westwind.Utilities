using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Westwind.Utilities.Logging;

namespace Westwind.Utilities.Test;

[TestClass]
public class LoggingTests
{

    [TestMethod]
    public void TextLoggerTest()
    {

        var adapter = new TextLogAdapter()
        {
            Filename = "testlog.txt"
        };
        var manager = LogManager.Create(adapter);

        bool result = manager.LogInfo("Test Log Entry");
        Assert.IsTrue(result, "Log entry was not written successfully.");

        result = manager.LogError("This is an error");
        Assert.IsTrue(result, "Log entry was not written successfully.");


        result = manager.LogWarning("This is a warning");
        Assert.IsTrue(result, "Log entry was not written successfully.");



        ShellUtils.GoUrl(Path.GetFullPath(adapter.Filename));
    }



    [TestMethod]
    public void TextLoggerCompactTest()
    {

        var adapter = new TextLogAdapter()
        {
            Filename = "testlogCompact.txt",
            UseCompactLogging = true
        };
        var manager = LogManager.Create(adapter);

        bool result = manager.LogInfo("Test Log Entry");
        Assert.IsTrue(result, "Log entry was not written successfully.");

        result = manager.LogError("This is an error");
        Assert.IsTrue(result, "Log entry was not written successfully.");


        result = manager.LogWarning("This is a warning");
        Assert.IsTrue(result, "Log entry was not written successfully.");



        ShellUtils.GoUrl(Path.GetFullPath(adapter.Filename));
    }


    [TestMethod]
    public void XmlLoggerTest()
    {

        var adapter = new XmlLogAdapter()
        {
            Filename = "testlog.xml"
        };
        var manager = LogManager.Create(adapter);

        manager.LogInfo("Test Log Entry");
        bool result = manager.LogError("This is an error");


        Assert.IsTrue(result, "Log entry was not written successfully.");

        //ShellUtils.GoUrl(Path.GetFullPath(adapter.Filename));
    }

    [TestMethod]
    public void CreateSqlLogDbTest()
    {
        var adapter = new SqlLogAdapter(
            TestConfigurationSettings.WestwindToolkitConnectionString ?? "Data Source=.;Initial Catalog=WestwindToolkitSamples;Integrated Security=True",
            "ApplicationLog");

        var manager = LogManager.Create(adapter);
        bool result = manager.CreateLog("ApplicationLog");
        Assert.IsTrue(result, "Log was not created successfully.");
    }

    [TestMethod]
    public void WriteSqlLogTest()
    {
        var adapter = new SqlLogAdapter()
        {
            Tablename = "ApplicationLog",
            ConnectionString = TestConfigurationSettings.WestwindToolkitConnectionString ?? "Data Source=.;Initial Catalog=WestwindToolkitSamples;Integrated Security=True"
        };
        var manager = LogManager.Create(adapter);
        bool result = manager.LogInfo("Info message posted");
        
        Assert.IsTrue(result, "Log was not created successfully.");
    }



    [TestMethod]
    public void GetSqlEntryTest()
    {
        var adapter = new SqlLogAdapter()
        {
            Tablename = "ApplicationLog",
            ConnectionString = TestConfigurationSettings.WestwindToolkitConnectionString ?? "Data Source=.;Initial Catalog=WestwindToolkitSamples;Integrated Security=True"
        };
        var manager = LogManager.Create(adapter);
        var entry = manager.GetEntry("4ud7meegda");

        Assert.IsNotNull(entry, "Log entry was not found.");
    }


}