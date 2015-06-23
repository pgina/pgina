# HTTP auth plugin

This plugin grabs user credentials and send http(s) request.
Response (HR) to such request is either:
-  200 OK and contains all info about given user. For details see UInfo.cs
-  400 <Error message>

If response contains groups that exists locally (e.g. 'administrators'),
logged user will be added to that groups.

## implementation details

The HR is cached in auth stage for processing within following stages.
Configuration of URL where to send requests can be done like:

- set environment variable PGINALOGINSERVER with desired URL (overrides all)
- setup DNS server to give TXT record named 'pginaloginserver' containing desired URL
- DEFAULT hardcoded value is 'http://pginaloginserver/login'.
  Only configurable part is to let DNS point to right IP with pginaloginserver
