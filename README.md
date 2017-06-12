# MailAware
Watches a mailbox to ensure you are getting mail on time and alerts if not.

---
In order to run this, ensure you have the latest .NET Core (>= 1.1.0) from [here](https://www.microsoft.com/net/download/core#/current).


Ensure you have a configuration under "src/MailAware.Console" named **config.json** such as the sample below:

```
{
  "targetMailServers": [
    {
      "hostAddress": "mail.example.com",
      "username": "someuser",
      "password": "somepass",
      "targetSubjectSnippet": "Status Report",
      "alarmThresholdSecs":  1800,
	  "displayName": "Mailbox 1"
    }
  ],
  "notificationMailServer": {
    "hostAddress": "mail.example.com",
    "username": "someuser",
    "password": "somepass",
    "subjectPrefix": "MailAware Alert: ",
    "fromAddress": "alerts@example.com",
    "recipients": [
      "address@example.com"
    ]
  }
}
```

```
cd src/MailAware.Console
dotnet restore
dotnet build
dotnet run
```
