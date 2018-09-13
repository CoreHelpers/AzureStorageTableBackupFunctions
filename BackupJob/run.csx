using System;
using CoreHelpers.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

public class BackupStorageLogger : DefaultStorageLogger
{
    private ILogger logger { get; set; }
    
    public BackupStorageLogger(ILogger logger) 
    {
        this.logger = logger;
    }

    public override void LogInformation(string text)
    {
        this.logger.LogInformation(text);
    }
}

public static void Run(TimerInfo backupTimer, ILogger log)
{    
    // Welcome Message
    log.LogInformation("Backup Client for Azure Storage Account Tables");
    log.LogInformation("");

    // Check the parameter 
    ValidateParameter("SRC_ACCOUNT_NAME", log);
    ValidateParameter("SRC_ACCOUNT_KEY", log);
    ValidateParameter("TGT_ACCOUNT_KEY", log);
    ValidateParameter("TGT_ACCOUNT_KEY", log);
    ValidateParameter("TGT_ACCOUNT_CONTAINER", log);

    var srcAccountName = Environment.GetEnvironmentVariable("SRC_ACCOUNT_NAME");
    var srcAccountKey = Environment.GetEnvironmentVariable("SRC_ACCOUNT_KEY");
    var srcAccountEndpointSuffix = Environment.GetEnvironmentVariable("SRC_ACCOUNT_ENDPOINT_SUFFIX");

    var tgtAccountName = Environment.GetEnvironmentVariable("TGT_ACCOUNT_NAME");
    var tgtAccountKey = Environment.GetEnvironmentVariable("TGT_ACCOUNT_KEY");
    var tgtAccountContainer = Environment.GetEnvironmentVariable("TGT_ACCOUNT_CONTAINER");
    var tgtAccountEndpointSuffix = Environment.GetEnvironmentVariable("TGT_ACCOUNT_ENDPOINT_SUFFIX");

    // Information 
    log.LogInformation($"           Account Name: {srcAccountName}");
    log.LogInformation($"Account Endpoint-Suffix: {srcAccountEndpointSuffix}");

    // instantiate the logger
    var logger = new BackupStorageLogger(log);

    // build a storage context 
    using (var storageContext = new StorageContext(srcAccountName, srcAccountKey, srcAccountEndpointSuffix))
    {
        // build the cloud account 
        var backupStorageAccount = new CloudStorageAccount(new StorageCredentials(tgtAccountName, tgtAccountKey), tgtAccountEndpointSuffix, true);

        // instantiate the backup service 
        var backupService = new BackupService(storageContext, backupStorageAccount, logger);

        // build the backup prefix 
        var prefix = DateTime.Now.ToString("yyyy-mm-dd") + "-" + Guid.NewGuid().ToString();

        // exceute the backup 
        backupService.Backup(tgtAccountContainer, prefix).Wait();
    }

    // Thank you 
    log.LogInformation("Backup is finished");            
}

static void ValidateParameter(string variable, ILogger log) {

    var value = Environment.GetEnvironmentVariable(variable);
    if (String.IsNullOrEmpty(value))
    {
        var msg = $"ERROR: Missing {variable} environment variable";        
        log.LogInformation(msg);        
        throw new Exception(msg);
    }
}