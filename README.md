# AuthenJWTLab
* This is a login/logout sys with JWT Authentication.
* Using Authorization to limit api usage.
* Also could use Swagger and Postman to test Api.

## Http status feedback
* ordinary
  1. 500 something occur at server.
  2. 401 not login or access token expired or Invalid.
  3. 403 not have authorization.
* refresh token Api
  1. 400 access token Invalid.
  2. 401 refresh token Invalid.
