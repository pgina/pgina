## About

MySQL provides connectivity for client applications developed in .NET compatible programming languages with MySQL Connector/NET through a series of packages.

MySql.Data is the core package of Connector/NET. It is compatible with .NET Framework 4.6+ and .NET 8.0+ and provides classic MySQL protocol and MySQL X DevAPI capabilities.

More information at [MySQL Connector/NET documentation](https://dev.mysql.com/doc/connector-net/en/).

## How to use

```
    MySql.Data.MySqlClient.MySqlConnection myConnection;
    string myConnectionString;
    //set the correct values for your server, user, password and database name
    myConnectionString = "server=127.0.0.1;uid=root;pwd=12345;database=test";

    try
    {
      myConnection = new MySql.Data.MySqlClient.MySqlConnection(myConnectionString);
      //open a connection
      myConnection.Open();

      // create a MySQL command and set the SQL statement with parameters
      MySqlCommand myCommand = new MySqlCommand();
      myCommand.Connection = myConnection;
      myCommand.CommandText = @"SELECT * FROM clients WHERE client_id = @clientId;";
      myCommand.Parameters.AddWithValue("@clientId", clientId);

      // execute the command and read the results
      using var myReader = myCommand.ExecuteReader()
      {
        while (myReader.Read())
        {
          var id = myReader.GetInt32("client_id");
          var name = myReader.GetString("client_name");
          // ...
        }
      }
      myConnection.Close();
    }
    catch (MySql.Data.MySqlClient.MySqlException ex)
    {
      MessageBox.Show(ex.Message);
    }
```

## Related Packages

* Entity Framework Core: [MySql.EntityFrameworkCore](https://www.nuget.org/packages/MySql.EntityFrameworkCore/)
* Entity Framework: [MySql.Data.EntityFramework](https://www.nuget.org/packages/MySql.Data.EntityFramework/)
* Web: [MySql.Web](https://www.nuget.org/packages/MySql.Web/)
* OpenTelemetry: [MySql.Data.OpenTelemetry](https://www.nuget.org/packages/MySql.Data.OpenTelemetry/)

## Licensing

Please refer to files [README](https://github.com/mysql/mysql-connector-net/blob/-/README) and [LICENSE](https://github.com/mysql/mysql-connector-net/blob/-/LICENSE), available in the Connector/NET GitHub repository, and [Legal Notices in documentation](https://dev.mysql.com/doc/connector-net/en/preface.html) for further details.

## Security

Oracle values the independent security research community and believes that responsible disclosure of security vulnerabilities helps us ensure the security and privacy of all our users. Please refer to the [security guidelines](https://github.com/mysql/mysql-connector-net/blob/-/SECURITY.md) document for additional information.

## Contributing

We greatly appreciate feedback from our users, including bug reports and code contributions. Your input helps us improve, and we thank you for any issues you report or code you contribute. Please refer to the [contributing guidelines](https://github.com/mysql/mysql-connector-net/blob/-/CONTRIBUTING.md) document for additional information.

### Additional Resources

* [MySQL Connector/NET GitHub](https://github.com/mysql/mysql-connector-net/)
* [MySQL Connector/NET Developer Guide](https://dev.mysql.com/doc/connector-net/en/)
* [MySQL Connector/NET API](https://dev.mysql.com/doc/dev/connector-net/latest/)
* [MySQL NuGet](https://www.nuget.org/profiles/MySQL/)
* [MySQL Connector/NET and C#, Mono, .Net Forum](https://forums.mysql.com/list.php?38)
* [`#connectors` channel on MySQL Community Slack](https://mysqlcommunity.slack.com/messages/connectors/)  ([Sign-up](https://lefred.be/mysql-community-on-slack/) required if you do not have an Oracle account.)
* [@MySQL on X](https://x.com/MySQL/).
* [MySQL Blog](https://blogs.oracle.com/mysql/).
* [MySQL Connectors Blog archive](https://dev.mysql.com/blog-archive/?cat=Connectors%20%2F%20Languages).
* [MySQL Newsletter](https://www.mysql.com/news-and-events/newsletter/).
* [MySQL Bugs Tracking System](https://bugs.mysql.com).

For more information about this and other MySQL products, please visit [MySQL Contact & Questions](https://www.mysql.com/about/contact/).
