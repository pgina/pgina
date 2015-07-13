# HTTP auth plugin

This plugin grabs user credentials and send http(s) request.
What is it good for? Well, you can let all logon implementation details to a http(s) server.
It does not absolutely matter if the server is written in java, .net (fffufuu:) or python or node.js.
Everything it shall do is to decide if a pair of username and password can be logged in.
The request shall be application/json encoded POST request.
Response (HR) to such request have status:
-  200 and contains all info about given user. For details see [UInfo.cs](HttpAuth/UInfo.cs)
-  400 <Error message usefully (for non programmers, please :) explaining why error occured>

If response contains groups that exists locally (e.g. 'administrators'),
logged user will be added to that groups.

Example of successful login response body (note the first blank line indicating there is no obstacle to logon):
```

gandalf
Gandalg The Gray
g@ndalf.shire
administrators;lecturers;wizards
```
Will login gandalf that will then be among local admins
(and lecturers and wizards if they exists as local groups).


## implementation details

The HR is cached in auth stage for processing within following stages.
Configuration of URL where to send requests (LP - login endpoint) can be done with:

- setting environment variable PGINALOGINSERVER with desired URL (overrides all)
- setup DNS server to give TXT record named 'pginaloginserver' containing the LP
- DEFAULT hardcoded value is 'http://pginaloginserver/login'.
  Only configurable part is to let DNS point to right IP with pginaloginserver
  and make sure that login app will listen on /login path on that IP.
