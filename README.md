# Network Resource Monitor
Open source solution for network resource monitoring

All settings are available via the file options.json



![Screenshot](NetworkStateMonitoringScr.jpg)

## options.json
```
{
  "CheckingIntervalMilliseconds": 60000,

  // Domain admin credentials,
  // IMPORTANT! This user have to be a local admin on every PC that need to be controlled
  "SystemUserLogin": "domainAdminLogin",
  "SystemUserPassword": "domainAdminPassword",

  "MinFreeDiskSpaceInPercent": 10,
  "MinFreeDiskSpaceInMb": 1024,

  "TelegramRequest": "https://api.telegram.org/bot000000000:00000000000000000000-00000000000000/sendMessage?chat_id=-000000000&text=",
  // bot000000000 - bot id
  // 00000000000000000000-00000000000000 - token
  // -000000000 - chat id
  // It's just sample request. In our case we used intermediate service
  // to prevent messages from being sent at night, but accumulating and send them the next morning at the scheduled time
  // Additionally we used Socks5 proxy server


  "ServerLogicGroups": [
    {
      "Id": "group1",
      "Caption": "Group 1 Caption"
    },

    {
      "Id": "group2",
      "Caption": "Group 2 Caption",
      "IsSpesialBlockForWebsites": true, // Set this property in true if this block supposed to monitor website
      "Sites": [
        {
          "Id": "block1",
          "Url": "http://www.yourSite.com",
          "Name": "Block 1 Name",
          "HtmlToControl": "id=\"Login\" name=\"Login\"", // Some html-tags on web page to detect that site is available
        },
      ]
    },
  ],

  "ServerDatas": [

    {
      "Id": "block1",
      "HostName": "server1",
      "LogicGroupId": "group1", // Group Id from block abowe
      "WindowsServicesNames": [
        "MSExchangePop3", // Some service name to control
      ],
      "DatabasesIgnoreList": [
        "dataBaseName"
      ],
      "SqlServerLogin": "yourSqlLogin",
      "SqlServerPassword": "yourSqlpassword",
      "IsNesseseryToCheckSqlBases": true // if set in true, then all data bases of this server will be monitored (except items specified in DatabasesIgnorList)
    },
  ]
}
```
